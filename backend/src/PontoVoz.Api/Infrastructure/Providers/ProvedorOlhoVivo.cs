using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Options;

using PontoVoz.Api.Application.Interfaces;
using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.ValueObjects;

namespace PontoVoz.Api.Infrastructure.Providers;

/// <summary>
/// Adaptador da API Olho Vivo da SPTrans, versão 2.1 (requisito R8). <b>Desligado por padrão</b> e
/// <b>nunca testado contra a API real</b>: os testes usam um <see cref="HttpMessageHandler"/> falso.
/// <para>
/// Segue a documentação oficial: autenticação por <c>POST Login/Autenticar?token=</c> (resposta
/// <c>true</c> ou <c>false</c>) e previsão por <c>GET Previsao/Parada?codigoParada=</c>, com
/// <c>p.l[].cl</c> (código da linha) e <c>p.l[].vs[]</c> (veículos: <c>p</c> = prefixo,
/// <c>t</c> = horário previsto <c>HH:mm</c>). A sessão por cookie depois do login não está na
/// página oficial (vem de fontes de terceiros); por isso o adaptador autentica a cada consulta e
/// guarda o cookie no handler.
/// </para>
/// Nunca envia dados <c>SIM-</c> nem códigos sem correspondência oficial declarada. Qualquer falha
/// (autenticação recusada, prazo esgotado, conexão, HTTP sem sucesso, JSON inesperado) vira
/// <see cref="ResultadoProvedor.Indisponivel"/>; só o cancelamento do cliente é propagado. O token
/// não aparece em logs nem em erros.
/// </summary>
public sealed partial class ProvedorOlhoVivo(HttpClient http, IOptionsMonitor<OpcoesPrevisoes> opcoes, ILogger<ProvedorOlhoVivo> logger) : IProvedorPrevisaoChegada
{
    private static readonly TimeZoneInfo FusoSaoPaulo = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    [GeneratedRegex(@"\A[A-Za-z0-9-]{1,20}\z")]
    private static partial Regex FormatoPrefixo();

    public OrigemDado OrigemPara(Parada parada) => OrigemDado.OlhoVivo;

    /// <summary>Dado de demonstração: nunca vai para a SPTrans (R8).</summary>
    public static bool EhSimulado(Parada parada) =>
        parada.DadosSimulados || parada.CodigoExterno.StartsWith("SIM-", StringComparison.OrdinalIgnoreCase);

    public static bool EhSimulada(Linha linha) =>
        linha.DadosSimulados || linha.Codigo.StartsWith("SIM-", StringComparison.OrdinalIgnoreCase);

    public async Task<ResultadoProvedor> ConsultarAsync(Parada parada, IReadOnlyList<Linha> linhas, DateTimeOffset referencia, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(parada);
        ArgumentNullException.ThrowIfNull(linhas);
        OpcoesOlhoVivo configuracao = opcoes.CurrentValue.OlhoVivo;

        if (!configuracao.PodeConsultar)
        {
            logger.LogWarning("Olho Vivo desabilitado ou sem token configurado; nenhuma chamada foi feita.");
            return Indisponivel(referencia);
        }

        if (EhSimulado(parada) || !configuracao.Paradas.TryGetValue(parada.CodigoExterno, out int codigoParada))
        {
            logger.LogWarning("Parada {Codigo} sem código oficial declarado (ou de demonstração); nenhuma chamada foi feita.", parada.CodigoExterno);
            return Indisponivel(referencia);
        }

        // Só as linhas com código oficial declarado; as de demonstração nunca são enviadas.
        Dictionary<int, Linha> porCodigoOficial = [];
        foreach (Linha linha in linhas.Where(linha => !EhSimulada(linha)))
        {
            if (configuracao.Linhas.TryGetValue(linha.Codigo, out int cl))
            {
                porCodigoOficial[cl] = linha;
            }
        }

        if (porCodigoOficial.Count == 0)
        {
            logger.LogWarning("Nenhuma linha da parada {Codigo} tem código oficial declarado; nenhuma chamada foi feita.", parada.CodigoExterno);
            return Indisponivel(referencia);
        }

        using var prazo = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        prazo.CancelAfter(configuracao.Prazo);
        try
        {
            if (!await AutenticarAsync(configuracao.Token!, prazo.Token))
            {
                logger.LogWarning("Olho Vivo recusou a autenticação.");
                return Indisponivel(referencia);
            }

            using HttpResponseMessage resposta = await http.GetAsync(
                new Uri($"Previsao/Parada?codigoParada={codigoParada.ToString(CultureInfo.InvariantCulture)}", UriKind.Relative), prazo.Token);
            if (!resposta.IsSuccessStatusCode)
            {
                logger.LogWarning("Olho Vivo respondeu {Status} na consulta de previsões.", (int)resposta.StatusCode);
                return Indisponivel(referencia);
            }

            await using Stream corpo = await resposta.Content.ReadAsStreamAsync(prazo.Token);
            using JsonDocument json = await JsonDocument.ParseAsync(corpo, cancellationToken: prazo.Token);
            List<PrevisaoChegada>? previsoes = Interpretar(json.RootElement, porCodigoOficial, referencia);
            if (previsoes is null)
            {
                logger.LogWarning("Olho Vivo respondeu num formato inesperado.");
                return Indisponivel(referencia);
            }

            return ResultadoProvedor.Respondido(OrigemDado.OlhoVivo, referencia, previsoes);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // O cliente desistiu: não é indisponibilidade do provedor.
            throw;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Olho Vivo não respondeu em {Segundos} s.", configuracao.Prazo.TotalSeconds);
            return Indisponivel(referencia);
        }
        catch (HttpRequestException erro)
        {
            logger.LogWarning("Falha de conexão com a Olho Vivo ({Erro}).", erro.HttpRequestError);
            return Indisponivel(referencia);
        }
        catch (JsonException)
        {
            logger.LogWarning("Olho Vivo respondeu com JSON inválido.");
            return Indisponivel(referencia);
        }
    }

    /// <summary>
    /// Converte o horário <c>HH:mm</c> da SPTrans (São Paulo) num instante, no dia mais próximo da
    /// referência: perto da meia-noite, "00:05" consultado às 23h58 é do dia seguinte.
    /// </summary>
    public static DateTimeOffset InstanteDoHorario(TimeSpan horario, DateTimeOffset referencia)
    {
        DateTimeOffset local = TimeZoneInfo.ConvertTime(referencia, FusoSaoPaulo);
        DateTimeOffset candidato = NoDia(local.Date, horario);
        if (candidato < local.AddHours(-12))
        {
            candidato = NoDia(local.Date.AddDays(1), horario);
        }
        else if (candidato > local.AddHours(12))
        {
            candidato = NoDia(local.Date.AddDays(-1), horario);
        }

        return candidato.ToUniversalTime();
    }

    private static DateTimeOffset NoDia(DateTime dia, TimeSpan horario)
    {
        DateTime momento = DateTime.SpecifyKind(dia + horario, DateTimeKind.Unspecified);
        return new DateTimeOffset(momento, FusoSaoPaulo.GetUtcOffset(momento));
    }

    private async Task<bool> AutenticarAsync(string token, CancellationToken cancellationToken)
    {
        // O token vai na query, como manda a documentação oficial, e só por HTTPS. Os loggers do
        // HttpClient foram retirados no registro para que a URL com o token não vá para o log.
        using var conteudo = new StringContent(string.Empty);
        using HttpResponseMessage resposta = await http.PostAsync(
            new Uri($"Login/Autenticar?token={Uri.EscapeDataString(token)}", UriKind.Relative), conteudo, cancellationToken);
        if (!resposta.IsSuccessStatusCode)
        {
            return false;
        }

        string corpo = await resposta.Content.ReadAsStringAsync(cancellationToken);
        return bool.TryParse(corpo.Trim(), out bool autenticado) && autenticado;
    }

    /// <summary>
    /// Previsões das linhas pedidas, ou <c>null</c> se o formato não é o documentado. Um veículo sem
    /// horário válido é ignorado (nada é inventado); um prefixo fora do formato vira <c>null</c>.
    /// </summary>
    private static List<PrevisaoChegada>? Interpretar(JsonElement raiz, Dictionary<int, Linha> linhas, DateTimeOffset referencia)
    {
        if (raiz.ValueKind != JsonValueKind.Object || !raiz.TryGetProperty("p", out JsonElement parada))
        {
            return null;
        }

        var previsoes = new List<PrevisaoChegada>();
        if (parada.ValueKind == JsonValueKind.Null)
        {
            return previsoes;
        }

        if (parada.ValueKind != JsonValueKind.Object
            || !parada.TryGetProperty("l", out JsonElement listaLinhas)
            || listaLinhas.ValueKind is not (JsonValueKind.Array or JsonValueKind.Null))
        {
            return null;
        }

        if (listaLinhas.ValueKind == JsonValueKind.Null)
        {
            return previsoes;
        }

        foreach (JsonElement linha in listaLinhas.EnumerateArray())
        {
            if (linha.ValueKind != JsonValueKind.Object
                || !linha.TryGetProperty("cl", out JsonElement cl)
                || cl.ValueKind != JsonValueKind.Number
                || !cl.TryGetInt32(out int codigoLinha))
            {
                return null;
            }

            if (!linhas.TryGetValue(codigoLinha, out Linha? nossa)
                || !linha.TryGetProperty("vs", out JsonElement veiculos)
                || veiculos.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            var referenciaLinha = ReferenciaLinha.De(nossa);
            foreach (JsonElement veiculo in veiculos.EnumerateArray())
            {
                if (veiculo.ValueKind != JsonValueKind.Object
                    || !veiculo.TryGetProperty("t", out JsonElement t)
                    || t.ValueKind != JsonValueKind.String
                    || !TimeSpan.TryParseExact(t.GetString(), @"hh\:mm", CultureInfo.InvariantCulture, out TimeSpan horario))
                {
                    continue;
                }

                previsoes.Add(new PrevisaoChegada(referenciaLinha, InstanteDoHorario(horario, referencia), Prefixo(veiculo)));
            }
        }

        return previsoes;
    }

    private static string? Prefixo(JsonElement veiculo)
    {
        if (!veiculo.TryGetProperty("p", out JsonElement p))
        {
            return null;
        }

        string? texto = p.ValueKind switch
        {
            JsonValueKind.String => p.GetString(),
            JsonValueKind.Number => p.GetRawText(),
            _ => null,
        };
        return texto is not null && FormatoPrefixo().IsMatch(texto) ? texto : null;
    }

    private static ResultadoProvedor Indisponivel(DateTimeOffset referencia) =>
        ResultadoProvedor.Indisponivel(OrigemDado.OlhoVivo, referencia);
}

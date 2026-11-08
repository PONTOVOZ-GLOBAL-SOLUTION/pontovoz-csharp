using Microsoft.Extensions.Options;

using PontoVoz.Api.Application.Interfaces;
using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.ValueObjects;

namespace PontoVoz.Api.Infrastructure.Providers;

/// <summary>
/// Configuração de demonstração de uma linha simulada (requisito R1): os ônibus passam a cada
/// <see cref="IntervaloMinutos"/>, numa grade contada em minutos desde 01/01/1970 00:00 UTC e
/// deslocada por <see cref="DefasagemMinutos"/>. A viagem número <c>k</c> chega em
/// <c>1970-01-01T00:00Z + DefasagemMinutos + k × IntervaloMinutos</c> e é feita pelo veículo
/// <c>Prefixos[k mod Prefixos.Count]</c>. Como a âncora é fixa e a conta é em minutos corridos,
/// a mesma regra vale em qualquer data, inclusive na virada do dia e do mês e depois de reiniciar a API.
/// </summary>
public sealed record LinhaSimulada(string CodigoLinha, int IntervaloMinutos, int DefasagemMinutos, int QuantidadeChegadas, IReadOnlyList<string> Prefixos);

/// <summary>
/// Provedor simulado (padrão): calcula as próximas chegadas das linhas de demonstração no
/// instante da consulta, sem gravar nada. As previsões são sempre futuras (ou exatamente no
/// instante da consulta) e saem com origem <c>Simulado</c> e nível <c>Previsao</c>. Linhas fora
/// de <see cref="LinhasDemonstracao"/> ficam sem previsão: nada é inventado para elas.
/// </summary>
public sealed class ProvedorPrevisaoSimulado(IOptionsMonitor<OpcoesPrevisoes> opcoes) : IProvedorPrevisaoChegada
{
    private static readonly DateTimeOffset Ancora = DateTimeOffset.UnixEpoch;

    /// <summary>
    /// Linhas com previsão simulada, pelo código estável (nunca pelo id interno). Os valores
    /// reproduzem o exemplo do contrato às 14h32 de 10/11/2026 (SIM-L002 às 14h33 e 14h51,
    /// SIM-L004 às 14h40) sem guardar essa data: em qualquer outro instante, a mesma grade continua.
    /// SIM-L001 fica de propósito sem configuração, para a demonstração da previsão parcial.
    /// </summary>
    public static IReadOnlyDictionary<string, LinhaSimulada> LinhasDemonstracao { get; } = new Dictionary<string, LinhaSimulada>(StringComparer.Ordinal)
    {
        ["SIM-L002"] = new("SIM-L002", IntervaloMinutos: 18, DefasagemMinutos: 9, QuantidadeChegadas: 2, ["SIM-V2001", "SIM-V2002", "SIM-V2003"]),
        ["SIM-L003"] = new("SIM-L003", IntervaloMinutos: 20, DefasagemMinutos: 7, QuantidadeChegadas: 2, ["SIM-V3001"]),
        ["SIM-L004"] = new("SIM-L004", IntervaloMinutos: 15, DefasagemMinutos: 10, QuantidadeChegadas: 1, ["SIM-V4001", "SIM-V4002"]),
    };

    public OrigemDado OrigemPara(Parada parada) => OrigemDado.Simulado;

    public Task<ResultadoProvedor> ConsultarAsync(Parada parada, IReadOnlyList<Linha> linhas, DateTimeOffset referencia, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(parada);
        ArgumentNullException.ThrowIfNull(linhas);
        cancellationToken.ThrowIfCancellationRequested();

        if (opcoes.CurrentValue.SimularIndisponibilidade)
        {
            return Task.FromResult(ResultadoProvedor.Indisponivel(OrigemDado.Simulado, referencia));
        }

        var previsoes = new List<PrevisaoChegada>();
        foreach (Linha linha in linhas)
        {
            if (LinhasDemonstracao.TryGetValue(linha.Codigo, out LinhaSimulada? configuracao))
            {
                previsoes.AddRange(ProximasChegadas(configuracao, ReferenciaLinha.De(linha), referencia));
            }
        }

        return Task.FromResult(ResultadoProvedor.Respondido(OrigemDado.Simulado, referencia, previsoes));
    }

    /// <summary>As próximas chegadas da grade a partir de <paramref name="referencia"/> (inclusive).</summary>
    public static IEnumerable<PrevisaoChegada> ProximasChegadas(LinhaSimulada configuracao, ReferenciaLinha linha, DateTimeOffset referencia)
    {
        ArgumentNullException.ThrowIfNull(configuracao);
        long intervaloTicks = TimeSpan.FromMinutes(configuracao.IntervaloMinutos).Ticks;
        long defasagemTicks = TimeSpan.FromMinutes(configuracao.DefasagemMinutos).Ticks;
        long decorrido = referencia.UtcTicks - Ancora.UtcTicks - defasagemTicks;

        // Primeira viagem com horário >= referência: divisão inteira arredondada para cima.
        long viagem = (long)Math.Ceiling((decimal)decorrido / intervaloTicks);
        for (int i = 0; i < configuracao.QuantidadeChegadas; i++, viagem++)
        {
            var chegada = new DateTimeOffset(Ancora.UtcTicks + defasagemTicks + (viagem * intervaloTicks), TimeSpan.Zero);
            string prefixo = configuracao.Prefixos[(int)(((viagem % configuracao.Prefixos.Count) + configuracao.Prefixos.Count) % configuracao.Prefixos.Count)];
            yield return new PrevisaoChegada(linha, chegada, prefixo);
        }
    }
}

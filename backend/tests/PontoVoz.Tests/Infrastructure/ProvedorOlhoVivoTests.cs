using System.Globalization;
using System.Net;
using System.Text;

using Microsoft.Extensions.Logging;

using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.ValueObjects;
using PontoVoz.Api.Infrastructure.Providers;
using PontoVoz.Tests.Api;
using PontoVoz.Tests.Domain;

namespace PontoVoz.Tests.Infrastructure;

/// <summary>
/// HTTP falso para o adaptador Olho Vivo: guarda as requisições recebidas e responde pela função
/// do teste. Nenhum teste deste projeto acessa a API real da SPTrans.
/// </summary>
internal sealed class HttpFalso(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder) : HttpMessageHandler
{
    public List<Uri> Requisicoes { get; } = [];

    public static HttpResponseMessage Json(string conteudo, HttpStatusCode status = HttpStatusCode.OK) =>
        new(status) { Content = new StringContent(conteudo, Encoding.UTF8, "application/json") };

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        lock (Requisicoes)
        {
            Requisicoes.Add(request.RequestUri!);
        }

        return responder(request, cancellationToken);
    }
}

/// <summary>
/// Adaptador Olho Vivo (R8) com respostas simuladas no formato da documentação oficial v2.1.
/// Estes testes provam o tratamento de respostas e falhas, <b>não</b> o funcionamento contra a API
/// real da SPTrans, que nunca foi acessada.
/// </summary>
public sealed class ProvedorOlhoVivoTests
{
    /// <summary>Valor fictício, só de teste.</summary>
    private const string TokenDeTeste = "token-ficticio-de-teste-olho-vivo";

    private const int CodigoParadaOficial = 340015329;
    private const int CodigoLinhaOficial = 1273;
    private const int CodigoOutraLinhaOficial = 1989;

    private static readonly TimeSpan SaoPaulo = TimeSpan.FromHours(-3);
    private static readonly DateTimeOffset Referencia = new(2026, 11, 10, 14, 32, 0, SaoPaulo);

    private readonly Parada _paradaReal = Cenario.ParadaAdministrativa(dadosSimulados: false, codigo: "PT-0001").ComId(10);
    private readonly Linha _linhaReal = Cenario.LinhaAdministrativa(dadosSimulados: false, codigo: "LT-8000").ComId(20);
    private readonly Linha _outraLinhaReal = Cenario.LinhaAdministrativa(dadosSimulados: false, codigo: "LT-8001").ComId(21);
    private readonly LogsEmMemoria _logs = new();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static string RespostaValida(string veiculosLinha, string veiculosOutra = "[]") => $$$"""
        {"hr":"14:32","p":{"cp":{{{CodigoParadaOficial}}},"np":"PARADA REAL","py":-23.5,"px":-46.6,"l":[
          {"c":"8000-10","cl":{{{CodigoLinhaOficial}}},"sl":1,"lt0":"TERM. A","lt1":"TERM. B","qv":2,"vs":{{{veiculosLinha}}}},
          {"c":"8001-10","cl":{{{CodigoOutraLinhaOficial}}},"sl":1,"lt0":"TERM. C","lt1":"TERM. D","qv":0,"vs":{{{veiculosOutra}}}},
          {"c":"9999-10","cl":5555,"sl":2,"lt0":"OUTRA","lt1":"OUTRA","qv":1,"vs":[{"p":"11111","t":"14:35","a":true,"ta":"2026-11-10T17:31:00Z","py":0,"px":0}]}
        ]}}
        """;

    private static OpcoesPrevisoes Opcoes(bool habilitado = true, string? token = TokenDeTeste, int prazo = 5) => new()
    {
        OlhoVivo = new OpcoesOlhoVivo
        {
            Habilitado = habilitado,
            Token = token,
            TempoLimiteSegundos = prazo,
            Paradas = new() { ["PT-0001"] = CodigoParadaOficial, ["SIM-P001"] = 1 },
            Linhas = new() { ["LT-8000"] = CodigoLinhaOficial, ["LT-8001"] = CodigoOutraLinhaOficial, ["SIM-L002"] = 2 },
        },
    };

    private ProvedorOlhoVivo Adaptador(HttpFalso http, OpcoesPrevisoes? opcoes = null) =>
        new(new HttpClient(http) { BaseAddress = new Uri(OpcoesOlhoVivo.UrlBasePadrao) },
            new MonitorFixo<OpcoesPrevisoes>(opcoes ?? Opcoes()),
            LoggerFactory.Create(l => l.AddProvider(_logs)).CreateLogger<ProvedorOlhoVivo>());

    /// <summary>Login aceito e a resposta de previsão informada.</summary>
    private static HttpFalso Api(string previsao, string login = "true") =>
        new((requisicao, _) => Task.FromResult(requisicao.RequestUri!.AbsolutePath.EndsWith("/Login/Autenticar", StringComparison.Ordinal)
            ? HttpFalso.Json(login)
            : HttpFalso.Json(previsao)));

    private Task<ResultadoProvedor> ConsultarAsync(ProvedorOlhoVivo adaptador, CancellationToken? cancelamento = null) =>
        adaptador.ConsultarAsync(_paradaReal, [_linhaReal, _outraLinhaReal], Referencia, cancelamento ?? Ct);

    private void AssertSemTokenNosLogs() => Assert.DoesNotContain(TokenDeTeste, _logs.TextoCompleto, StringComparison.Ordinal);

    [Fact]
    public async Task AutenticacaoERespostaValidas_ViramPrevisoesOlhoVivo()
    {
        HttpFalso http = Api(RespostaValida("""
            [{"p":"74558","t":"14:40","a":true,"ta":"2026-11-10T17:31:00Z","py":-23.5,"px":-46.6},
             {"p":74559,"t":"14:55","a":false,"ta":"2026-11-10T17:31:00Z","py":-23.5,"px":-46.6},
             {"p":"74560","t":"14:20","a":true,"ta":"2026-11-10T17:31:00Z","py":-23.5,"px":-46.6}]
            """));

        ResultadoProvedor resultado = await ConsultarAsync(Adaptador(http));

        Assert.Equal((StatusPrevisao.Disponivel, OrigemDado.OlhoVivo), (resultado.Status, resultado.Origem));
        // 14h20 já passou: é descartada. A linha 5555 não foi pedida: é ignorada.
        Assert.Equal(
            [("LT-8000", "2026-11-10T17:40:00Z", "74558"), ("LT-8000", "2026-11-10T17:55:00Z", "74559")],
            resultado.Previsoes.Select(p => (p.Linha.CodigoLinha, p.PrevistaParaUtc.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture), p.PrefixoVeiculo!)));
        Assert.All(resultado.Previsoes, p => Assert.Equal(NivelConfirmacao.Previsao, p.NivelConfirmacao));

        Assert.Equal(2, http.Requisicoes.Count);
        Assert.All(http.Requisicoes, uri => Assert.Equal(Uri.UriSchemeHttps, uri.Scheme));
        Assert.Equal("/v2.1/Login/Autenticar", http.Requisicoes[0].AbsolutePath);
        Assert.Equal($"?token={TokenDeTeste}", http.Requisicoes[0].Query);
        Assert.Equal("/v2.1/Previsao/Parada", http.Requisicoes[1].AbsolutePath);
        Assert.Equal($"?codigoParada={CodigoParadaOficial}", http.Requisicoes[1].Query);
        AssertSemTokenNosLogs();
    }

    [Fact]
    public async Task VeiculoSemHorarioOuComPrefixoInvalido_NaoInventaDados()
    {
        HttpFalso http = Api(RespostaValida("""
            [{"p":"74558","a":true},
             {"p":"74559","t":"25:99"},
             {"p":"PREFIXO INVALIDO COM ESPACO","t":"14:45"},
             {"t":"14:50"}]
            """));

        ResultadoProvedor resultado = await ConsultarAsync(Adaptador(http));

        Assert.Equal([("2026-11-10T17:45:00Z", (string?)null), ("2026-11-10T17:50:00Z", null)],
            resultado.Previsoes.Select(p => (p.PrevistaParaUtc.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture), p.PrefixoVeiculo)));
    }

    [Theory]
    [InlineData("""{"hr":"14:32","p":{"cp":1,"np":"X","l":[{"cl":1273,"vs":[]},{"cl":1989,"vs":[]}]}}""")]
    [InlineData("""{"hr":"14:32","p":{"cp":1,"np":"X","l":[]}}""")]
    [InlineData("""{"hr":"14:32","p":{"cp":1,"np":"X","l":null}}""")]
    [InlineData("""{"hr":"14:32","p":null}""")]
    public async Task RespostaValidaSemVeiculos_ViraSemPrevisoes(string json)
    {
        ResultadoProvedor resultado = await ConsultarAsync(Adaptador(Api(json)));

        Assert.Equal((StatusPrevisao.SemPrevisoes, OrigemDado.OlhoVivo), (resultado.Status, resultado.Origem));
    }

    [Theory]
    [InlineData("false")]
    [InlineData("algo inesperado")]
    public async Task AutenticacaoRecusada_FicaIndisponivelSemConsultarPrevisao(string respostaLogin)
    {
        HttpFalso http = Api(RespostaValida("[]"), login: respostaLogin);

        ResultadoProvedor resultado = await ConsultarAsync(Adaptador(http));

        Assert.Equal(StatusPrevisao.ProvedorIndisponivel, resultado.Status);
        Assert.Single(http.Requisicoes);
        Assert.Contains(_logs.Entradas, e => e.Mensagem.Contains("recusou a autenticação", StringComparison.Ordinal));
        AssertSemTokenNosLogs();
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task ErroHttpNaPrevisao_FicaIndisponivel(HttpStatusCode status)
    {
        var http = new HttpFalso((requisicao, _) => Task.FromResult(requisicao.RequestUri!.AbsolutePath.EndsWith("/Login/Autenticar", StringComparison.Ordinal)
            ? HttpFalso.Json("true")
            : HttpFalso.Json("{}", status)));

        ResultadoProvedor resultado = await ConsultarAsync(Adaptador(http));

        Assert.Equal(StatusPrevisao.ProvedorIndisponivel, resultado.Status);
        AssertSemTokenNosLogs();
    }

    [Fact]
    public async Task FalhaDeConexao_FicaIndisponivel()
    {
        var http = new HttpFalso((_, _) => throw new HttpRequestException("Nome do host não encontrado.", null, HttpStatusCode.ServiceUnavailable));

        ResultadoProvedor resultado = await ConsultarAsync(Adaptador(http));

        Assert.Equal(StatusPrevisao.ProvedorIndisponivel, resultado.Status);
        AssertSemTokenNosLogs();
    }

    [Fact]
    public async Task SemRespostaNoPrazo_FicaIndisponivel()
    {
        var http = new HttpFalso(async (_, token) =>
        {
            await Task.Delay(Timeout.Infinite, token);
            return HttpFalso.Json("true");
        });

        ResultadoProvedor resultado = await ConsultarAsync(Adaptador(http, Opcoes(prazo: 1)));

        Assert.Equal(StatusPrevisao.ProvedorIndisponivel, resultado.Status);
        Assert.Contains(_logs.Entradas, e => e.Mensagem.Contains("não respondeu em 1 s", StringComparison.Ordinal));
    }

    [Fact]
    public async Task CancelamentoDoCliente_EPropagadoENaoViraIndisponibilidade()
    {
        using var cancelamento = CancellationTokenSource.CreateLinkedTokenSource(Ct);
        var http = new HttpFalso(async (_, token) =>
        {
            await cancelamento.CancelAsync();
            await Task.Delay(Timeout.Infinite, token);
            return HttpFalso.Json("true");
        });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => ConsultarAsync(Adaptador(http), cancelamento.Token));
    }

    [Theory]
    [InlineData("isto não é json")]
    [InlineData("""{"hr":"14:32"}""")]
    [InlineData("""{"hr":"14:32","p":{"cp":1,"np":"X"}}""")]
    [InlineData("""{"hr":"14:32","p":{"cp":1,"np":"X","l":[{"cl":"texto","vs":[]}]}}""")]
    [InlineData("""{"hr":"14:32","p":{"cp":1,"np":"X","l":[{"vs":[]}]}}""")]
    [InlineData("""[1, 2, 3]""")]
    public async Task JsonInvalidoOuSemCamposObrigatorios_FicaIndisponivel(string json)
    {
        ResultadoProvedor resultado = await ConsultarAsync(Adaptador(Api(json)));

        Assert.Equal(StatusPrevisao.ProvedorIndisponivel, resultado.Status);
    }

    [Theory]
    [InlineData(false, TokenDeTeste)]
    [InlineData(true, null)]
    [InlineData(true, "   ")]
    public async Task DesabilitadoOuSemToken_NaoFazNenhumaChamada(bool habilitado, string? token)
    {
        var http = new HttpFalso((_, _) => throw new InvalidOperationException("Não deveria chamar a rede."));

        ResultadoProvedor resultado = await ConsultarAsync(Adaptador(http, Opcoes(habilitado, token)));

        Assert.Equal(StatusPrevisao.ProvedorIndisponivel, resultado.Status);
        Assert.Empty(http.Requisicoes);
    }

    [Fact]
    public async Task DadosSim_NuncaSaoEnviadosASptrans()
    {
        var http = new HttpFalso((_, _) => throw new InvalidOperationException("Não deveria chamar a rede."));
        Parada centro = Cenario.ParadaSimulada().ComId(1);
        Linha l002 = Cenario.LinhaSimulada("SIM-L002", "SIM-102").ComId(2);

        // Mesmo com "SIM-P001" e "SIM-L002" (indevidamente) no mapeamento, nada é enviado.
        ResultadoProvedor simulada = await Adaptador(http).ConsultarAsync(centro, [l002], Referencia, Ct);
        ResultadoProvedor linhaSim = await Adaptador(http).ConsultarAsync(_paradaReal, [l002], Referencia, Ct);

        Assert.Equal(StatusPrevisao.ProvedorIndisponivel, simulada.Status);
        Assert.Equal(StatusPrevisao.ProvedorIndisponivel, linhaSim.Status);
        Assert.Empty(http.Requisicoes);
    }

    [Fact]
    public async Task ParadaSemCodigoOficialDeclarado_NaoFazChamada()
    {
        var http = new HttpFalso((_, _) => throw new InvalidOperationException("Não deveria chamar a rede."));
        Parada semCodigo = Cenario.ParadaAdministrativa(dadosSimulados: false, codigo: "PT-0002").ComId(11);

        ResultadoProvedor resultado = await Adaptador(http).ConsultarAsync(semCodigo, [_linhaReal], Referencia, Ct);

        Assert.Equal(StatusPrevisao.ProvedorIndisponivel, resultado.Status);
        Assert.Empty(http.Requisicoes);
    }

    [Theory]
    [InlineData("2026-11-10T23:58:00-03:00", "00:05", "2026-11-11T03:05:00Z")]
    [InlineData("2026-11-30T23:50:00-03:00", "00:10", "2026-12-01T03:10:00Z")]
    [InlineData("2026-11-11T00:02:00-03:00", "23:59", "2026-11-11T02:59:00Z")]
    [InlineData("2026-11-10T14:32:00-03:00", "14:40", "2026-11-10T17:40:00Z")]
    public void HorarioHHmm_UsaODiaMaisProximoDaReferenciaEmSaoPaulo(string referencia, string horario, string esperado)
    {
        DateTimeOffset instante = ProvedorOlhoVivo.InstanteDoHorario(TimeSpan.ParseExact(horario, @"hh\:mm", CultureInfo.InvariantCulture), DateTimeOffset.Parse(referencia, CultureInfo.InvariantCulture));

        Assert.Equal(DateTimeOffset.Parse(esperado, CultureInfo.InvariantCulture), instante);
        Assert.Equal(TimeSpan.Zero, instante.Offset);
    }

    // ----- Seleção do provedor -----

    private SeletorProvedorPrevisao Seletor(HttpFalso http, OpcoesPrevisoes opcoes) =>
        new(new ProvedorPrevisaoSimulado(new MonitorFixo<OpcoesPrevisoes>(opcoes)), Adaptador(http, opcoes), new MonitorFixo<OpcoesPrevisoes>(opcoes));

    [Fact]
    public async Task Seletor_PorPadrao_UsaOSimuladoSemChamadaExterna()
    {
        var http = new HttpFalso((_, _) => throw new InvalidOperationException("Não deveria chamar a rede."));
        SeletorProvedorPrevisao seletor = Seletor(http, new OpcoesPrevisoes());

        ResultadoProvedor resultado = await seletor.ConsultarAsync(_paradaReal, [_linhaReal], Referencia, Ct);

        Assert.Equal(OrigemDado.Simulado, resultado.Origem);
        Assert.Equal(OrigemDado.Simulado, seletor.OrigemPara(_paradaReal));
        Assert.Empty(http.Requisicoes);
    }

    [Fact]
    public async Task Seletor_HabilitadoEComCodigoOficial_UsaOlhoVivo_MasDadosSimContinuamNoSimulado()
    {
        HttpFalso http = Api(RespostaValida("""[{"p":"74558","t":"14:40"}]"""));
        SeletorProvedorPrevisao seletor = Seletor(http, Opcoes());
        Parada centro = Cenario.ParadaSimulada().ComId(1);
        Linha l002 = Cenario.LinhaSimulada("SIM-L002", "SIM-102").ComId(2);

        ResultadoProvedor real = await seletor.ConsultarAsync(_paradaReal, [_linhaReal], Referencia, Ct);
        int chamadasDepoisDaReal = http.Requisicoes.Count;
        ResultadoProvedor demonstracao = await seletor.ConsultarAsync(centro, [l002], Referencia, Ct);

        Assert.Equal(OrigemDado.OlhoVivo, real.Origem);
        Assert.Equal(OrigemDado.OlhoVivo, seletor.OrigemPara(_paradaReal));
        Assert.Equal(OrigemDado.Simulado, demonstracao.Origem);
        Assert.Equal(OrigemDado.Simulado, seletor.OrigemPara(centro));
        Assert.Equal(chamadasDepoisDaReal, http.Requisicoes.Count);
    }

    [Fact]
    public async Task Seletor_FalhaDaOlhoVivo_NaoViraPrevisaoSimulada()
    {
        var http = new HttpFalso((_, _) => throw new HttpRequestException("falha"));
        SeletorProvedorPrevisao seletor = Seletor(http, Opcoes());

        ResultadoProvedor resultado = await seletor.ConsultarAsync(_paradaReal, [_linhaReal], Referencia, Ct);

        Assert.Equal((StatusPrevisao.ProvedorIndisponivel, OrigemDado.OlhoVivo), (resultado.Status, resultado.Origem));
        Assert.Empty(resultado.Previsoes);
    }
}

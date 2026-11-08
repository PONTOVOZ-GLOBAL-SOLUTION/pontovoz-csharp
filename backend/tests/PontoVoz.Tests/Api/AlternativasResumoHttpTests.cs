using System.Globalization;
using System.Net;
using System.Text.Json.Nodes;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using PontoVoz.Api.Application.Interfaces;
using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.ValueObjects;
using PontoVoz.Api.Infrastructure.Persistence;
using PontoVoz.Api.Infrastructure.Providers;
using PontoVoz.Tests.Application;
using PontoVoz.Tests.Infrastructure;

namespace PontoVoz.Tests.Api;

/// <summary>
/// <c>/alternativas</c> e <c>/resumo</c> (seções 4.7 e 4.8) pela API real, com SQLite isolado, o
/// seed, o registro manual dos exemplos e o relógio às 14h32 de 10/11/2026. Compara as respostas com
/// os quatro exemplos do contrato e confere a indisponibilidade nas três rotas que usam previsões.
/// Os ids vêm sempre da própria API.
/// </summary>
public sealed class AlternativasResumoHttpTests
{
    private static CancellationToken Ct => ApiTeste.Ct;

    private static Action<IWebHostBuilder> ComIndisponibilidade =>
        builder => builder.UseSetting("Previsoes:SimularIndisponibilidade", "true");

    private static async Task<int> IdDoDestinoAsync(ApiTeste api, string termo) =>
        (await api.GetAsync($"/api/v1/destinos?termo={termo}")).Json["itens"]!.AsArray().Single()!["id"]!.GetValue<int>();

    private static string[] Avisos(JsonNode json) => [.. json["avisos"]!.AsArray().Select(a => a!["codigo"]!.GetValue<string>())];

    private static void AssertErro(ApiTeste.Resposta resposta, HttpStatusCode status, string codigo)
    {
        Assert.Equal(status, resposta.Status);
        Assert.Equal("application/problem+json", resposta.TipoMidia);
        Assert.Equal(codigo, resposta.Json["codigo"]!.GetValue<string>());
        Assert.NotNull(resposta.Json["traceId"]);
    }

    // ----- Alternativas -----

    [Fact]
    public async Task Alternativas_DoExemplo_SaoIguaisAoContrato()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();
        int parada = await api.IdDaParadaAsync("SIM-P001");
        int norte = await IdDoDestinoAsync(api, "norte");

        ApiTeste.Resposta resposta = await api.GetAsync($"/api/v1/paradas/{parada}/alternativas?destinoId={norte}");

        Assert.Equal(HttpStatusCode.OK, resposta.Status);
        Assert.Equal("application/json", resposta.TipoMidia);
        Assert.Equal(norte, resposta.Json["destino"]!["id"]!.GetValue<int>());
        ExemplosContrato.AssertJsonIgualAoExemplo(ExemplosContrato.Exemplo("alternativas.json"), resposta.Json);
        Assert.Equal(["SIM-L004", "SIM-L002", "SIM-L001"], resposta.Json["alternativas"]!.AsArray().Select(a => a!["codigoLinha"]!.GetValue<string>()));
    }

    [Fact]
    public async Task Alternativas_LinhaSemPrevisao_TemOsCamposNulosPresentes()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();
        int parada = await api.IdDaParadaAsync("SIM-P001");
        int norte = await IdDoDestinoAsync(api, "norte");

        JsonObject semPrevisao = (await api.GetAsync($"/api/v1/paradas/{parada}/alternativas?destinoId={norte}")).Json["alternativas"]!.AsArray()[2]!.AsObject();

        foreach (string campo in new[] { "posicao", "previstaPara", "esperaMinutos", "tempoTotalEstimadoMinutos", "chegadaEstimadaAoDestino" })
        {
            Assert.True(semPrevisao.ContainsKey(campo), campo);
            Assert.Null(semPrevisao[campo]);
        }

        Assert.Equal("SemPrevisao", semPrevisao["situacao"]!.GetValue<string>());
        Assert.Equal(30, semPrevisao["percursoEstimadoMinutos"]!.GetValue<int>());
    }

    [Fact]
    public async Task Alternativas_ComProvedorIndisponivel_200IgualAoExemploSemComparacao()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync(ComIndisponibilidade);
        int parada = await api.IdDaParadaAsync("SIM-P001");
        int norte = await IdDoDestinoAsync(api, "norte");

        ApiTeste.Resposta resposta = await api.GetAsync($"/api/v1/paradas/{parada}/alternativas?destinoId={norte}");

        Assert.Equal(HttpStatusCode.OK, resposta.Status);
        ExemplosContrato.AssertJsonIgualAoExemplo(ExemplosContrato.Exemplo("alternativas-sem-comparacao.json"), resposta.Json);
    }

    [Fact]
    public async Task Alternativas_SemLinhaDireta_200ListaVaziaESemConsultarOProvedor()
    {
        // Mesmo com o provedor indisponível, SemLinhaDireta continua: o provedor não é consultado.
        await using ApiTeste api = await ApiTeste.CriarAsync(ComIndisponibilidade);
        int parada = await api.IdDaParadaAsync("SIM-P001");
        int sul = await IdDoDestinoAsync(api, "sul");

        ApiTeste.Resposta resposta = await api.GetAsync($"/api/v1/paradas/{parada}/alternativas?destinoId={sul}");

        Assert.Equal(HttpStatusCode.OK, resposta.Status);
        JsonObject json = resposta.Json.AsObject();
        Assert.Equal(("SemLinhaDireta", false), (json["situacao"]!.GetValue<string>(), json["comparacaoCompleta"]!.GetValue<bool>()));
        Assert.True(json.ContainsKey("statusPrevisao") && json["statusPrevisao"] is null);
        Assert.True(json.ContainsKey("origemPrevisao") && json["origemPrevisao"] is null);
        Assert.Empty(json["alternativas"]!.AsArray());
        Assert.Equal(["SemLinhaDireta", "DadosSimulados", "LimitacaoLinhasDiretas"], Avisos(json));
    }

    [Fact]
    public async Task Alternativas_OutraParadaComDuasLinhas_ComparaComPercursosDoSeed()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();
        int avenida = await api.IdDaParadaAsync("SIM-P002");
        int sul = await IdDoDestinoAsync(api, "sul");

        JsonNode json = (await api.GetAsync($"/api/v1/paradas/{avenida}/alternativas?destinoId={sul}")).Json;

        // SIM-P002 → Terminal Sul: só a SIM-L003 tem percurso (25 min) no seed.
        JsonNode unica = Assert.Single(json["alternativas"]!.AsArray())!;
        Assert.Equal(("SIM-L003", 1, 25), (unica["codigoLinha"]!.GetValue<string>(), unica["posicao"]!.GetValue<int>(), unica["percursoEstimadoMinutos"]!.GetValue<int>()));
        Assert.True(json["comparacaoCompleta"]!.GetValue<bool>());
        Assert.Equal(unica["esperaMinutos"]!.GetValue<int>() + 25, unica["tempoTotalEstimadoMinutos"]!.GetValue<int>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("?destinoId=abc")]
    [InlineData("?destinoId=0")]
    [InlineData("?destinoId=-1")]
    public async Task Alternativas_DestinoIdAusenteOuInvalido_Responde400(string query)
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();
        int parada = await api.IdDaParadaAsync("SIM-P001");

        ApiTeste.Resposta resposta = await api.GetAsync($"/api/v1/paradas/{parada}/alternativas{query}");

        AssertErro(resposta, HttpStatusCode.BadRequest, "RequisicaoInvalida");
        Assert.Equal("destinoId", Assert.Single(resposta.Json["errors"]!.AsObject()).Key);
    }

    [Fact]
    public async Task Alternativas_ParadaOuDestinoInexistenteOuInativo_Responde404()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();
        int parada = await api.IdDaParadaAsync("SIM-P001");
        int norte = await IdDoDestinoAsync(api, "norte");
        int sul = await IdDoDestinoAsync(api, "sul");

        AssertErro(await api.GetAsync($"/api/v1/paradas/99/alternativas?destinoId={norte}"), HttpStatusCode.NotFound, "ParadaNaoEncontrada");
        AssertErro(await api.GetAsync($"/api/v1/paradas/{parada}/alternativas?destinoId={norte + 1000}"), HttpStatusCode.NotFound, "DestinoNaoEncontrado");
        AssertErro(await api.GetAsync($"/api/v1/paradas/abc/alternativas?destinoId={norte}"), HttpStatusCode.NotFound, "RotaNaoEncontrada");

        // O MVP não tem rota para desativar destino; o teste desativa direto no banco isolado.
        await using (PontoVozDbContext contexto = api.NovoContexto())
        {
            await contexto.Database.ExecuteSqlAsync($"UPDATE Destinos SET Ativo = 0 WHERE Id = {sul}", Ct);
        }

        AssertErro(await api.GetAsync($"/api/v1/paradas/{parada}/alternativas?destinoId={sul}"), HttpStatusCode.NotFound, "DestinoNaoEncontrado");

        await api.EnviarAsync(HttpMethod.Put, $"/api/v1/admin/paradas/{parada}", """{"nome":"Parada Demonstração Centro","ativa":false}""");
        AssertErro(await api.GetAsync($"/api/v1/paradas/{parada}/alternativas?destinoId={norte}"), HttpStatusCode.NotFound, "ParadaNaoEncontrada");
    }

    // ----- Resumo -----

    [Fact]
    public async Task Resumo_Completo_EIgualAoContrato()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();
        int parada = await api.IdDaParadaAsync("SIM-P001");

        ApiTeste.Resposta resposta = await api.GetAsync($"/api/v1/paradas/{parada}/resumo");

        Assert.Equal(HttpStatusCode.OK, resposta.Status);
        Assert.Equal("application/json", resposta.TipoMidia);
        ExemplosContrato.AssertJsonIgualAoExemplo(ExemplosContrato.Exemplo("resumo.json"), resposta.Json);
        JsonNode json = resposta.Json;
        string consultadoEm = json["consultadoEm"]!.GetValue<string>();
        Assert.Equal(consultadoEm, json["previsoes"]!["consultadoEm"]!.GetValue<string>());
        Assert.Equal(consultadoEm, json["ultimasPassagens"]!["consultadoEm"]!.GetValue<string>());
    }

    [Fact]
    public async Task ComProvedorIndisponivel_PrevisoesDa503_AlternativasEResumoDao200()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync(ComIndisponibilidade);
        int parada = await api.IdDaParadaAsync("SIM-P001");
        int norte = await IdDoDestinoAsync(api, "norte");

        ApiTeste.Resposta previsoes = await api.GetAsync($"/api/v1/paradas/{parada}/previsoes");
        ApiTeste.Resposta alternativas = await api.GetAsync($"/api/v1/paradas/{parada}/alternativas?destinoId={norte}");
        ApiTeste.Resposta resumo = await api.GetAsync($"/api/v1/paradas/{parada}/resumo");

        AssertErro(previsoes, HttpStatusCode.ServiceUnavailable, "ProvedorPrevisoesIndisponivel");
        Assert.Equal("30", Assert.Single(previsoes.Cabecalhos.GetValues("Retry-After")));
        Assert.Equal((HttpStatusCode.OK, "SemComparacao"), (alternativas.Status, alternativas.Json["situacao"]!.GetValue<string>()));
        Assert.Equal((HttpStatusCode.OK, "Parcial"), (resumo.Status, resumo.Json["situacao"]!.GetValue<string>()));
        ExemplosContrato.AssertJsonIgualAoExemplo(ExemplosContrato.Exemplo("resumo-provedor-indisponivel.json"), resumo.Json);
    }

    [Fact]
    public async Task Resumo_ParadaInexistenteOuInativa_Responde404()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();
        int parada = await api.IdDaParadaAsync("SIM-P002");
        await api.EnviarAsync(HttpMethod.Put, $"/api/v1/admin/paradas/{parada}", """{"nome":"Parada Demonstração Avenida","ativa":false}""");

        AssertErro(await api.GetAsync("/api/v1/paradas/99/resumo"), HttpStatusCode.NotFound, "ParadaNaoEncontrada");
        AssertErro(await api.GetAsync($"/api/v1/paradas/{parada}/resumo"), HttpStatusCode.NotFound, "ParadaNaoEncontrada");
    }

    [Fact]
    public async Task Resumo_ParadaSemLinhas_CompletoComListasVazias()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync(ComIndisponibilidade);
        ApiTeste.Resposta criada = await api.EnviarAsync(HttpMethod.Post, "/api/v1/admin/paradas",
            """{"codigoExterno":"TESTE-P900","nome":"Parada Sem Linhas","dadosSimulados":true}""");
        int parada = criada.Json["id"]!.GetValue<int>();

        JsonNode json = (await api.GetAsync($"/api/v1/paradas/{parada}/resumo")).Json;

        Assert.Equal("Completo", json["situacao"]!.GetValue<string>());
        Assert.Empty(json["partesIndisponiveis"]!.AsArray());
        Assert.Empty(json["parada"]!["linhas"]!.AsArray());
        Assert.Equal("SemPrevisoes", json["previsoes"]!["status"]!.GetValue<string>());
        Assert.Contains("ParadaSemLinhas", Avisos(json["previsoes"]!));
        Assert.Contains("ParadaSemLinhas", Avisos(json["ultimasPassagens"]!));
        Assert.Contains("CadastroNaoVerificado", Avisos(json["previsoes"]!));
    }

    [Fact]
    public async Task Resumo_RegistroAindaNoFuturo_SoApareceQuandoOHorarioChega()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();
        int parada = await api.IdDaParadaAsync("SIM-P001");
        int l002 = await api.IdDaLinhaAsync("SIM-L002");
        string observadaEm = ApiTeste.ConsultaDosExemplos.AddMinutes(1).ToString("yyyy-MM-dd'T'HH:mm:sszzz", CultureInfo.InvariantCulture);
        Assert.Equal(HttpStatusCode.Created, (await api.EnviarAsync(HttpMethod.Post, "/api/v1/admin/observacoes-passagem",
            $$"""{"paradaId":{{parada}},"linhaId":{{l002}},"observadaEm":"{{observadaEm}}","prefixoVeiculo":"SIM-V2001","origemRegistro":"operador-local"}""")).Status);

        JsonNode antes = (await api.GetAsync($"/api/v1/paradas/{parada}/resumo")).Json;
        api.Relogio.Advance(TimeSpan.FromMinutes(1));
        JsonNode depois = (await api.GetAsync($"/api/v1/paradas/{parada}/resumo")).Json;

        string NivelDaL002(JsonNode resumo) => resumo["ultimasPassagens"]!["passagens"]!.AsArray()
            .Single(p => p!["codigoLinha"]!.GetValue<string>() == "SIM-L002")!["nivelConfirmacao"]!.GetValue<string>();
        Assert.Equal("ObservacaoSimulada", NivelDaL002(antes));
        Assert.Equal("RegistroManual", NivelDaL002(depois));
    }

    [Fact]
    public async Task AlternativasEResumo_NaoGravamNadaESaoPublicas()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync(chaveAdministrativa: "chave-de-teste-b9-nao-real", ambiente: "Production");
        int parada = await api.IdDaParadaAsync("SIM-P001");
        int norte = await IdDoDestinoAsync(api, "norte");
        (int, int, int, int) Contagem(PontoVozDbContext c) => (c.Paradas.Count(), c.ParadasLinhas.Count(), c.RegistrosPassagem.Count(), c.PercursosEstimados.Count());
        (int, int, int, int) antes;
        await using (PontoVozDbContext contexto = api.NovoContexto())
        {
            antes = Contagem(contexto);
        }

        for (int i = 0; i < 3; i++)
        {
            Assert.Equal(HttpStatusCode.OK, (await api.GetAsync($"/api/v1/paradas/{parada}/alternativas?destinoId={norte}")).Status);
            Assert.Equal(HttpStatusCode.OK, (await api.GetAsync($"/api/v1/paradas/{parada}/resumo")).Status);
        }

        await using PontoVozDbContext depois = api.NovoContexto();
        Assert.Equal(antes, Contagem(depois));
    }

    /// <summary>Valor fictício, só de teste.</summary>
    private const string TokenDeTeste = "token-ficticio-b9-olho-vivo";

    [Fact]
    public async Task Resumo_ComProvedorAssincronoDeVerdade_NaoTemConcorrenciaNoDbContext()
    {
        // A Olho Vivo (com HTTP falso e atraso) roda junto com a consulta de passagens pelo EF Core real.
        // Duas operações simultâneas no mesmo DbContext lançariam InvalidOperationException (500).
        var http = new HttpFalso(async (requisicao, token) =>
        {
            await Task.Delay(80, token);
            return requisicao.RequestUri!.AbsolutePath.EndsWith("/Login/Autenticar", StringComparison.Ordinal)
                ? HttpFalso.Json("true")
                : HttpFalso.Json("""{"hr":"14:32","p":{"cp":4200,"np":"X","l":[{"cl":33,"vs":[{"p":"74558","t":"14:41"}]}]}}""");
        });
        var logs = new LogsEmMemoria();
        await using ApiTeste api = await ApiTeste.CriarAsync(builder =>
        {
            builder.UseSetting("Previsoes:OlhoVivo:Habilitado", "true");
            builder.UseSetting("Previsoes:OlhoVivo:Token", TokenDeTeste);
            builder.UseSetting("Previsoes:OlhoVivo:Paradas:PT-0500", "4200");
            builder.UseSetting("Previsoes:OlhoVivo:Linhas:LT-0500", "33");
            builder.ConfigureTestServices(services => services.AddHttpClient<ProvedorOlhoVivo>().ConfigurePrimaryHttpMessageHandler(() => http));
            builder.ConfigureLogging(logging => logging.AddProvider(logs));
        });
        int parada = (await api.EnviarAsync(HttpMethod.Post, "/api/v1/admin/paradas", """{"codigoExterno":"PT-0500","nome":"Parada Real","dadosSimulados":false}""")).Json["id"]!.GetValue<int>();
        int linha = (await api.EnviarAsync(HttpMethod.Post, "/api/v1/admin/linhas",
            """{"codigo":"LT-0500","letreiro":"500-A","terminalPrincipal":"Terminal A","terminalSecundario":"Terminal B","sentido":"PrincipalParaSecundario","dadosSimulados":false}""")).Json["id"]!.GetValue<int>();
        await api.EnviarAsync(HttpMethod.Put, $"/api/v1/admin/paradas/{parada}/linhas/{linha}");

        ApiTeste.Resposta resposta = await api.GetAsync($"/api/v1/paradas/{parada}/resumo");

        Assert.Equal(HttpStatusCode.OK, resposta.Status);
        JsonNode json = resposta.Json;
        Assert.Equal(("Completo", "OlhoVivo", false), (json["situacao"]!.GetValue<string>(), json["previsoes"]!["origem"]!.GetValue<string>(), json["dadosSimulados"]!.GetValue<bool>()));
        Assert.Single(json["previsoes"]!["previsoes"]!.AsArray());
        Assert.DoesNotContain(logs.Entradas, e => e.Nivel >= LogLevel.Error);
        Assert.DoesNotContain(TokenDeTeste, logs.TextoCompleto, StringComparison.Ordinal);
    }

    /// <summary>Provedor que espera até a requisição ser cancelada.</summary>
    private sealed class ProvedorLento : IProvedorPrevisaoChegada
    {
        public TaskCompletionSource Iniciou { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public OrigemDado OrigemPara(Parada parada) => OrigemDado.Simulado;

        public async Task<ResultadoProvedor> ConsultarAsync(Parada parada, IReadOnlyList<Linha> linhas, DateTimeOffset referencia, CancellationToken cancellationToken)
        {
            Iniciou.TrySetResult();
            await Task.Delay(Timeout.Infinite, cancellationToken);
            throw new InvalidOperationException("Não deveria chegar aqui.");
        }
    }

    [Theory]
    [InlineData("resumo")]
    [InlineData("alternativas")]
    public async Task Cancelamento_NaoViraErroNemRespostaParcial(string rota)
    {
        var lento = new ProvedorLento();
        var logs = new LogsEmMemoria();
        await using ApiTeste api = await ApiTeste.CriarAsync(builder =>
        {
            builder.ConfigureTestServices(services => services.AddScoped<IProvedorPrevisaoChegada>(_ => lento));
            builder.ConfigureLogging(logging => logging.AddProvider(logs).AddFilter("Microsoft.AspNetCore.Diagnostics", LogLevel.Debug));
        });
        int parada = await api.IdDaParadaAsync("SIM-P001");
        int norte = await IdDoDestinoAsync(api, "norte");
        string caminho = rota == "resumo" ? $"/api/v1/paradas/{parada}/resumo" : $"/api/v1/paradas/{parada}/alternativas?destinoId={norte}";
        using HttpClient cliente = api.Cliente();
        using var cancelamento = CancellationTokenSource.CreateLinkedTokenSource(Ct);

        Task<HttpResponseMessage> requisicao = cliente.GetAsync(new Uri(caminho, UriKind.Relative), cancelamento.Token);
        await lento.Iniciou.Task.WaitAsync(TimeSpan.FromSeconds(10), Ct);
        await cancelamento.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => requisicao);
        const string Abortada = "The request was aborted by the client.";
        DateTime limite = DateTime.UtcNow.AddSeconds(10);
        while (!logs.Entradas.Any(e => e.Mensagem.StartsWith(Abortada, StringComparison.Ordinal)) && DateTime.UtcNow < limite)
        {
            await Task.Delay(50, Ct);
        }

        Assert.Contains(logs.Entradas, e => e.Mensagem.StartsWith(Abortada, StringComparison.Ordinal));
        Assert.DoesNotContain(logs.Entradas, e => e.Nivel >= LogLevel.Error);
    }
}

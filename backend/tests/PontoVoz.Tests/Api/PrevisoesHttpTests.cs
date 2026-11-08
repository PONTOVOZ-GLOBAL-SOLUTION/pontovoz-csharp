using System.Net;
using System.Text.Json;
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
/// <c>GET /api/v1/paradas/{id}/previsoes</c> (seção 4.5) pela API real, com SQLite isolado e o
/// relógio parado às 14h32 de 10/11/2026: o exemplo do contrato, os estados, os erros 400, 404,
/// 422 e 503 (com <c>Retry-After</c>), o acesso público, a ausência de gravação e o caminho do
/// adaptador Olho Vivo com HTTP falso. Os ids vêm da própria API.
/// </summary>
public sealed class PrevisoesHttpTests
{
    private static CancellationToken Ct => ApiTeste.Ct;

    private static Action<IWebHostBuilder> ComIndisponibilidade =>
        builder => builder.UseSetting("Previsoes:SimularIndisponibilidade", "true");

    private static async Task<int> IdAsync(ApiTeste api, string codigo) =>
        codigo.Contains("-P", StringComparison.Ordinal) ? await api.IdDaParadaAsync(codigo) : await api.IdDaLinhaAsync(codigo);

    private static string[] Avisos(JsonNode json) => [.. json["avisos"]!.AsArray().Select(a => a!["codigo"]!.GetValue<string>())];

    private static void AssertErro(ApiTeste.Resposta resposta, HttpStatusCode status, string codigo)
    {
        Assert.Equal(status, resposta.Status);
        Assert.Equal("application/problem+json", resposta.TipoMidia);
        Assert.Equal(codigo, resposta.Json["codigo"]!.GetValue<string>());
        Assert.NotNull(resposta.Json["traceId"]);
    }

    private static async Task<int> CriarParadaAsync(ApiTeste api, string codigo, bool ficticia)
    {
        ApiTeste.Resposta resposta = await api.EnviarAsync(HttpMethod.Post, "/api/v1/admin/paradas",
            $$"""{"codigoExterno":"{{codigo}}","nome":"Parada {{codigo}}","dadosSimulados":{{(ficticia ? "true" : "false")}}}""");
        Assert.Equal(HttpStatusCode.Created, resposta.Status);
        return resposta.Json["id"]!.GetValue<int>();
    }

    private static async Task<int> CriarLinhaAssociadaAsync(ApiTeste api, int paradaId, string codigo, bool ficticia)
    {
        ApiTeste.Resposta resposta = await api.EnviarAsync(HttpMethod.Post, "/api/v1/admin/linhas",
            $$"""{"codigo":"{{codigo}}","letreiro":"{{codigo}}","terminalPrincipal":"Terminal A","terminalSecundario":"Terminal B","sentido":"PrincipalParaSecundario","dadosSimulados":{{(ficticia ? "true" : "false")}}}""");
        Assert.Equal(HttpStatusCode.Created, resposta.Status);
        int linhaId = resposta.Json["id"]!.GetValue<int>();
        Assert.Equal(HttpStatusCode.NoContent, (await api.EnviarAsync(HttpMethod.Put, $"/api/v1/admin/paradas/{paradaId}/linhas/{linhaId}")).Status);
        return linhaId;
    }

    // ----- 200 -----

    [Fact]
    public async Task Previsoes_DoExemplo_SaoIguaisAoContrato()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();
        int id = await api.IdDaParadaAsync("SIM-P001");

        ApiTeste.Resposta resposta = await api.GetAsync($"/api/v1/paradas/{id}/previsoes");

        Assert.Equal(HttpStatusCode.OK, resposta.Status);
        Assert.Equal("application/json", resposta.TipoMidia);
        Assert.Equal(id, resposta.Json["paradaId"]!.GetValue<int>());
        ExemplosContrato.AssertJsonIgualAoExemplo(ExemplosContrato.Exemplo("previsoes.json"), resposta.Json);
    }

    [Fact]
    public async Task Previsoes_TiposDoJson_EnumsTextoDatasComFusoEOrdem()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();
        int id = await api.IdDaParadaAsync("SIM-P001");

        using JsonDocument json = JsonDocument.Parse((await api.GetAsync($"/api/v1/paradas/{id}/previsoes")).Corpo);
        JsonElement raiz = json.RootElement;

        Assert.Equal("Disponivel", raiz.GetProperty("status").GetString());
        Assert.Equal("Simulado", raiz.GetProperty("origem").GetString());
        Assert.EndsWith("-03:00", raiz.GetProperty("consultadoEm").GetString(), StringComparison.Ordinal);
        DateTimeOffset anterior = DateTimeOffset.MinValue;
        foreach (JsonElement previsao in raiz.GetProperty("previsoes").EnumerateArray())
        {
            Assert.Equal("Previsao", previsao.GetProperty("nivelConfirmacao").GetString());
            Assert.StartsWith("SIM-V", previsao.GetProperty("prefixoVeiculo").GetString(), StringComparison.Ordinal);
            string previstaPara = previsao.GetProperty("previstaPara").GetString()!;
            Assert.EndsWith("-03:00", previstaPara, StringComparison.Ordinal);
            DateTimeOffset instante = DateTimeOffset.Parse(previstaPara, System.Globalization.CultureInfo.InvariantCulture);
            Assert.True(instante >= anterior);
            anterior = instante;
            Assert.True(previsao.GetProperty("esperaMinutos").GetInt32() >= 0);
        }
    }

    [Fact]
    public async Task Previsoes_ComLinhaIdComPrevisao_DisponivelSemLinhasSemPrevisao()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();
        int parada = await api.IdDaParadaAsync("SIM-P001");
        int linha = await api.IdDaLinhaAsync("SIM-L004");

        JsonNode json = (await api.GetAsync($"/api/v1/paradas/{parada}/previsoes?linhaId={linha}")).Json;

        Assert.Equal("Disponivel", json["status"]!.GetValue<string>());
        Assert.Equal(["SIM-L004"], json["previsoes"]!.AsArray().Select(p => p!["codigoLinha"]!.GetValue<string>()));
        Assert.Empty(json["linhasSemPrevisao"]!.AsArray());
        Assert.Equal(["DadosSimulados"], Avisos(json));
    }

    [Fact]
    public async Task Previsoes_ComLinhaSemPrevisao_SemPrevisoes()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();
        int parada = await api.IdDaParadaAsync("SIM-P001");
        int linha = await api.IdDaLinhaAsync("SIM-L001");

        JsonNode json = (await api.GetAsync($"/api/v1/paradas/{parada}/previsoes?linhaId={linha}")).Json;

        Assert.Equal("SemPrevisoes", json["status"]!.GetValue<string>());
        Assert.Empty(json["previsoes"]!.AsArray());
        Assert.Equal(["SIM-L001"], json["linhasSemPrevisao"]!.AsArray().Select(l => l!["codigoLinha"]!.GetValue<string>()));
        Assert.Equal(["DadosSimulados", "SemPrevisoes"], Avisos(json));
    }

    [Fact]
    public async Task Previsoes_ParadaSemLinhas_200MesmoComOProvedorIndisponivel()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync(ComIndisponibilidade);
        int parada = await CriarParadaAsync(api, "TESTE-P800", ficticia: true);

        ApiTeste.Resposta resposta = await api.GetAsync($"/api/v1/paradas/{parada}/previsoes");

        Assert.Equal(HttpStatusCode.OK, resposta.Status);
        JsonNode json = resposta.Json;
        Assert.Equal("SemPrevisoes", json["status"]!.GetValue<string>());
        Assert.Empty(json["previsoes"]!.AsArray());
        Assert.Empty(json["linhasSemPrevisao"]!.AsArray());
        Assert.Equal(["DadosSimulados", "CadastroNaoVerificado", "ParadaSemLinhas"], Avisos(json));
    }

    [Fact]
    public async Task Previsoes_SaoPublicas_SemChaveAdministrativa()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync(chaveAdministrativa: "chave-de-teste-previsoes-nao-real", ambiente: "Production");
        int parada = await api.IdDaParadaAsync("SIM-P001");

        Assert.Equal(HttpStatusCode.OK, (await api.GetAsync($"/api/v1/paradas/{parada}/previsoes")).Status);
    }

    // ----- 400, 404, 422 -----

    [Theory]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-2")]
    public async Task Previsoes_ComLinhaIdInvalido_Responde400(string linhaId)
    {
        await using ApiTeste api = await ApiTeste.CriarAsync(ComIndisponibilidade);
        int parada = await api.IdDaParadaAsync("SIM-P001");

        ApiTeste.Resposta resposta = await api.GetAsync($"/api/v1/paradas/{parada}/previsoes?linhaId={linhaId}");

        AssertErro(resposta, HttpStatusCode.BadRequest, "RequisicaoInvalida");
        Assert.Equal("linhaId", Assert.Single(resposta.Json["errors"]!.AsObject()).Key);
    }

    [Fact]
    public async Task Previsoes_ComParadaInexistenteOuInativa_Responde404()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();
        int parada = await api.IdDaParadaAsync("SIM-P002");
        await api.EnviarAsync(HttpMethod.Put, $"/api/v1/admin/paradas/{parada}", """{"nome":"Parada Demonstração Avenida","ativa":false}""");

        AssertErro(await api.GetAsync("/api/v1/paradas/99/previsoes"), HttpStatusCode.NotFound, "ParadaNaoEncontrada");
        AssertErro(await api.GetAsync($"/api/v1/paradas/{parada}/previsoes"), HttpStatusCode.NotFound, "ParadaNaoEncontrada");
        AssertErro(await api.GetAsync("/api/v1/paradas/abc/previsoes"), HttpStatusCode.NotFound, "RotaNaoEncontrada");
    }

    [Fact]
    public async Task Previsoes_ComLinhaInexistente_Responde404()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();
        int parada = await api.IdDaParadaAsync("SIM-P001");
        int inexistente = await api.IdDaLinhaAsync("SIM-L004") + 1000;

        AssertErro(await api.GetAsync($"/api/v1/paradas/{parada}/previsoes?linhaId={inexistente}"), HttpStatusCode.NotFound, "LinhaNaoEncontrada");
    }

    [Fact]
    public async Task Previsoes_ComLinhaQueNaoAtendeAParada_Responde422AntesDoProvedor()
    {
        // Com o provedor indisponível, o 422 continua: o provedor só é consultado depois das validações.
        await using ApiTeste api = await ApiTeste.CriarAsync(ComIndisponibilidade);
        int parada = await api.IdDaParadaAsync("SIM-P001");
        int linha = await api.IdDaLinhaAsync("SIM-L003");

        AssertErro(await api.GetAsync($"/api/v1/paradas/{parada}/previsoes?linhaId={linha}"), (HttpStatusCode)422, "LinhaNaoAtendeParada");
    }

    // ----- 503 -----

    [Fact]
    public async Task Previsoes_ComProvedorIndisponivel_Responde503IgualAoExemploComRetryAfter()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync(ComIndisponibilidade);
        int parada = await api.IdDaParadaAsync("SIM-P001");
        string caminho = $"/api/v1/paradas/{parada}/previsoes";

        ApiTeste.Resposta resposta = await api.GetAsync(caminho);

        AssertErro(resposta, HttpStatusCode.ServiceUnavailable, "ProvedorPrevisoesIndisponivel");
        Assert.Equal(TimeSpan.FromSeconds(30), resposta.Cabecalhos.RetryAfter!.Delta);
        Assert.Equal("30", Assert.Single(resposta.Cabecalhos.GetValues("Retry-After")));

        // O exemplo usa a parada de id 1; só o instance e o traceId (dinâmicos) são normalizados.
        JsonNode esperado = ExemplosContrato.Exemplo("erro-provedor-previsoes-indisponivel.json");
        esperado.AsObject().Remove("traceId");
        esperado["instance"] = caminho;
        JsonNode obtido = resposta.Json.DeepClone();
        obtido.AsObject().Remove("traceId");
        Assert.True(JsonNode.DeepEquals(esperado, obtido), obtido.ToJsonString());
    }

    [Theory]
    [InlineData("/api/v1/paradas")]
    [InlineData("/api/v1/linhas")]
    [InlineData("/api/v1/destinos")]
    [InlineData("/health")]
    public async Task ComProvedorIndisponivel_AsOutrasConsultasContinuam(string caminho)
    {
        await using ApiTeste api = await ApiTeste.CriarAsync(ComIndisponibilidade);

        Assert.Equal(HttpStatusCode.OK, (await api.GetAsync(caminho)).Status);
        int parada = await api.IdDaParadaAsync("SIM-P001");
        Assert.Equal(HttpStatusCode.OK, (await api.GetAsync($"/api/v1/paradas/{parada}/ultimas-passagens")).Status);
    }

    // ----- Sem gravação -----

    [Fact]
    public async Task Previsoes_NaoGravamNadaNemMudamAsUltimasPassagens()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();
        int parada = await api.IdDaParadaAsync("SIM-P001");
        string ultimasAntes = (await api.GetAsync($"/api/v1/paradas/{parada}/ultimas-passagens")).Corpo;
        (int, int, int) contagemAntes;
        await using (PontoVozDbContext antes = api.NovoContexto())
        {
            contagemAntes = (await antes.RegistrosPassagem.CountAsync(Ct), await antes.Paradas.CountAsync(Ct), await antes.ParadasLinhas.CountAsync(Ct));
        }

        for (int i = 0; i < 3; i++)
        {
            Assert.Equal(HttpStatusCode.OK, (await api.GetAsync($"/api/v1/paradas/{parada}/previsoes")).Status);
            api.Relogio.Advance(TimeSpan.FromMinutes(7));
        }

        await using PontoVozDbContext depois = api.NovoContexto();
        Assert.Equal(contagemAntes, (await depois.RegistrosPassagem.CountAsync(Ct), await depois.Paradas.CountAsync(Ct), await depois.ParadasLinhas.CountAsync(Ct)));
        Assert.DoesNotContain(await depois.RegistrosPassagem.ToListAsync(Ct), r => r.NivelConfirmacao == NivelConfirmacao.Previsao);

        // As passagens são as mesmas; só o relógio andou.
        JsonNode antesJson = JsonNode.Parse(ultimasAntes)!;
        JsonNode depoisJson = (await api.GetAsync($"/api/v1/paradas/{parada}/ultimas-passagens")).Json;
        Assert.Equal(
            antesJson["passagens"]!.AsArray().Select(p => p!["id"]!.GetValue<int>()),
            depoisJson["passagens"]!.AsArray().Select(p => p!["id"]!.GetValue<int>()));
    }

    // ----- Olho Vivo com HTTP falso -----

    /// <summary>Valor fictício, só de teste.</summary>
    private const string TokenDeTeste = "token-ficticio-http-olho-vivo";

    private static Action<IWebHostBuilder> ComOlhoVivo(HttpFalso http, LogsEmMemoria logs) => builder =>
    {
        builder.UseSetting("Previsoes:OlhoVivo:Habilitado", "true");
        builder.UseSetting("Previsoes:OlhoVivo:Token", TokenDeTeste);
        builder.UseSetting("Previsoes:OlhoVivo:Paradas:PT-0500", "4200");
        builder.UseSetting("Previsoes:OlhoVivo:Linhas:LT-0500", "33");
        builder.ConfigureTestServices(services => services.AddHttpClient<ProvedorOlhoVivo>().ConfigurePrimaryHttpMessageHandler(() => http));
        builder.ConfigureLogging(logging => logging.AddProvider(logs));
    };

    [Fact]
    public async Task OlhoVivoHabilitado_ParadaComCodigoOficial_UsaOAdaptador_EDadosSimContinuamSimulados()
    {
        var http = new HttpFalso((requisicao, _) => Task.FromResult(requisicao.RequestUri!.AbsolutePath.EndsWith("/Login/Autenticar", StringComparison.Ordinal)
            ? HttpFalso.Json("true")
            : HttpFalso.Json("""{"hr":"14:32","p":{"cp":4200,"np":"X","l":[{"cl":33,"vs":[{"p":"74558","t":"14:41"},{"t":"14:50"}]}]}}""")));
        var logs = new LogsEmMemoria();
        await using ApiTeste api = await ApiTeste.CriarAsync(ComOlhoVivo(http, logs));
        int parada = await CriarParadaAsync(api, "PT-0500", ficticia: false);
        await CriarLinhaAssociadaAsync(api, parada, "LT-0500", ficticia: false);

        ApiTeste.Resposta real = await api.GetAsync($"/api/v1/paradas/{parada}/previsoes");
        int chamadas = http.Requisicoes.Count;
        ApiTeste.Resposta demonstracao = await api.GetAsync($"/api/v1/paradas/{await api.IdDaParadaAsync("SIM-P001")}/previsoes");

        JsonNode json = real.Json;
        Assert.Equal(("Disponivel", "OlhoVivo", false), (json["status"]!.GetValue<string>(), json["origem"]!.GetValue<string>(), json["dadosSimulados"]!.GetValue<bool>()));
        Assert.Equal(["CadastroNaoVerificado"], Avisos(json));
        JsonArray previsoes = json["previsoes"]!.AsArray();
        Assert.Equal(["2026-11-10T14:41:00-03:00", "2026-11-10T14:50:00-03:00"], previsoes.Select(p => p!["previstaPara"]!.GetValue<string>()));
        Assert.Equal("74558", previsoes[0]!["prefixoVeiculo"]!.GetValue<string>());
        Assert.True(previsoes[1]!.AsObject().ContainsKey("prefixoVeiculo"));
        Assert.Null(previsoes[1]!["prefixoVeiculo"]);

        Assert.Equal("Simulado", demonstracao.Json["origem"]!.GetValue<string>());
        Assert.Equal(chamadas, http.Requisicoes.Count);
        Assert.Equal(2, chamadas);
        Assert.DoesNotContain(TokenDeTeste, logs.TextoCompleto, StringComparison.Ordinal);
        Assert.DoesNotContain(TokenDeTeste, real.Corpo, StringComparison.Ordinal);
    }

    [Fact]
    public async Task OlhoVivoFalhando_Responde503SemPrevisaoSimuladaNoLugar()
    {
        var http = new HttpFalso((_, _) => throw new HttpRequestException("falha de conexão simulada"));
        var logs = new LogsEmMemoria();
        await using ApiTeste api = await ApiTeste.CriarAsync(ComOlhoVivo(http, logs));
        int parada = await CriarParadaAsync(api, "PT-0500", ficticia: false);
        await CriarLinhaAssociadaAsync(api, parada, "LT-0500", ficticia: false);

        ApiTeste.Resposta resposta = await api.GetAsync($"/api/v1/paradas/{parada}/previsoes");

        AssertErro(resposta, HttpStatusCode.ServiceUnavailable, "ProvedorPrevisoesIndisponivel");
        foreach (string proibido in new[] { TokenDeTeste, "falha de conexão simulada", "HttpRequestException", "api.olhovivo" })
        {
            Assert.DoesNotContain(proibido, resposta.Corpo, StringComparison.OrdinalIgnoreCase);
        }

        Assert.DoesNotContain(TokenDeTeste, logs.TextoCompleto, StringComparison.Ordinal);
        Assert.DoesNotContain(logs.Entradas, e => e.Nivel >= LogLevel.Error);
    }

    // ----- Cancelamento -----

    /// <summary>Provedor que espera até a requisição ser cancelada.</summary>
    private sealed class ProvedorLento : IProvedorPrevisaoChegada
    {
        public TaskCompletionSource Iniciou { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource Cancelado { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public OrigemDado OrigemPara(Parada parada) => OrigemDado.Simulado;

        public async Task<ResultadoProvedor> ConsultarAsync(Parada parada, IReadOnlyList<Linha> linhas, DateTimeOffset referencia, CancellationToken cancellationToken)
        {
            using CancellationTokenRegistration registro = cancellationToken.Register(() => Cancelado.TrySetResult());
            Iniciou.TrySetResult();
            await Task.Delay(Timeout.Infinite, cancellationToken);
            throw new InvalidOperationException("Não deveria chegar aqui.");
        }
    }

    [Fact]
    public async Task Cancelamento_ChegaAoProvedorENaoViraErroNem503()
    {
        var lento = new ProvedorLento();
        var logs = new LogsEmMemoria();
        await using ApiTeste api = await ApiTeste.CriarAsync(builder =>
        {
            builder.ConfigureTestServices(services => services.AddScoped<IProvedorPrevisaoChegada>(_ => lento));
            builder.ConfigureLogging(logging => logging.AddProvider(logs).AddFilter("Microsoft.AspNetCore.Diagnostics", LogLevel.Debug));
        });
        int parada = await api.IdDaParadaAsync("SIM-P001");
        using HttpClient cliente = api.Cliente();
        using var cancelamento = CancellationTokenSource.CreateLinkedTokenSource(Ct);

        Task<HttpResponseMessage> requisicao = cliente.GetAsync(new Uri($"/api/v1/paradas/{parada}/previsoes", UriKind.Relative), cancelamento.Token);
        await lento.Iniciou.Task.WaitAsync(TimeSpan.FromSeconds(10), Ct);
        await cancelamento.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => requisicao);
        await lento.Cancelado.Task.WaitAsync(TimeSpan.FromSeconds(10), Ct);
        const string Abortada = "The request was aborted by the client.";
        DateTime limite = DateTime.UtcNow.AddSeconds(10);
        while (!logs.Entradas.Any(e => e.Mensagem.StartsWith(Abortada, StringComparison.Ordinal)) && DateTime.UtcNow < limite)
        {
            await Task.Delay(50, Ct);
        }

        Assert.Contains(logs.Entradas, e => e.Mensagem.StartsWith(Abortada, StringComparison.Ordinal));
        Assert.DoesNotContain(logs.Entradas, e => e.Nivel >= LogLevel.Error);
        Assert.DoesNotContain(logs.Entradas, e => e.Categoria.EndsWith("TratadorExcecoes", StringComparison.Ordinal));
    }
}

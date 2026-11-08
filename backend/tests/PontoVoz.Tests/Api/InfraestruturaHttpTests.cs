using System.Net;
using System.Text.Json.Nodes;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using PontoVoz.Api.Application.Consultas;
using PontoVoz.Api.Application.Dtos;
using PontoVoz.Api.Application.Interfaces;
using PontoVoz.Tests.Application;

namespace PontoVoz.Tests.Api;

/// <summary>
/// Comportamentos transversais da camada HTTP: erro inesperado, cancelamento, CORS, documento
/// OpenAPI, Scalar e ausência das rotas que ainda não foram implementadas (B7 a B9).
/// </summary>
public sealed class InfraestruturaHttpTests
{
    private const string OrigemPermitida = "http://localhost:5173";
    private const string OrigemNaoPermitida = "http://origem-nao-listada.example";

    private static CancellationToken Ct => ApiTeste.Ct;

    // ----- 500 e cancelamento -----

    /// <summary>Service que falha com uma mensagem que nunca pode chegar ao cliente.</summary>
    private sealed class ParadaServiceComFalha : IParadaService
    {
        public const string Segredo = "Data Source=C:\\segredo\\pontovoz.db;Password=nao-pode-vazar";

        public Task<PaginaDto<ParadaDto>> BuscarAsync(ConsultaParadas consulta, CancellationToken cancellationToken) =>
            throw new InvalidOperationException(Segredo);

        public Task<ParadaDetalheDto> ObterDetalheAsync(int id, CancellationToken cancellationToken) =>
            throw new InvalidOperationException(Segredo);
    }

    /// <summary>Service que espera até a requisição ser cancelada e avisa quando recebeu o cancelamento.</summary>
    private sealed class ParadaServiceLento : IParadaService
    {
        public TaskCompletionSource Iniciou { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource Cancelado { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task<PaginaDto<ParadaDto>> BuscarAsync(ConsultaParadas consulta, CancellationToken cancellationToken)
        {
            using CancellationTokenRegistration registro = cancellationToken.Register(() => Cancelado.TrySetResult());
            Iniciou.TrySetResult();
            await Task.Delay(Timeout.Infinite, cancellationToken);
            throw new InvalidOperationException("Não deveria chegar aqui.");
        }

        public Task<ParadaDetalheDto> ObterDetalheAsync(int id, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }

    [Fact]
    public async Task ErroInesperado_Responde500GenericoSemDadosSensiveis()
    {
        var logs = new LogsEmMemoria();
        await using ApiTeste api = await ApiTeste.CriarAsync(builder =>
        {
            builder.ConfigureTestServices(services => services.AddScoped<IParadaService, ParadaServiceComFalha>());
            builder.ConfigureLogging(logging => logging.AddProvider(logs));
        });

        ApiTeste.Resposta resposta = await api.GetAsync("/api/v1/paradas?termo=centro");

        Assert.Equal(HttpStatusCode.InternalServerError, resposta.Status);
        Assert.Equal("application/problem+json", resposta.TipoMidia);
        JsonNode json = resposta.Json;
        Assert.Equal("ErroInterno", json["codigo"]!.GetValue<string>());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.6.1", json["type"]!.GetValue<string>());
        Assert.Equal("Erro interno", json["title"]!.GetValue<string>());
        Assert.Equal("Ocorreu um erro inesperado. Tente novamente mais tarde.", json["detail"]!.GetValue<string>());
        Assert.NotNull(json["traceId"]);
        Assert.Null(json["errors"]);
        foreach (string proibido in new[] { "segredo", "Data Source", "Password", "InvalidOperationException", "ParadaServiceComFalha", " at " })
        {
            Assert.DoesNotContain(proibido, resposta.Corpo, StringComparison.OrdinalIgnoreCase);
        }

        // O detalhe fica só no log do servidor.
        Assert.Contains(logs.Entradas, e => e.Nivel == LogLevel.Error && e.Categoria.EndsWith("TratadorExcecoes", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Cancelamento_ChegaAoServiceENaoViraErroInterno()
    {
        var logs = new LogsEmMemoria();
        var lento = new ParadaServiceLento();
        await using ApiTeste api = await ApiTeste.CriarAsync(builder =>
        {
            builder.ConfigureTestServices(services => services.AddSingleton<IParadaService>(lento));
            builder.ConfigureLogging(logging => logging.AddProvider(logs).AddFilter("Microsoft.AspNetCore.Diagnostics", LogLevel.Debug));
        });
        using HttpClient cliente = api.Cliente();
        using var cancelamento = CancellationTokenSource.CreateLinkedTokenSource(Ct);

        Task<HttpResponseMessage> requisicao = cliente.GetAsync(new Uri("/api/v1/paradas", UriKind.Relative), cancelamento.Token);
        await lento.Iniciou.Task.WaitAsync(TimeSpan.FromSeconds(10), Ct);
        await cancelamento.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => requisicao);
        await lento.Cancelado.Task.WaitAsync(TimeSpan.FromSeconds(10), Ct);

        // O ExceptionHandlerMiddleware reconhece o cancelamento e não chama o tratamento de erros:
        // nenhum log de erro e nenhum ProblemDetails 500.
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

    // ----- CORS -----

    [Fact]
    public async Task Cors_ComOrigemConfigurada_LiberaSoEssaOrigemESemCredenciais()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync(builder => builder.UseSetting("Cors:OrigensPermitidas:0", OrigemPermitida));

        ApiTeste.Resposta permitida = await api.GetAsync("/api/v1/linhas", OrigemPermitida);
        ApiTeste.Resposta naoPermitida = await api.GetAsync("/api/v1/linhas", OrigemNaoPermitida);

        Assert.Equal(HttpStatusCode.OK, permitida.Status);
        Assert.Equal(OrigemPermitida, Assert.Single(permitida.Cabecalhos.GetValues("Access-Control-Allow-Origin")));
        Assert.False(permitida.Cabecalhos.Contains("Access-Control-Allow-Credentials"));

        // O navegador bloqueia a leitura: a resposta sai sem o cabeçalho de liberação.
        Assert.Equal(HttpStatusCode.OK, naoPermitida.Status);
        Assert.False(naoPermitida.Cabecalhos.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task Cors_PreflightDaOrigemConfigurada_AceitaSoGet()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync(builder => builder.UseSetting("Cors:OrigensPermitidas:0", OrigemPermitida));
        using HttpClient cliente = api.Cliente();

        async Task<HttpResponseMessage> PreflightAsync(string origem, string metodo)
        {
            var requisicao = new HttpRequestMessage(HttpMethod.Options, new Uri("/api/v1/paradas", UriKind.Relative));
            requisicao.Headers.Add("Origin", origem);
            requisicao.Headers.Add("Access-Control-Request-Method", metodo);
            return await cliente.SendAsync(requisicao, Ct);
        }

        using HttpResponseMessage get = await PreflightAsync(OrigemPermitida, "GET");
        using HttpResponseMessage post = await PreflightAsync(OrigemPermitida, "POST");
        using HttpResponseMessage outraOrigem = await PreflightAsync(OrigemNaoPermitida, "GET");

        Assert.Equal(OrigemPermitida, Assert.Single(get.Headers.GetValues("Access-Control-Allow-Origin")));
        Assert.Contains("GET", get.Headers.GetValues("Access-Control-Allow-Methods"));
        Assert.False(post.Headers.Contains("Access-Control-Allow-Methods") && post.Headers.GetValues("Access-Control-Allow-Methods").Contains("POST"));
        Assert.False(outraOrigem.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task Cors_SemOrigensConfiguradas_NaoLiberaNenhumaOrigem()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();

        ApiTeste.Resposta resposta = await api.GetAsync("/api/v1/linhas", OrigemPermitida);

        Assert.Equal(HttpStatusCode.OK, resposta.Status);
        Assert.False(resposta.Cabecalhos.Contains("Access-Control-Allow-Origin"));
    }

    // ----- OpenAPI, Scalar e rotas futuras -----

    [Theory]
    [InlineData("DELETE", "/api/v1/admin/paradas/{parada}")]
    [InlineData("GET", "/api/v1/admin/paradas")]
    [InlineData("PUT", "/api/v1/admin/linhas")]
    public async Task OperacoesAdministrativasForaDoContrato_Respondem405(string metodo, string modelo)
    {
        // Sem DELETE, sem consulta administrativa e sem atualização de linha (fora do B7).
        await using ApiTeste api = await ApiTeste.CriarAsync();
        int paradaId = await api.IdDaParadaAsync("SIM-P001");

        ApiTeste.Resposta resposta = await api.EnviarAsync(new HttpMethod(metodo), modelo.Replace("{parada}", paradaId.ToString(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal));

        Assert.Equal(HttpStatusCode.MethodNotAllowed, resposta.Status);
        Assert.Equal("MetodoNaoPermitido", resposta.Json["codigo"]!.GetValue<string>());
    }

    [Fact]
    public async Task OpenApi_ListaSomenteAsOperacoesImplementadas()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();

        ApiTeste.Resposta resposta = await api.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, resposta.Status);
        JsonObject caminhos = resposta.Json["paths"]!.AsObject();

        // 11 operações: /health, as 5 consultas públicas do B6 e as 5 rotas administrativas do B7.
        string[] operacoes = [.. caminhos
            .SelectMany(par => par.Value!.AsObject().Select(op => $"{op.Key.ToUpperInvariant()} {par.Key}"))
            .Order(StringComparer.Ordinal)];
        Assert.Equal(
            [
                "GET /api/v1/destinos",
                "GET /api/v1/linhas",
                "GET /api/v1/paradas",
                "GET /api/v1/paradas/{id}",
                "GET /api/v1/paradas/{id}/ultimas-passagens",
                "GET /health",
                "POST /api/v1/admin/linhas",
                "POST /api/v1/admin/observacoes-passagem",
                "POST /api/v1/admin/paradas",
                "PUT /api/v1/admin/paradas/{id}",
                "PUT /api/v1/admin/paradas/{id}/linhas/{linhaId}",
            ],
            operacoes);

        JsonObject respostas = caminhos["/api/v1/paradas/{id}/ultimas-passagens"]!["get"]!["responses"]!.AsObject();
        Assert.Equal(["200", "400", "404", "422", "500"], respostas.Select(par => par.Key).Order(StringComparer.Ordinal));
        Assert.True(respostas["422"]!["content"]!.AsObject().ContainsKey("application/problem+json"));
        Assert.True(respostas["200"]!["content"]!.AsObject().ContainsKey("application/json"));
    }

    [Fact]
    public async Task OpenApiExportado_EstaIgualAoDocumentoGeradoPelaApi()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();
        JsonNode gerado = (await api.GetAsync("/openapi/v1.json")).Json;
        JsonNode exportado = JsonNode.Parse(await File.ReadAllTextAsync(ExemplosContrato.CaminhoNoContrato("openapi/pontovoz-v1.json"), Ct))!;

        // "servers" vem do endereço da requisição (http://localhost:5080/ na exportação, http://localhost/ aqui).
        gerado.AsObject().Remove("servers");
        exportado.AsObject().Remove("servers");
        Assert.True(
            JsonNode.DeepEquals(gerado, exportado),
            "contracts/openapi/pontovoz-v1.json está desatualizado. Rode backend/scripts/exportar-openapi.ps1.");
    }

    [Fact]
    public async Task Scalar_CarregaAPaginaDaDocumentacao()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();

        // /scalar redireciona para /scalar/, que entrega a página.
        ApiTeste.Resposta resposta = await api.GetAsync("/scalar/");

        Assert.Equal(HttpStatusCode.OK, resposta.Status);
        Assert.Equal("text/html", resposta.TipoMidia);
        Assert.Contains("<title>Scalar API Reference</title>", resposta.Corpo, StringComparison.Ordinal);
        Assert.Contains("\"url\":\"openapi/v1.json\"", resposta.Corpo, StringComparison.Ordinal);

        // O script da interface também é servido pela API, sem depender de CDN.
        ApiTeste.Resposta script = await api.GetAsync("/scalar/scalar.js");
        Assert.Equal(HttpStatusCode.OK, script.Status);
        Assert.Contains("javascript", script.TipoMidia, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("GET", "/api/v1/paradas/{parada}/previsoes")]
    [InlineData("GET", "/api/v1/paradas/{parada}/alternativas?destinoId=1")]
    [InlineData("GET", "/api/v1/paradas/{parada}/resumo")]
    [InlineData("GET", "/api/v1/admin/destinos")]
    [InlineData("POST", "/api/v1/admin/destinos")]
    public async Task RotasDeB8EB9EAdministrativasExtras_NaoExistem(string metodo, string modelo)
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();
        ApiTeste.Resposta busca = await api.GetAsync("/api/v1/paradas?codigoExterno=SIM-P001");
        int paradaId = busca.Json["itens"]![0]!["id"]!.GetValue<int>();
        using HttpClient cliente = api.Cliente();
        using var requisicao = new HttpRequestMessage(new HttpMethod(metodo), new Uri(modelo.Replace("{parada}", paradaId.ToString(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal), UriKind.Relative));
        if (metodo == "POST")
        {
            requisicao.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        }

        using HttpResponseMessage resposta = await cliente.SendAsync(requisicao, Ct);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        JsonNode json = JsonNode.Parse(await resposta.Content.ReadAsStringAsync(Ct))!;
        Assert.Equal("RotaNaoEncontrada", json["codigo"]!.GetValue<string>());
    }
}

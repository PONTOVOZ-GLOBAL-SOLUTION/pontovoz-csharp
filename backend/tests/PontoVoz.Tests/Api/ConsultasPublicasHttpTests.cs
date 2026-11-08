using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

using PontoVoz.Tests.Application;

namespace PontoVoz.Tests.Api;

/// <summary>Uma API com banco de demonstração para os testes só de leitura desta classe.</summary>
public sealed class ApiDemonstracaoFixture : IAsyncLifetime
{
    public ApiTeste Api { get; private set; } = null!;

    public async ValueTask InitializeAsync() => Api = await ApiTeste.CriarAsync();

    public async ValueTask DisposeAsync() => await Api.DisposeAsync();
}

/// <summary>
/// Rotas públicas do B6 chamadas por HTTP, com o JSON comparado aos exemplos do contrato. Os ids
/// internos vêm sempre de respostas da própria API (contrato, seção 1.2), nunca de valores fixos.
/// </summary>
public sealed class ConsultasPublicasHttpTests(ApiDemonstracaoFixture fixture) : IClassFixture<ApiDemonstracaoFixture>
{
    private const string JsonSucesso = "application/json";
    private const string JsonProblema = "application/problem+json";

    private ApiTeste Api => fixture.Api;

    private async Task<int> IdDaParadaAsync(string codigoExterno)
    {
        ApiTeste.Resposta resposta = await Api.GetAsync($"/api/v1/paradas?codigoExterno={codigoExterno}");
        return resposta.Json["itens"]!.AsArray().Single()!["id"]!.GetValue<int>();
    }

    private async Task<Dictionary<string, int>> IdsDasLinhasAsync()
    {
        ApiTeste.Resposta resposta = await Api.GetAsync("/api/v1/linhas");
        return resposta.Json["itens"]!.AsArray().ToDictionary(l => l!["codigo"]!.GetValue<string>(), l => l!["id"]!.GetValue<int>());
    }

    /// <summary>Confere o formato comum dos erros: status, Content-Type, <c>codigo</c>, <c>traceId</c> e <c>instance</c>.</summary>
    private static JsonNode AssertProblema(ApiTeste.Resposta resposta, HttpStatusCode status, string? codigo, string caminho)
    {
        Assert.Equal(status, resposta.Status);
        Assert.Equal(JsonProblema, resposta.TipoMidia);
        JsonNode json = resposta.Json;
        Assert.Equal((int)status, json["status"]!.GetValue<int>());
        Assert.Equal(codigo, json["codigo"]?.GetValue<string>());
        Assert.Matches("^00-[0-9a-f]{32}-[0-9a-f]{16}-0[01]$", json["traceId"]!.GetValue<string>());
        Assert.Equal(caminho, json["instance"]!.GetValue<string>());
        return json;
    }

    /// <summary>Retira o <c>traceId</c>, que muda a cada requisição, para comparar o erro com o exemplo.</summary>
    private static JsonNode SemTraceId(JsonNode json)
    {
        JsonNode copia = json.DeepClone();
        copia.AsObject().Remove("traceId");
        return copia;
    }

    // ----- Paradas -----

    [Fact]
    public async Task Paradas_SemFiltro_Responde200ComPaginacaoPadrao()
    {
        ApiTeste.Resposta resposta = await Api.GetAsync("/api/v1/paradas");

        Assert.Equal(HttpStatusCode.OK, resposta.Status);
        Assert.Equal(JsonSucesso, resposta.TipoMidia);
        Assert.Equal("utf-8", resposta.ContentType!.CharSet);
        JsonNode json = resposta.Json;
        Assert.Equal(1, json["pagina"]!.GetValue<int>());
        Assert.Equal(20, json["tamanhoPagina"]!.GetValue<int>());
        Assert.Equal(2, json["totalItens"]!.GetValue<int>());
        Assert.Equal(1, json["totalPaginas"]!.GetValue<int>());
        Assert.Equal(["SIM-P002", "SIM-P001"], json["itens"]!.AsArray().Select(p => p!["codigoExterno"]!.GetValue<string>()));
    }

    [Fact]
    public async Task Paradas_PorTermoSemAcento_EIgualAoExemploDeListagem()
    {
        ApiTeste.Resposta resposta = await Api.GetAsync("/api/v1/paradas?termo=demonstracao");

        Assert.Equal(HttpStatusCode.OK, resposta.Status);
        ExemplosContrato.AssertJsonIgualAoExemplo(ExemplosContrato.Exemplo("paradas-listagem.json"), resposta.Json);
    }

    [Theory]
    [InlineData("SIM-P001")]
    [InlineData("sim-p001")]
    public async Task Paradas_PorCodigoExterno_EIgualAoExemploDaBuscaPorCodigo(string codigo)
    {
        ApiTeste.Resposta resposta = await Api.GetAsync($"/api/v1/paradas?codigoExterno={codigo}");

        Assert.Equal(HttpStatusCode.OK, resposta.Status);
        ExemplosContrato.AssertJsonIgualAoExemplo(ExemplosContrato.Exemplo("paradas-busca-codigo.json"), resposta.Json);
    }

    [Fact]
    public async Task Paradas_PaginaAlemDoTotal_Responde200ComListaVazia()
    {
        ApiTeste.Resposta resposta = await Api.GetAsync("/api/v1/paradas?pagina=5&tamanhoPagina=1");

        Assert.Equal(HttpStatusCode.OK, resposta.Status);
        JsonNode json = resposta.Json;
        Assert.Empty(json["itens"]!.AsArray());
        Assert.Equal(5, json["pagina"]!.GetValue<int>());
        Assert.Equal(1, json["tamanhoPagina"]!.GetValue<int>());
        Assert.Equal(2, json["totalItens"]!.GetValue<int>());
        Assert.Equal(2, json["totalPaginas"]!.GetValue<int>());
    }

    [Fact]
    public async Task Paradas_ComTermoETamanhoInvalidos_Responde400IgualAoExemploDeValidacao()
    {
        ApiTeste.Resposta resposta = await Api.GetAsync("/api/v1/paradas?termo=a&tamanhoPagina=0");

        JsonNode json = AssertProblema(resposta, HttpStatusCode.BadRequest, "RequisicaoInvalida", "/api/v1/paradas");
        Assert.Equal(["termo", "tamanhoPagina"], json["errors"]!.AsObject().Select(par => par.Key));
        Assert.True(JsonNode.DeepEquals(SemTraceId(ExemplosContrato.Exemplo("erro-validacao.json")), SemTraceId(json)), json.ToJsonString());
    }

    [Theory]
    [InlineData("pagina=0", "pagina")]
    [InlineData("tamanhoPagina=51", "tamanhoPagina")]
    [InlineData("pagina=abc", "pagina")]
    [InlineData("tamanhoPagina=1.5", "tamanhoPagina")]
    [InlineData("pagina=99999999999", "pagina")]
    public async Task Paradas_ComPaginacaoInvalida_Responde400NoCampoCerto(string query, string campo)
    {
        ApiTeste.Resposta resposta = await Api.GetAsync($"/api/v1/paradas?{query}");

        JsonNode json = AssertProblema(resposta, HttpStatusCode.BadRequest, "RequisicaoInvalida", "/api/v1/paradas");
        Assert.Equal("A requisição contém parâmetros inválidos.", json["title"]!.GetValue<string>());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.1", json["type"]!.GetValue<string>());
        Assert.Equal(campo, Assert.Single(json["errors"]!.AsObject()).Key);
    }

    [Fact]
    public async Task Paradas_ValorInvalidoNaQuery_NaoRepeteOValorNaMensagem()
    {
        ApiTeste.Resposta resposta = await Api.GetAsync("/api/v1/paradas?pagina=valor-recebido");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.Status);
        Assert.DoesNotContain("valor-recebido", resposta.Corpo);
        Assert.Equal("O valor informado para pagina é inválido.", resposta.Json["errors"]!["pagina"]![0]!.GetValue<string>());
    }

    [Fact]
    public async Task Parada_Existente_EIgualAoExemploDeDetalhe()
    {
        int id = await IdDaParadaAsync("SIM-P001");

        ApiTeste.Resposta resposta = await Api.GetAsync($"/api/v1/paradas/{id}");

        Assert.Equal(HttpStatusCode.OK, resposta.Status);
        Assert.Equal(id, resposta.Json["id"]!.GetValue<int>());
        ExemplosContrato.AssertJsonIgualAoExemplo(ExemplosContrato.Exemplo("parada-detalhe.json"), resposta.Json);
    }

    [Fact]
    public async Task Parada_Inexistente_Responde404IgualAoExemplo()
    {
        // O exemplo usa o id 99; o teste confirma que ele não existe neste banco em vez de supor.
        ApiTeste.Resposta todas = await Api.GetAsync("/api/v1/paradas?tamanhoPagina=50");
        Assert.DoesNotContain(99, todas.Json["itens"]!.AsArray().Select(p => p!["id"]!.GetValue<int>()));

        ApiTeste.Resposta resposta = await Api.GetAsync("/api/v1/paradas/99");

        JsonNode json = AssertProblema(resposta, HttpStatusCode.NotFound, "ParadaNaoEncontrada", "/api/v1/paradas/99");
        Assert.Null(json["errors"]);

        // O exemplo do contrato foi escrito para a rota de previsões (B7): só o instance muda.
        JsonNode esperado = SemTraceId(ExemplosContrato.Exemplo("erro-parada-nao-encontrada.json"));
        esperado["instance"] = "/api/v1/paradas/99";
        Assert.True(JsonNode.DeepEquals(esperado, SemTraceId(json)), json.ToJsonString());
    }

    [Theory]
    [InlineData("/api/v1/paradas/abc")]
    [InlineData("/api/v1/paradas/0")]
    [InlineData("/api/v1/paradas/-1")]
    [InlineData("/api/v1/paradas/1.5")]
    [InlineData("/api/v1/paradas/99999999999")]
    [InlineData("/api/v1/paradas/abc/ultimas-passagens")]
    [InlineData("/api/v1/paradas/0/ultimas-passagens")]
    [InlineData("/api/v1/rota-inexistente")]
    public async Task RotaSemCorrespondencia_Responde404RotaNaoEncontrada(string caminho)
    {
        ApiTeste.Resposta resposta = await Api.GetAsync(caminho);

        // Caminho que não corresponde a nenhuma rota (inclusive {id:int:min(1)}): 404 com código próprio, sem 500.
        JsonNode json = AssertProblema(resposta, HttpStatusCode.NotFound, "RotaNaoEncontrada", caminho);
        Assert.Equal("Rota não encontrada", json["title"]!.GetValue<string>());
        Assert.Equal("Nenhuma rota da API corresponde ao caminho informado.", json["detail"]!.GetValue<string>());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", json["type"]!.GetValue<string>());
    }

    [Fact]
    public async Task MetodoNaoAceito_Responde405MetodoNaoPermitido()
    {
        using HttpClient cliente = Api.Cliente();
        using HttpResponseMessage resposta = await cliente.PostAsync(new Uri("/api/v1/paradas", UriKind.Relative), new StringContent("{}"), ApiTeste.Ct);
        string corpo = await resposta.Content.ReadAsStringAsync(ApiTeste.Ct);

        Assert.Equal(HttpStatusCode.MethodNotAllowed, resposta.StatusCode);
        Assert.Equal(["GET"], resposta.Content.Headers.Allow);
        AssertProblema(new ApiTeste.Resposta(resposta.StatusCode, resposta.Content.Headers.ContentType, resposta.Headers, corpo),
            HttpStatusCode.MethodNotAllowed, "MetodoNaoPermitido", "/api/v1/paradas");
    }

    // ----- Linhas e destinos -----

    [Fact]
    public async Task Linhas_SemFiltro_Responde200OrdenadasPeloCodigo()
    {
        ApiTeste.Resposta resposta = await Api.GetAsync("/api/v1/linhas");

        Assert.Equal(HttpStatusCode.OK, resposta.Status);
        Assert.Equal(JsonSucesso, resposta.TipoMidia);
        Assert.Equal(["SIM-L001", "SIM-L002", "SIM-L003", "SIM-L004"], resposta.Json["itens"]!.AsArray().Select(l => l!["codigo"]!.GetValue<string>()));
        Assert.Equal(4, resposta.Json["totalItens"]!.GetValue<int>());
    }

    [Fact]
    public async Task Linhas_PorTermo_EIgualAoExemploDoDocumento()
    {
        ApiTeste.Resposta resposta = await Api.GetAsync("/api/v1/linhas?termo=104");

        Assert.Equal(HttpStatusCode.OK, resposta.Status);
        ExemplosContrato.AssertJsonIgualAoExemplo(ExemplosContrato.BlocoDoDocumento("Exemplo de `GET /api/v1/linhas?termo=104`"), resposta.Json);
    }

    [Fact]
    public async Task Linhas_PorTerminalSemAcento_FiltraAsLinhas()
    {
        ApiTeste.Resposta resposta = await Api.GetAsync("/api/v1/linhas?termo=demonstracao%20sul");

        Assert.Equal(HttpStatusCode.OK, resposta.Status);
        Assert.Equal("SIM-L003", Assert.Single(resposta.Json["itens"]!.AsArray())!["codigo"]!.GetValue<string>());
    }

    [Fact]
    public async Task Destinos_SemFiltro_Responde200OrdenadosPeloNome()
    {
        ApiTeste.Resposta resposta = await Api.GetAsync("/api/v1/destinos");

        Assert.Equal(HttpStatusCode.OK, resposta.Status);
        Assert.Equal(JsonSucesso, resposta.TipoMidia);
        Assert.Equal(
            ["Terminal Demonstração Centro", "Terminal Demonstração Norte", "Terminal Demonstração Sul"],
            resposta.Json["itens"]!.AsArray().Select(d => d!["nome"]!.GetValue<string>()));
    }

    [Fact]
    public async Task Destinos_PorTermo_EIgualAoExemplo()
    {
        ApiTeste.Resposta resposta = await Api.GetAsync("/api/v1/destinos?termo=norte");

        Assert.Equal(HttpStatusCode.OK, resposta.Status);
        ExemplosContrato.AssertJsonIgualAoExemplo(ExemplosContrato.Exemplo("destinos-busca.json"), resposta.Json);
    }

    [Theory]
    [InlineData("/api/v1/linhas?termo=x", "termo")]
    [InlineData("/api/v1/linhas?pagina=x", "pagina")]
    [InlineData("/api/v1/destinos?tamanhoPagina=0", "tamanhoPagina")]
    [InlineData("/api/v1/destinos?pagina=-1", "pagina")]
    public async Task LinhasEDestinos_ComParametroInvalido_Respondem400(string caminho, string campo)
    {
        ApiTeste.Resposta resposta = await Api.GetAsync(caminho);

        JsonNode json = AssertProblema(resposta, HttpStatusCode.BadRequest, "RequisicaoInvalida", caminho[..caminho.IndexOf('?', StringComparison.Ordinal)]);
        Assert.Equal(campo, Assert.Single(json["errors"]!.AsObject()).Key);
    }

    // ----- Últimas passagens -----

    [Fact]
    public async Task UltimasPassagens_EIgualAoExemploDoContrato()
    {
        int id = await IdDaParadaAsync("SIM-P001");

        ApiTeste.Resposta resposta = await Api.GetAsync($"/api/v1/paradas/{id}/ultimas-passagens");

        Assert.Equal(HttpStatusCode.OK, resposta.Status);
        Assert.Equal(JsonSucesso, resposta.TipoMidia);
        Assert.Equal(id, resposta.Json["paradaId"]!.GetValue<int>());
        ExemplosContrato.AssertJsonIgualAoExemplo(ExemplosContrato.Exemplo("ultimas-passagens.json"), resposta.Json);
    }

    [Fact]
    public async Task UltimasPassagens_SeparaManualSimuladaDesatualizadaESemRegistro()
    {
        int id = await IdDaParadaAsync("SIM-P001");

        JsonNode json = (await Api.GetAsync($"/api/v1/paradas/{id}/ultimas-passagens")).Json;
        Dictionary<string, JsonNode> porLinha = json["passagens"]!.AsArray().ToDictionary(p => p!["codigoLinha"]!.GetValue<string>(), p => p!);

        JsonNode manual = porLinha["SIM-L004"];
        Assert.Equal("RegistroManual", manual["nivelConfirmacao"]!.GetValue<string>());
        Assert.Equal("operador-local", manual["origemRegistro"]!.GetValue<string>());
        Assert.False(manual["desatualizada"]!.GetValue<bool>());

        JsonNode simulada = porLinha["SIM-L002"];
        Assert.Equal("ObservacaoSimulada", simulada["nivelConfirmacao"]!.GetValue<string>());
        Assert.True(simulada["desatualizada"]!.GetValue<bool>());
        Assert.Contains("PassagemDesatualizada", json["avisos"]!.AsArray().Select(a => a!["codigo"]!.GetValue<string>()));

        Assert.Equal("SIM-L001", Assert.Single(json["linhasSemRegistro"]!.AsArray())!["codigoLinha"]!.GetValue<string>());
    }

    [Fact]
    public async Task UltimasPassagens_ComLinhaIdValido_LimitaAUmaLinha()
    {
        int paradaId = await IdDaParadaAsync("SIM-P001");
        int linhaId = (await IdsDasLinhasAsync())["SIM-L004"];

        ApiTeste.Resposta resposta = await Api.GetAsync($"/api/v1/paradas/{paradaId}/ultimas-passagens?linhaId={linhaId}");

        Assert.Equal(HttpStatusCode.OK, resposta.Status);
        JsonNode passagem = Assert.Single(resposta.Json["passagens"]!.AsArray())!;
        Assert.Equal(linhaId, passagem["linhaId"]!.GetValue<int>());
        Assert.Empty(resposta.Json["linhasSemRegistro"]!.AsArray());
    }

    [Fact]
    public async Task UltimasPassagens_SemRegistroDaLinhaFiltrada_ListaALinhaSemRegistro()
    {
        int paradaId = await IdDaParadaAsync("SIM-P001");
        int linhaId = (await IdsDasLinhasAsync())["SIM-L001"];

        JsonNode json = (await Api.GetAsync($"/api/v1/paradas/{paradaId}/ultimas-passagens?linhaId={linhaId}")).Json;

        Assert.Empty(json["passagens"]!.AsArray());
        Assert.Equal(linhaId, Assert.Single(json["linhasSemRegistro"]!.AsArray())!["linhaId"]!.GetValue<int>());
        Assert.Contains("PassagemNaoRegistrada", json["avisos"]!.AsArray().Select(a => a!["codigo"]!.GetValue<string>()));
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-3")]
    public async Task UltimasPassagens_ComLinhaIdInvalido_Responde400(string linhaId)
    {
        int paradaId = await IdDaParadaAsync("SIM-P001");
        string caminho = $"/api/v1/paradas/{paradaId}/ultimas-passagens";

        ApiTeste.Resposta resposta = await Api.GetAsync($"{caminho}?linhaId={linhaId}");

        JsonNode json = AssertProblema(resposta, HttpStatusCode.BadRequest, "RequisicaoInvalida", caminho);
        Assert.Equal("linhaId", Assert.Single(json["errors"]!.AsObject()).Key);
    }

    [Fact]
    public async Task UltimasPassagens_ComLinhaInexistente_Responde404()
    {
        int paradaId = await IdDaParadaAsync("SIM-P001");
        int inexistente = (await IdsDasLinhasAsync()).Values.Max() + 1000;
        string caminho = $"/api/v1/paradas/{paradaId}/ultimas-passagens";

        ApiTeste.Resposta resposta = await Api.GetAsync($"{caminho}?linhaId={inexistente}");

        JsonNode json = AssertProblema(resposta, HttpStatusCode.NotFound, "LinhaNaoEncontrada", caminho);
        Assert.Equal($"Não existe linha ativa com o id {inexistente}.", json["detail"]!.GetValue<string>());
    }

    [Fact]
    public async Task UltimasPassagens_ComParadaInexistente_Responde404()
    {
        ApiTeste.Resposta resposta = await Api.GetAsync("/api/v1/paradas/99/ultimas-passagens");

        AssertProblema(resposta, HttpStatusCode.NotFound, "ParadaNaoEncontrada", "/api/v1/paradas/99/ultimas-passagens");
    }

    [Fact]
    public async Task UltimasPassagens_ComLinhaQueNaoAtendeAParada_Responde422()
    {
        int paradaId = await IdDaParadaAsync("SIM-P001");
        int linhaId = (await IdsDasLinhasAsync())["SIM-L003"];
        string caminho = $"/api/v1/paradas/{paradaId}/ultimas-passagens";

        ApiTeste.Resposta resposta = await Api.GetAsync($"{caminho}?linhaId={linhaId}");

        JsonNode json = AssertProblema(resposta, (HttpStatusCode)422, "LinhaNaoAtendeParada", caminho);
        Assert.Equal("Linha não atende a parada", json["title"]!.GetValue<string>());
        Assert.Equal($"A linha {linhaId} não atende a parada {paradaId}.", json["detail"]!.GetValue<string>());
        Assert.Null(json["errors"]);
    }

    // ----- Formato do JSON -----

    [Fact]
    public async Task Json_EnumsSaoTextoENullsSaoEnviados()
    {
        int id = await IdDaParadaAsync("SIM-P001");

        using JsonDocument parada = JsonDocument.Parse((await Api.GetAsync($"/api/v1/paradas/{id}")).Corpo);
        using JsonDocument passagens = JsonDocument.Parse((await Api.GetAsync($"/api/v1/paradas/{id}/ultimas-passagens")).Corpo);

        JsonElement detalhe = parada.RootElement;
        Assert.Equal(JsonValueKind.String, detalhe.GetProperty("origem").ValueKind);
        Assert.Equal(JsonValueKind.String, detalhe.GetProperty("linhas")[0].GetProperty("sentido").ValueKind);
        Assert.Equal(JsonValueKind.Null, detalhe.GetProperty("endereco").ValueKind);
        Assert.Equal(JsonValueKind.Null, detalhe.GetProperty("coordenada").ValueKind);

        JsonElement manual = passagens.RootElement.GetProperty("passagens").EnumerateArray()
            .Single(p => p.GetProperty("codigoLinha").GetString() == "SIM-L004");
        Assert.Equal(JsonValueKind.String, manual.GetProperty("nivelConfirmacao").ValueKind);
        Assert.Equal(JsonValueKind.Null, manual.GetProperty("prefixoVeiculo").ValueKind);
    }

    [Fact]
    public async Task Health_ComBancoDeDemonstracao_Responde200()
    {
        ApiTeste.Resposta resposta = await Api.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, resposta.Status);
        Assert.Equal("Healthy", resposta.Json["status"]!.GetValue<string>());
    }
}

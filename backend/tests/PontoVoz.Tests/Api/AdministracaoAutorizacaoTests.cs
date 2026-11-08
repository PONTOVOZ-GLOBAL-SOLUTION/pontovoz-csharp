using System.Net;
using System.Text.Json.Nodes;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace PontoVoz.Tests.Api;

/// <summary>
/// Chave administrativa (contracts/api-v1.md, seção 5.1), pela API real: a matriz de ambiente e
/// chave, a ordem (autorização antes da validação do corpo), o 401 com <c>WWW-Authenticate</c>,
/// o 403 e a ausência da chave em respostas e logs.
/// </summary>
public sealed class AdministracaoAutorizacaoTests
{
    /// <summary>Valor fictício, só de teste. Não é e não foi usado como chave real.</summary>
    private const string ChaveDeTeste = "chave-de-teste-b7-nao-real";

    private const string ChaveErradaDeTeste = "tentativa-errada-de-teste";

    private const string Desafio = "ApiKey header=\"X-PontoVoz-Admin-Key\"";

    /// <summary>JSON malformado: se a autorização passar, a resposta é 400; se não passar, 401 ou 403 (nunca 400).</summary>
    private const string CorpoInvalido = "{ isto não é JSON";

    private static Dictionary<string, string>? Cabecalho(string? valor) =>
        valor is null ? null : new Dictionary<string, string> { ["X-PontoVoz-Admin-Key"] = valor };

    public static TheoryData<string, string?, string?, HttpStatusCode, string> Matriz => new()
    {
        // ambiente, chave configurada, cabeçalho enviado, status esperado, código esperado
        { "Development", null, null, HttpStatusCode.BadRequest, "RequisicaoInvalida" },
        { "Development", null, ChaveErradaDeTeste, HttpStatusCode.BadRequest, "RequisicaoInvalida" },
        { "Development", ChaveDeTeste, ChaveDeTeste, HttpStatusCode.BadRequest, "RequisicaoInvalida" },
        { "Development", ChaveDeTeste, null, HttpStatusCode.Unauthorized, "ChaveAdministrativaInvalida" },
        { "Development", ChaveDeTeste, ChaveErradaDeTeste, HttpStatusCode.Unauthorized, "ChaveAdministrativaInvalida" },
        { "Production", null, null, HttpStatusCode.Forbidden, "OperacoesAdministrativasDesabilitadas" },
        { "Production", null, ChaveDeTeste, HttpStatusCode.Forbidden, "OperacoesAdministrativasDesabilitadas" },
        { "Production", ChaveDeTeste, ChaveDeTeste, HttpStatusCode.BadRequest, "RequisicaoInvalida" },
        { "Production", ChaveDeTeste, null, HttpStatusCode.Unauthorized, "ChaveAdministrativaInvalida" },
        { "Production", ChaveDeTeste, ChaveErradaDeTeste, HttpStatusCode.Unauthorized, "ChaveAdministrativaInvalida" },
        { "Staging", null, null, HttpStatusCode.Forbidden, "OperacoesAdministrativasDesabilitadas" },
    };

    [Theory]
    [MemberData(nameof(Matriz))]
    public async Task Matriz_AmbienteEChave_DecideAntesDaValidacaoDoCorpo(string ambiente, string? chave, string? cabecalho, HttpStatusCode esperado, string codigo)
    {
        await using ApiTeste api = await ApiTeste.CriarAsync(chaveAdministrativa: chave, ambiente: ambiente);

        ApiTeste.Resposta resposta = await api.EnviarAsync(HttpMethod.Post, "/api/v1/admin/paradas", CorpoInvalido, Cabecalho(cabecalho));

        Assert.Equal(esperado, resposta.Status);
        Assert.Equal("application/problem+json", resposta.TipoMidia);
        JsonNode json = resposta.Json;
        Assert.Equal(codigo, json["codigo"]!.GetValue<string>());
        Assert.NotNull(json["traceId"]);
        Assert.Equal("/api/v1/admin/paradas", json["instance"]!.GetValue<string>());
        Assert.Equal(esperado == HttpStatusCode.Unauthorized, resposta.Cabecalhos.WwwAuthenticate.Count > 0);
    }

    [Fact]
    public async Task Sem401_CabecalhoWwwAuthenticateNoFormatoDoContrato()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync(chaveAdministrativa: ChaveDeTeste);

        ApiTeste.Resposta resposta = await api.EnviarAsync(HttpMethod.Post, "/api/v1/admin/linhas", "{}");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.Status);
        Assert.Equal(Desafio, Assert.Single(resposta.Cabecalhos.GetValues("WWW-Authenticate")));
        Assert.Equal("Chave administrativa inválida", resposta.Json["title"]!.GetValue<string>());
        Assert.Null(resposta.Json["errors"]);
    }

    [Fact]
    public async Task ComChaveCorreta_OperacaoAdministrativaAcontece()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync(chaveAdministrativa: ChaveDeTeste, ambiente: "Production");

        ApiTeste.Resposta resposta = await api.EnviarAsync(HttpMethod.Post, "/api/v1/admin/paradas",
            """{"codigoExterno":"TESTE-P010","nome":"Parada com Chave","dadosSimulados":true}""", Cabecalho(ChaveDeTeste));

        Assert.Equal(HttpStatusCode.Created, resposta.Status);
    }

    [Theory]
    [InlineData("POST", "/api/v1/admin/paradas")]
    [InlineData("PUT", "/api/v1/admin/paradas/1")]
    [InlineData("POST", "/api/v1/admin/linhas")]
    [InlineData("PUT", "/api/v1/admin/paradas/1/linhas/1")]
    [InlineData("POST", "/api/v1/admin/observacoes-passagem")]
    public async Task TodasAsRotasAdministrativas_PassamPeloFiltro(string metodo, string caminho)
    {
        await using ApiTeste producaoSemChave = await ApiTeste.CriarAsync(ambiente: "Production");
        await using ApiTeste comChave = await ApiTeste.CriarAsync(chaveAdministrativa: ChaveDeTeste);

        ApiTeste.Resposta desabilitada = await producaoSemChave.EnviarAsync(new HttpMethod(metodo), caminho, CorpoInvalido);
        ApiTeste.Resposta semChave = await comChave.EnviarAsync(new HttpMethod(metodo), caminho, CorpoInvalido);

        Assert.Equal(HttpStatusCode.Forbidden, desabilitada.Status);
        Assert.Equal(HttpStatusCode.Unauthorized, semChave.Status);
    }

    [Fact]
    public async Task ChaveNaoAparece_NemNasRespostasNemNosLogs()
    {
        var logs = new LogsEmMemoria();
        await using ApiTeste api = await ApiTeste.CriarAsync(
            builder => builder.ConfigureLogging(logging => logging.AddProvider(logs)),
            chaveAdministrativa: ChaveDeTeste);

        ApiTeste.Resposta errada = await api.EnviarAsync(HttpMethod.Post, "/api/v1/admin/paradas", CorpoInvalido, Cabecalho(ChaveErradaDeTeste));
        ApiTeste.Resposta certa = await api.EnviarAsync(HttpMethod.Post, "/api/v1/admin/paradas", CorpoInvalido, Cabecalho(ChaveDeTeste));

        Assert.Equal(HttpStatusCode.Unauthorized, errada.Status);
        Assert.Equal(HttpStatusCode.BadRequest, certa.Status);
        foreach (string texto in new[] { errada.Corpo, certa.Corpo, logs.TextoCompleto })
        {
            Assert.DoesNotContain(ChaveDeTeste, texto, StringComparison.Ordinal);
            Assert.DoesNotContain(ChaveErradaDeTeste, texto, StringComparison.Ordinal);
        }

        Assert.Contains(logs.Entradas, e => e.Nivel == LogLevel.Warning && e.Mensagem.Contains("chave ausente ou incorreta", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("Development", null, "SEM proteção por chave")]
    [InlineData("Production", null, "estão desabilitadas")]
    [InlineData("Development", ChaveDeTeste, null)]
    public async Task Inicializacao_AvisaNoLogQuandoNaoHaChave(string ambiente, string? chave, string? trechoEsperado)
    {
        var logs = new LogsEmMemoria();
        await using ApiTeste api = await ApiTeste.CriarAsync(
            builder => builder.ConfigureLogging(logging => logging.AddProvider(logs)),
            chaveAdministrativa: chave,
            ambiente: ambiente);

        // A primeira requisição inicia o host, e com ele o aviso de inicialização.
        _ = await api.GetAsync("/health");

        var avisos = logs.Entradas.Where(e => e.Categoria.EndsWith("AvisoChaveAdministrativa", StringComparison.Ordinal)).ToList();
        if (trechoEsperado is null)
        {
            Assert.Empty(avisos);
        }
        else
        {
            (string _, LogLevel nivel, string mensagem) = Assert.Single(avisos);
            Assert.Equal(LogLevel.Warning, nivel);
            Assert.Contains(trechoEsperado, mensagem, StringComparison.Ordinal);
        }

        Assert.DoesNotContain(ChaveDeTeste, logs.TextoCompleto, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/api/v1/paradas")]
    [InlineData("/api/v1/linhas")]
    [InlineData("/api/v1/destinos")]
    public async Task RotasPublicas_ContinuamSemChave(string caminho)
    {
        await using ApiTeste api = await ApiTeste.CriarAsync(chaveAdministrativa: ChaveDeTeste, ambiente: "Production");

        ApiTeste.Resposta resposta = await api.GetAsync(caminho);

        Assert.Equal(HttpStatusCode.OK, resposta.Status);
    }
}

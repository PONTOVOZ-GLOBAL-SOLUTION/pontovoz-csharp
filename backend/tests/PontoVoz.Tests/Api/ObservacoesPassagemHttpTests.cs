using System.Globalization;
using System.Net;
using System.Text.Json.Nodes;

using Microsoft.EntityFrameworkCore;

using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Infrastructure.Persistence;
using PontoVoz.Tests.Application;

namespace PontoVoz.Tests.Api;

/// <summary>
/// <c>POST /api/v1/admin/observacoes-passagem</c> (seção 5.6) pela API real: o exemplo do
/// contrato, a janela de 24 horas e 2 minutos (J1 a J5), o cálculo na consulta (4.6.3), a natureza
/// do prefixo do veículo, o rótulo técnico, os erros 404 e 422 e a gravação no SQLite. O relógio
/// é um <c>FakeTimeProvider</c> e o banco começa só com o seed, sem o registro manual do exemplo.
/// </summary>
public sealed class ObservacoesPassagemHttpTests
{
    private const string Rota = "/api/v1/admin/observacoes-passagem";

    private static readonly DateTimeOffset Agora = new(2026, 11, 10, 14, 25, 10, TimeSpan.FromHours(-3));

    private static Task<ApiTeste> ApiAsync(DateTimeOffset? inicio = null) =>
        ApiTeste.CriarAsync(registrarManualDoExemplo: false, inicioRelogio: inicio ?? Agora);

    private static string Instante(DateTimeOffset instante) => instante.ToString("yyyy-MM-dd'T'HH:mm:sszzz", CultureInfo.InvariantCulture);

    private static string Corpo(int paradaId, int linhaId, DateTimeOffset observadaEm, string? prefixo = null, string origem = "operador-local") =>
        new JsonObject
        {
            ["paradaId"] = paradaId,
            ["linhaId"] = linhaId,
            ["observadaEm"] = Instante(observadaEm),
            ["prefixoVeiculo"] = prefixo,
            ["origemRegistro"] = origem,
        }.ToJsonString();

    private static Task<ApiTeste.Resposta> RegistrarAsync(ApiTeste api, string json) => api.EnviarAsync(HttpMethod.Post, Rota, json);

    private static async Task<(int Parada, int Linha)> CentroESimL004Async(ApiTeste api) =>
        (await api.IdDaParadaAsync("SIM-P001"), await api.IdDaLinhaAsync("SIM-L004"));

    private static JsonNode? PassagemDaLinha(JsonNode ultimas, string codigoLinha) =>
        ultimas["passagens"]!.AsArray().SingleOrDefault(p => p!["codigoLinha"]!.GetValue<string>() == codigoLinha);

    private static void AssertErro(ApiTeste.Resposta resposta, HttpStatusCode status, string codigo, string? campo = null)
    {
        Assert.Equal(status, resposta.Status);
        Assert.Equal("application/problem+json", resposta.TipoMidia);
        Assert.Equal(codigo, resposta.Json["codigo"]!.GetValue<string>());
        if (campo is not null)
        {
            Assert.Equal(campo, Assert.Single(resposta.Json["errors"]!.AsObject()).Key);
        }
    }

    [Fact]
    public async Task Registro_DoExemplo_Responde201IgualAoContratoEApareceNasUltimasPassagens()
    {
        await using ApiTeste api = await ApiAsync();
        var (parada, linha) = await CentroESimL004Async(api);
        JsonNode requisicao = ExemplosContrato.Exemplo("admin-observacao-passagem-requisicao.json");
        requisicao["paradaId"] = parada;
        requisicao["linhaId"] = linha;

        ApiTeste.Resposta resposta = await RegistrarAsync(api, requisicao.ToJsonString());

        Assert.Equal(HttpStatusCode.Created, resposta.Status);
        Assert.Null(resposta.Cabecalhos.Location);
        Assert.Equal(parada, resposta.Json["paradaId"]!.GetValue<int>());
        ExemplosContrato.AssertJsonIgualAoExemplo(ExemplosContrato.Exemplo("admin-observacao-passagem-resposta.json"), resposta.Json);

        // Na consulta das 14h32, a mesma passagem aparece com 8 minutos: é a resposta de ultimas-passagens.json.
        api.Relogio.SetUtcNow(ApiTeste.ConsultaDosExemplos);
        ApiTeste.Resposta ultimas = await api.GetAsync($"/api/v1/paradas/{parada}/ultimas-passagens");
        ExemplosContrato.AssertJsonIgualAoExemplo(ExemplosContrato.Exemplo("ultimas-passagens.json"), ultimas.Json);
    }

    [Fact]
    public async Task Registro_GravaNoSqliteEmUtcComoRegistroManualNaoVerificado()
    {
        await using ApiTeste api = await ApiAsync();
        var (parada, linha) = await CentroESimL004Async(api);
        DateTimeOffset observada = Agora.AddMinutes(-3);

        ApiTeste.Resposta resposta = await RegistrarAsync(api, Corpo(parada, linha, observada, "SIM-V4001"));

        JsonNode json = resposta.Json;
        Assert.Equal("RegistroManual", json["nivelConfirmacao"]!.GetValue<string>());
        Assert.False(json["verificada"]!.GetValue<bool>());
        Assert.False(json["oficialSptrans"]!.GetValue<bool>());
        Assert.Equal("RegistroManualNaoVerificado", json["aviso"]!["codigo"]!.GetValue<string>());
        Assert.Equal(3, json["minutosDesdeObservacao"]!.GetValue<int>());
        Assert.Equal(Instante(Agora), json["registradaEm"]!.GetValue<string>());

        await using PontoVozDbContext contexto = api.NovoContexto();
        int id = json["id"]!.GetValue<int>();
        PassagemRegistroManual gravada = await contexto.RegistrosPassagem.OfType<PassagemRegistroManual>().SingleAsync(p => p.Id == id, ApiTeste.Ct);
        Assert.Equal(TimeSpan.Zero, gravada.ObservadaEmUtc.Offset);
        Assert.Equal(observada, gravada.ObservadaEmUtc);
        Assert.Equal(Agora, gravada.RegistradaEmUtc);
        Assert.Equal("operador-local", gravada.OrigemRegistro);
        Assert.Equal("SIM-V4001", gravada.PrefixoVeiculo);
        Assert.True(gravada.DadosSimulados);
    }

    /// <summary>Casos J1 a J5 da seção 4.6.3: intervalo (s) entre o instante do servidor e <c>observadaEm</c>.</summary>
    [Theory]
    [InlineData(-121, HttpStatusCode.BadRequest)]
    [InlineData(-120, HttpStatusCode.Created)]
    [InlineData(0, HttpStatusCode.Created)]
    [InlineData(86400, HttpStatusCode.Created)]
    [InlineData(86401, HttpStatusCode.BadRequest)]
    public async Task Registro_JanelaDe24HorasE2Minutos_NasFronteirasExatas(int intervaloSegundos, HttpStatusCode esperado)
    {
        await using ApiTeste api = await ApiAsync();
        var (parada, linha) = await CentroESimL004Async(api);

        ApiTeste.Resposta resposta = await RegistrarAsync(api, Corpo(parada, linha, Agora.AddSeconds(-intervaloSegundos)));

        Assert.Equal(esperado, resposta.Status);
        if (esperado == HttpStatusCode.BadRequest)
        {
            AssertErro(resposta, HttpStatusCode.BadRequest, "RequisicaoInvalida", "observadaEm");
        }
    }

    [Fact]
    public async Task Registro_NoFuturoDentroDaTolerancia_SoApareceQuandoOHorarioChega()
    {
        await using ApiTeste api = await ApiAsync();
        var (parada, linha) = await CentroESimL004Async(api);
        DateTimeOffset observada = Agora.AddMinutes(2);

        ApiTeste.Resposta resposta = await RegistrarAsync(api, Corpo(parada, linha, observada));

        Assert.Equal(HttpStatusCode.Created, resposta.Status);
        Assert.Equal(0, resposta.Json["minutosDesdeObservacao"]!.GetValue<int>());
        Assert.False(resposta.Json["desatualizada"]!.GetValue<bool>());

        // Ainda no futuro: a SIM-L004 continua sem registro.
        JsonNode antes = (await api.GetAsync($"/api/v1/paradas/{parada}/ultimas-passagens")).Json;
        Assert.Null(PassagemDaLinha(antes, "SIM-L004"));
        Assert.Contains("SIM-L004", antes["linhasSemRegistro"]!.AsArray().Select(l => l!["codigoLinha"]!.GetValue<string>()));

        // Quando o horário chega, a passagem aparece.
        api.Relogio.SetUtcNow(observada);
        JsonNode depois = (await api.GetAsync($"/api/v1/paradas/{parada}/ultimas-passagens")).Json;
        JsonNode passagem = PassagemDaLinha(depois, "SIM-L004")!;
        Assert.Equal(0, passagem["minutosDesdeObservacao"]!.GetValue<int>());
        Assert.Equal("RegistroManual", passagem["nivelConfirmacao"]!.GetValue<string>());
    }

    /// <summary>Casos T5 e T6 da seção 4.6.3: minutos arredondados para baixo e desatualização pelo intervalo exato.</summary>
    [Fact]
    public async Task Consulta_CalculaMinutosEDesatualizacaoPeloIntervaloExato()
    {
        await using ApiTeste api = await ApiAsync();
        var (parada, linha) = await CentroESimL004Async(api);
        Assert.Equal(HttpStatusCode.Created, (await RegistrarAsync(api, Corpo(parada, linha, Agora))).Status);

        api.Relogio.Advance(TimeSpan.FromSeconds(3600));
        JsonNode t5 = PassagemDaLinha((await api.GetAsync($"/api/v1/paradas/{parada}/ultimas-passagens")).Json, "SIM-L004")!;
        api.Relogio.Advance(TimeSpan.FromSeconds(1));
        JsonNode t6 = PassagemDaLinha((await api.GetAsync($"/api/v1/paradas/{parada}/ultimas-passagens")).Json, "SIM-L004")!;

        Assert.Equal(60, t5["minutosDesdeObservacao"]!.GetValue<int>());
        Assert.False(t5["desatualizada"]!.GetValue<bool>());
        Assert.Equal(60, t6["minutosDesdeObservacao"]!.GetValue<int>());
        Assert.True(t6["desatualizada"]!.GetValue<bool>());
    }

    [Theory]
    [InlineData("SIM-V100", HttpStatusCode.Created)]
    [InlineData(null, HttpStatusCode.Created)]
    [InlineData("V1234", HttpStatusCode.BadRequest)]
    [InlineData("SIM V1", HttpStatusCode.BadRequest)]
    [InlineData("SIM-V12345678901234567", HttpStatusCode.BadRequest)]
    public async Task Registro_EmParadaFicticia_ExigePrefixoDeVeiculoSimulado(string? prefixo, HttpStatusCode esperado)
    {
        await using ApiTeste api = await ApiAsync();
        var (parada, linha) = await CentroESimL004Async(api);

        ApiTeste.Resposta resposta = await RegistrarAsync(api, Corpo(parada, linha, Agora, prefixo));

        Assert.Equal(esperado, resposta.Status);
        if (esperado == HttpStatusCode.BadRequest)
        {
            AssertErro(resposta, HttpStatusCode.BadRequest, "RequisicaoInvalida", "prefixoVeiculo");
        }
    }

    [Fact]
    public async Task Registro_ComParadaELinhaNaoFicticias_SaiSemDadosSimuladosEComCadastroNaoVerificado()
    {
        await using ApiTeste api = await ApiAsync();
        int parada = (await api.EnviarAsync(HttpMethod.Post, "/api/v1/admin/paradas",
            """{"codigoExterno":"PT-0100","nome":"Parada Real de Teste","dadosSimulados":false}""")).Json["id"]!.GetValue<int>();
        int linha = (await api.EnviarAsync(HttpMethod.Post, "/api/v1/admin/linhas",
            """{"codigo":"LT-0100","letreiro":"100-A","terminalPrincipal":"Terminal A","terminalSecundario":"Terminal B","sentido":"PrincipalParaSecundario","dadosSimulados":false}""")).Json["id"]!.GetValue<int>();
        Assert.Equal(HttpStatusCode.NoContent, (await api.EnviarAsync(HttpMethod.Put, $"/api/v1/admin/paradas/{parada}/linhas/{linha}")).Status);

        AssertErro(await RegistrarAsync(api, Corpo(parada, linha, Agora, "SIM-V1")), HttpStatusCode.BadRequest, "RequisicaoInvalida", "prefixoVeiculo");
        ApiTeste.Resposta resposta = await RegistrarAsync(api, Corpo(parada, linha, Agora, "V1234"));

        Assert.Equal(HttpStatusCode.Created, resposta.Status);
        Assert.False(resposta.Json["dadosSimulados"]!.GetValue<bool>());
        Assert.False(resposta.Json["verificada"]!.GetValue<bool>());

        JsonNode ultimas = (await api.GetAsync($"/api/v1/paradas/{parada}/ultimas-passagens")).Json;
        Assert.False(ultimas["dadosSimulados"]!.GetValue<bool>());
        string[] avisos = [.. ultimas["avisos"]!.AsArray().Select(a => a!["codigo"]!.GetValue<string>())];
        Assert.Contains("CadastroNaoVerificado", avisos);
        Assert.DoesNotContain("DadosSimulados", avisos);
        Assert.False(PassagemDaLinha(ultimas, "LT-0100")!["oficialSptrans"]!.GetValue<bool>());
    }

    [Theory]
    [InlineData("Operador Local")]
    [InlineData("operador@exemplo.com")]
    [InlineData("(11) 99999-0000")]
    [InlineData("Maria")]
    [InlineData("op")]
    [InlineData("")]
    public async Task Registro_ComOrigemQueNaoERotuloTecnico_Responde400(string origem)
    {
        await using ApiTeste api = await ApiAsync();
        var (parada, linha) = await CentroESimL004Async(api);

        ApiTeste.Resposta resposta = await RegistrarAsync(api, Corpo(parada, linha, Agora, origem: origem));

        AssertErro(resposta, HttpStatusCode.BadRequest, "RequisicaoInvalida", "origemRegistro");
        Assert.DoesNotContain(origem.Length > 0 ? origem : "\u0000", resposta.Corpo, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Registro_SemCamposObrigatorios_DevolveTodosOsErros()
    {
        await using ApiTeste api = await ApiAsync();

        ApiTeste.Resposta resposta = await RegistrarAsync(api, """{"prefixoVeiculo":null}""");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.Status);
        Assert.Equal(
            ["linhaId", "observadaEm", "origemRegistro", "paradaId"],
            resposta.Json["errors"]!.AsObject().Select(par => par.Key).Order(StringComparer.Ordinal));
    }

    [Theory]
    [InlineData("\"2026-11-10T14:24:00\"", HttpStatusCode.BadRequest)]
    [InlineData("\"10/11/2026 14:24\"", HttpStatusCode.BadRequest)]
    [InlineData("1731259440", HttpStatusCode.BadRequest)]
    [InlineData("\"2026-11-10T17:24:00Z\"", HttpStatusCode.Created)]
    [InlineData("\"2026-11-10T14:24:00-03:00\"", HttpStatusCode.Created)]
    public async Task Registro_ObservadaEmPrecisaTerFuso(string valor, HttpStatusCode esperado)
    {
        await using ApiTeste api = await ApiAsync();
        var (parada, linha) = await CentroESimL004Async(api);

        ApiTeste.Resposta resposta = await RegistrarAsync(api,
            $$"""{"paradaId":{{parada}},"linhaId":{{linha}},"observadaEm":{{valor}},"origemRegistro":"operador-local"}""");

        Assert.Equal(esperado, resposta.Status);
        if (esperado == HttpStatusCode.BadRequest)
        {
            AssertErro(resposta, HttpStatusCode.BadRequest, "RequisicaoInvalida", "observadaEm");
        }
        else
        {
            Assert.Equal("2026-11-10T14:24:00-03:00", resposta.Json["observadaEm"]!.GetValue<string>());
        }
    }

    [Theory]
    [InlineData("\"um\"", "paradaId")]
    [InlineData("\"1\"", "paradaId")]
    [InlineData("0", "paradaId")]
    [InlineData("-5", "paradaId")]
    public async Task Registro_ComParadaIdInvalido_Responde400(string valor, string campo)
    {
        await using ApiTeste api = await ApiAsync();
        int linha = await api.IdDaLinhaAsync("SIM-L004");

        ApiTeste.Resposta resposta = await RegistrarAsync(api,
            $$"""{"paradaId":{{valor}},"linhaId":{{linha}},"observadaEm":"{{Instante(Agora)}}","origemRegistro":"operador-local"}""");

        AssertErro(resposta, HttpStatusCode.BadRequest, "RequisicaoInvalida", campo);
    }

    [Fact]
    public async Task Registro_ComParadaOuLinhaInexistente_Responde404DepoisDaValidacao()
    {
        await using ApiTeste api = await ApiAsync();
        var (parada, linha) = await CentroESimL004Async(api);

        AssertErro(await RegistrarAsync(api, Corpo(parada + 1000, linha, Agora)), HttpStatusCode.NotFound, "ParadaNaoEncontrada");
        AssertErro(await RegistrarAsync(api, Corpo(parada, linha + 1000, Agora)), HttpStatusCode.NotFound, "LinhaNaoEncontrada");

        // Ordem da seção 2: um corpo inválido responde 400 antes de procurar a parada.
        AssertErro(await RegistrarAsync(api, Corpo(parada + 1000, linha, Agora.AddDays(-2))), HttpStatusCode.BadRequest, "RequisicaoInvalida", "observadaEm");
    }

    [Fact]
    public async Task Registro_ComLinhaQueNaoAtendeAParada_Responde422()
    {
        await using ApiTeste api = await ApiAsync();
        int parada = await api.IdDaParadaAsync("SIM-P001");
        int linha = await api.IdDaLinhaAsync("SIM-L003");

        ApiTeste.Resposta resposta = await RegistrarAsync(api, Corpo(parada, linha, Agora));

        AssertErro(resposta, (HttpStatusCode)422, "LinhaNaoAtendeParada");
        Assert.Equal("Linha não atende a parada", resposta.Json["title"]!.GetValue<string>());
        await using PontoVozDbContext contexto = api.NovoContexto();
        Assert.False(await contexto.RegistrosPassagem.AnyAsync(p => p.LinhaId == linha && p.ParadaId == parada, ApiTeste.Ct));
    }

    [Fact]
    public async Task Registro_ComParadaInativa_Responde422ParadaInativa()
    {
        await using ApiTeste api = await ApiAsync();
        var (parada, linha) = await CentroESimL004Async(api);
        await api.EnviarAsync(HttpMethod.Put, $"/api/v1/admin/paradas/{parada}", """{"nome":"Parada Demonstração Centro","ativa":false}""");

        AssertErro(await RegistrarAsync(api, Corpo(parada, linha, Agora)), (HttpStatusCode)422, "ParadaInativa");
    }

    [Fact]
    public async Task Registro_ComLinhaInativa_Responde422LinhaInativa()
    {
        await using ApiTeste api = await ApiAsync();
        var (parada, linha) = await CentroESimL004Async(api);
        await using (PontoVozDbContext contexto = api.NovoContexto())
        {
            (await contexto.Linhas.SingleAsync(l => l.Id == linha, ApiTeste.Ct)).Desativar();
            await contexto.SaveChangesAsync(ApiTeste.Ct);
        }

        AssertErro(await RegistrarAsync(api, Corpo(parada, linha, Agora)), (HttpStatusCode)422, "LinhaInativa");
    }

    [Fact]
    public async Task Registro_NaoRenovaAsPassagensSimuladasDoSeed()
    {
        await using ApiTeste api = await ApiAsync();
        var (parada, linha) = await CentroESimL004Async(api);
        List<(int, DateTimeOffset)> antes;
        await using (PontoVozDbContext contexto = api.NovoContexto())
        {
            antes = [.. (await contexto.RegistrosPassagem.OfType<PassagemSimulada>().ToListAsync(ApiTeste.Ct)).Select(p => (p.Id, p.ObservadaEmUtc))];
        }

        Assert.Equal(HttpStatusCode.Created, (await RegistrarAsync(api, Corpo(parada, linha, Agora))).Status);

        await using PontoVozDbContext depois = api.NovoContexto();
        List<(int, DateTimeOffset)> simuladas = [.. (await depois.RegistrosPassagem.OfType<PassagemSimulada>().ToListAsync(ApiTeste.Ct)).Select(p => (p.Id, p.ObservadaEmUtc))];
        Assert.Equal(antes, simuladas);
        Assert.Equal(1, await depois.RegistrosPassagem.OfType<PassagemRegistroManual>().CountAsync(ApiTeste.Ct));
    }
}

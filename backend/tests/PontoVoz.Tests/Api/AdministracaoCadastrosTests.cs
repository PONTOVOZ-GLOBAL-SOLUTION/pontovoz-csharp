using System.Globalization;
using System.Net;
using System.Text.Json.Nodes;

using Microsoft.EntityFrameworkCore;

using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Infrastructure.Persistence;
using PontoVoz.Api.Infrastructure.Persistence.Busca;
using PontoVoz.Tests.Application;

namespace PontoVoz.Tests.Api;

/// <summary>
/// Cadastro e atualização de paradas, cadastro de linhas e associação (seções 5.2 a 5.5), pela API
/// real e com o SQLite conferido diretamente. Cada teste tem o próprio banco temporário (Development
/// sem chave, como na demonstração local). Os ids vêm das respostas da API, nunca de valores fixos.
/// </summary>
public sealed class AdministracaoCadastrosTests
{
    private static string Texto(int valor) => valor.ToString(CultureInfo.InvariantCulture);

    private static JsonNode AssertErroValidacao(ApiTeste.Resposta resposta, params string[] campos)
    {
        Assert.Equal(HttpStatusCode.BadRequest, resposta.Status);
        Assert.Equal("application/problem+json", resposta.TipoMidia);
        JsonNode json = resposta.Json;
        Assert.Equal("RequisicaoInvalida", json["codigo"]!.GetValue<string>());
        Assert.Equal(campos.Order(StringComparer.Ordinal), json["errors"]!.AsObject().Select(par => par.Key).Order(StringComparer.Ordinal));
        return json;
    }

    private static void AssertErro(ApiTeste.Resposta resposta, HttpStatusCode status, string codigo)
    {
        Assert.Equal(status, resposta.Status);
        Assert.Equal("application/problem+json", resposta.TipoMidia);
        Assert.Equal(codigo, resposta.Json["codigo"]!.GetValue<string>());
        Assert.NotNull(resposta.Json["traceId"]);
    }

    private static Task<ApiTeste.Resposta> CadastrarParadaAsync(ApiTeste api, string json) =>
        api.EnviarAsync(HttpMethod.Post, "/api/v1/admin/paradas", json);

    private static Task<ApiTeste.Resposta> AtualizarParadaAsync(ApiTeste api, int id, string json) =>
        api.EnviarAsync(HttpMethod.Put, $"/api/v1/admin/paradas/{Texto(id)}", json);

    private static Task<ApiTeste.Resposta> CadastrarLinhaAsync(ApiTeste api, string json) =>
        api.EnviarAsync(HttpMethod.Post, "/api/v1/admin/linhas", json);

    private static Task<ApiTeste.Resposta> AssociarAsync(ApiTeste api, int paradaId, int linhaId) =>
        api.EnviarAsync(HttpMethod.Put, $"/api/v1/admin/paradas/{Texto(paradaId)}/linhas/{Texto(linhaId)}");

    private static async Task<int> CriarParadaFicticiaAsync(ApiTeste api, string codigo, string nome = "Parada Cadastro de Teste")
    {
        ApiTeste.Resposta resposta = await CadastrarParadaAsync(api, $$"""{"codigoExterno":"{{codigo}}","nome":"{{nome}}","dadosSimulados":true}""");
        Assert.Equal(HttpStatusCode.Created, resposta.Status);
        return resposta.Json["id"]!.GetValue<int>();
    }

    private static async Task<int> CriarLinhaFicticiaAsync(ApiTeste api, string codigo)
    {
        ApiTeste.Resposta resposta = await CadastrarLinhaAsync(api,
            $$"""{"codigo":"{{codigo}}","letreiro":"TESTE-1","terminalPrincipal":"Terminal A","terminalSecundario":"Terminal B","sentido":"PrincipalParaSecundario","dadosSimulados":true}""");
        Assert.Equal(HttpStatusCode.Created, resposta.Status);
        return resposta.Json["id"]!.GetValue<int>();
    }

    // ----- POST /api/v1/admin/paradas -----

    [Fact]
    public async Task CadastroDeParada_DoExemplo_Responde201IgualAoContratoComLocation()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync(inicioRelogio: new DateTimeOffset(2026, 11, 10, 14, 10, 0, TimeSpan.FromHours(-3)));
        string requisicao = ExemplosContrato.BlocoDoDocumento("### 5.2 `POST /api/v1/admin/paradas`").ToJsonString();

        ApiTeste.Resposta resposta = await CadastrarParadaAsync(api, requisicao);

        Assert.Equal(HttpStatusCode.Created, resposta.Status);
        Assert.Equal("application/json", resposta.TipoMidia);
        ExemplosContrato.AssertJsonIgualAoExemplo(ExemplosContrato.BlocoDoDocumento("Resposta **201**:"), resposta.Json);
        int id = resposta.Json["id"]!.GetValue<int>();
        Assert.Equal($"/api/v1/paradas/{Texto(id)}", resposta.Cabecalhos.Location!.OriginalString);

        // A parada criada já aparece nas consultas públicas, pelo Location e pela busca sem acentos.
        ApiTeste.Resposta detalhe = await api.GetAsync(resposta.Cabecalhos.Location.OriginalString);
        Assert.Equal(HttpStatusCode.OK, detalhe.Status);
        Assert.Equal("CadastroAdministrativo", detalhe.Json["origem"]!.GetValue<string>());
        Assert.True(detalhe.Json["dadosSimulados"]!.GetValue<bool>());
        ApiTeste.Resposta busca = await api.GetAsync("/api/v1/paradas?termo=cadastro de teste");
        Assert.Equal(id, Assert.Single(busca.Json["itens"]!.AsArray())!["id"]!.GetValue<int>());
    }

    [Fact]
    public async Task CadastroDeParada_NaoFicticia_GuardaEnderecoECoordenadaEDadosSimuladosFalse()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();

        ApiTeste.Resposta resposta = await CadastrarParadaAsync(api,
            """{"codigoExterno":"PT-0001","nome":"Parada Avenida Exemplo","endereco":"Av. Exemplo, 100","coordenada":{"latitude":-23.55,"longitude":-46.63},"dadosSimulados":false}""");

        Assert.Equal(HttpStatusCode.Created, resposta.Status);
        JsonNode json = resposta.Json;
        Assert.False(json["dadosSimulados"]!.GetValue<bool>());
        Assert.Equal("CadastroAdministrativo", json["origem"]!.GetValue<string>());
        Assert.True(json["ativa"]!.GetValue<bool>());
        Assert.Equal("Av. Exemplo, 100", json["endereco"]!.GetValue<string>());
        Assert.Equal(-23.55, json["coordenada"]!["latitude"]!.GetValue<double>());

        await using PontoVozDbContext contexto = api.NovoContexto();
        Parada gravada = await contexto.Paradas.SingleAsync(p => p.CodigoExterno == "PT-0001", ApiTeste.Ct);
        Assert.False(gravada.DadosSimulados);
        Assert.Equal(-46.63, gravada.Coordenada!.Longitude);
    }

    [Fact]
    public async Task CadastroDeParada_SemDadosSimulados_Responde400SemPresumirFalse()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();

        ApiTeste.Resposta resposta = await CadastrarParadaAsync(api, """{"codigoExterno":"TESTE-P020","nome":"Parada sem natureza"}""");

        JsonNode json = AssertErroValidacao(resposta, "dadosSimulados");
        Assert.Equal("O campo dadosSimulados é obrigatório.", json["errors"]!["dadosSimulados"]![0]!.GetValue<string>());
        await using PontoVozDbContext contexto = api.NovoContexto();
        Assert.False(await contexto.Paradas.AnyAsync(p => p.CodigoExterno == "TESTE-P020", ApiTeste.Ct));
    }

    [Theory]
    [InlineData("""{"codigoExterno":"SIM-P099","nome":"Parada Reservada","dadosSimulados":true}""", "codigoExterno")]
    [InlineData("""{"codigoExterno":"teste-p021","nome":"Parada Minúscula","dadosSimulados":true}""", "codigoExterno")]
    [InlineData("""{"codigoExterno":"TESTE-P022","nome":"ab","dadosSimulados":true}""", "nome")]
    [InlineData("""{"codigoExterno":"TESTE-P023","nome":"Parada Fictícia","endereco":"Rua Real, 1","dadosSimulados":true}""", "endereco")]
    [InlineData("""{"codigoExterno":"TESTE-P024","nome":"Parada Fictícia","coordenada":{"latitude":-23.5,"longitude":-46.6},"dadosSimulados":true}""", "endereco")]
    [InlineData("""{"codigoExterno":"TESTE-P025","nome":"Parada Real","coordenada":{"latitude":95,"longitude":-46.6},"dadosSimulados":false}""", "coordenada.latitude")]
    [InlineData("""{"codigoExterno":"TESTE-P026","nome":"Parada Real","coordenada":{"latitude":-23.5},"dadosSimulados":false}""", "coordenada.longitude")]
    [InlineData("""{"codigoExterno":"TESTE-P027","nome":"Parada Real","dadosSimulados":"sim"}""", "dadosSimulados")]
    public async Task CadastroDeParada_ComCampoInvalido_Responde400NoCampo(string json, string campo)
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();

        AssertErroValidacao(await CadastrarParadaAsync(api, json), campo);
    }

    [Fact]
    public async Task CadastroDeParada_ComVariosErros_DevolveTodosJuntos()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();

        AssertErroValidacao(await CadastrarParadaAsync(api, """{"codigoExterno":"SIM-X","nome":"ab","endereco":null}"""),
            "codigoExterno", "nome", "dadosSimulados");
    }

    [Theory]
    [InlineData(null, "application/json")]
    [InlineData("{ malformado", "application/json")]
    [InlineData("null", "application/json")]
    [InlineData("codigoExterno=TESTE-P028", "text/plain")]
    public async Task CadastroDeParada_SemCorpoJsonValido_Responde400EmCorpo(string? corpo, string tipo)
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();

        ApiTeste.Resposta resposta = await api.EnviarAsync(HttpMethod.Post, "/api/v1/admin/paradas", corpo, tipoConteudo: tipo);

        AssertErroValidacao(resposta, "corpo");
    }

    [Fact]
    public async Task CadastroDeParada_ComCodigoRepetido_Responde409MesmoComOutraInativa()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();
        int id = await CriarParadaFicticiaAsync(api, "TESTE-P030");
        Assert.Equal(HttpStatusCode.OK, (await AtualizarParadaAsync(api, id, """{"nome":"Parada Desativada","ativa":false}""")).Status);

        ApiTeste.Resposta repetida = await CadastrarParadaAsync(api, """{"codigoExterno":"TESTE-P030","nome":"Outra Parada","dadosSimulados":true}""");

        AssertErro(repetida, HttpStatusCode.Conflict, "CodigoJaCadastrado");
        Assert.Equal("Código já cadastrado", repetida.Json["title"]!.GetValue<string>());
        await using PontoVozDbContext contexto = api.NovoContexto();
        Assert.Equal(1, await contexto.Paradas.CountAsync(p => p.CodigoExterno == "TESTE-P030", ApiTeste.Ct));
    }

    // ----- PUT /api/v1/admin/paradas/{id} -----

    [Fact]
    public async Task AtualizacaoDeParada_TrocaSoOsDadosEditaveis()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();
        int id = await CriarParadaFicticiaAsync(api, "TESTE-P040");

        ApiTeste.Resposta resposta = await AtualizarParadaAsync(api, id, """{"nome":"Parada Renomeada","endereco":null,"coordenada":null,"ativa":true}""");

        Assert.Equal(HttpStatusCode.OK, resposta.Status);
        JsonNode json = resposta.Json;
        Assert.Equal("Parada Renomeada", json["nome"]!.GetValue<string>());
        Assert.Equal("TESTE-P040", json["codigoExterno"]!.GetValue<string>());
        Assert.Equal("CadastroAdministrativo", json["origem"]!.GetValue<string>());
        Assert.True(json["dadosSimulados"]!.GetValue<bool>());
    }

    [Fact]
    public async Task AtualizacaoDeParada_ComNomeNovo_AtualizaABuscaSemAcentos()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();
        int id = await CriarParadaFicticiaAsync(api, "TESTE-P041", "Parada Antiga");

        await AtualizarParadaAsync(api, id, """{"nome":"Estação Açaí da Vila","ativa":true}""");

        ApiTeste.Resposta nova = await api.GetAsync("/api/v1/paradas?termo=estacao acai");
        Assert.Equal(id, Assert.Single(nova.Json["itens"]!.AsArray())!["id"]!.GetValue<int>());
        Assert.Empty((await api.GetAsync("/api/v1/paradas?termo=parada antiga")).Json["itens"]!.AsArray());

        await using PontoVozDbContext contexto = api.NovoContexto();
        string textoBusca = await contexto.Paradas.Where(p => p.Id == id)
            .Select(p => EF.Property<string>(p, TextoBusca.Coluna)).SingleAsync(ApiTeste.Ct);
        Assert.Contains("estacao acai da vila", textoBusca, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AtualizacaoDeParada_DesativarEReativar_ControlaAsConsultasPublicas()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();
        int id = await CriarParadaFicticiaAsync(api, "TESTE-P042");

        ApiTeste.Resposta desativada = await AtualizarParadaAsync(api, id, ExemplosContrato.BlocoDoDocumento("### 5.3 `PUT /api/v1/admin/paradas/{id}`").ToJsonString());
        Assert.Equal(HttpStatusCode.OK, desativada.Status);
        Assert.False(desativada.Json["ativa"]!.GetValue<bool>());
        AssertErro(await api.GetAsync($"/api/v1/paradas/{Texto(id)}"), HttpStatusCode.NotFound, "ParadaNaoEncontrada");
        Assert.Equal(0, (await api.GetAsync("/api/v1/paradas?codigoExterno=TESTE-P042")).Json["totalItens"]!.GetValue<int>());

        // Uma parada inativa continua editável pela rota administrativa.
        ApiTeste.Resposta editadaInativa = await AtualizarParadaAsync(api, id, """{"nome":"Nome Durante Inativa","ativa":false}""");
        Assert.Equal(HttpStatusCode.OK, editadaInativa.Status);
        Assert.Equal("Nome Durante Inativa", editadaInativa.Json["nome"]!.GetValue<string>());

        ApiTeste.Resposta reativada = await AtualizarParadaAsync(api, id, """{"nome":"Nome Durante Inativa","ativa":true}""");
        Assert.True(reativada.Json["ativa"]!.GetValue<bool>());
        Assert.Equal(HttpStatusCode.OK, (await api.GetAsync($"/api/v1/paradas/{Texto(id)}")).Status);
    }

    [Fact]
    public async Task AtualizacaoDeParada_DesativarPreservaAssociacoesEPassagens()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();
        int id = await api.IdDaParadaAsync("SIM-P001");
        int associacoesAntes, passagensAntes;
        await using (PontoVozDbContext antes = api.NovoContexto())
        {
            associacoesAntes = await antes.ParadasLinhas.CountAsync(a => a.ParadaId == id, ApiTeste.Ct);
            passagensAntes = await antes.RegistrosPassagem.CountAsync(p => p.ParadaId == id, ApiTeste.Ct);
        }

        Assert.Equal(HttpStatusCode.OK, (await AtualizarParadaAsync(api, id, """{"nome":"Parada Demonstração Centro","ativa":false}""")).Status);

        await using PontoVozDbContext depois = api.NovoContexto();
        Assert.True(associacoesAntes > 0 && passagensAntes > 0);
        Assert.Equal(associacoesAntes, await depois.ParadasLinhas.CountAsync(a => a.ParadaId == id, ApiTeste.Ct));
        Assert.Equal(passagensAntes, await depois.RegistrosPassagem.CountAsync(p => p.ParadaId == id, ApiTeste.Ct));
        Parada parada = await depois.Paradas.SingleAsync(p => p.Id == id, ApiTeste.Ct);
        Assert.False(parada.Ativa);
        Assert.Equal("SIM-P001", parada.CodigoExterno);
        Assert.True(parada.DadosSimulados);
    }

    [Theory]
    [InlineData("""{"nome":"Parada Sem Ativa"}""", "ativa")]
    [InlineData("""{"ativa":true}""", "nome")]
    [InlineData("""{"nome":"Parada Fictícia","endereco":"Rua Real, 1","ativa":true}""", "endereco")]
    [InlineData("""{"nome":"Parada","ativa":"talvez"}""", "ativa")]
    public async Task AtualizacaoDeParada_ComCorpoInvalido_Responde400(string json, string campo)
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();
        int id = await CriarParadaFicticiaAsync(api, "TESTE-P043");

        AssertErroValidacao(await AtualizarParadaAsync(api, id, json), campo);
    }

    [Fact]
    public async Task AtualizacaoDeParada_Inexistente_Responde404()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();
        int inexistente = await CriarParadaFicticiaAsync(api, "TESTE-P044") + 1000;

        ApiTeste.Resposta resposta = await AtualizarParadaAsync(api, inexistente, """{"nome":"Qualquer Nome","ativa":true}""");

        AssertErro(resposta, HttpStatusCode.NotFound, "ParadaNaoEncontrada");
        Assert.Equal($"Não existe parada com o id {Texto(inexistente)}.", resposta.Json["detail"]!.GetValue<string>());
    }

    // ----- POST /api/v1/admin/linhas -----

    [Fact]
    public async Task CadastroDeLinha_DoExemplo_Responde201SemLocationEApareceNaConsultaPublica()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();
        JsonNode requisicao = ExemplosContrato.BlocoDoDocumento("### 5.4 `POST /api/v1/admin/linhas`");

        ApiTeste.Resposta resposta = await CadastrarLinhaAsync(api, requisicao.ToJsonString());

        Assert.Equal(HttpStatusCode.Created, resposta.Status);
        Assert.Null(resposta.Cabecalhos.Location);
        JsonNode json = resposta.Json;
        foreach (string campo in new[] { "codigo", "letreiro", "terminalPrincipal", "terminalSecundario", "sentido" })
        {
            Assert.Equal(requisicao[campo]!.GetValue<string>(), json[campo]!.GetValue<string>());
        }

        Assert.Equal("CadastroAdministrativo", json["origem"]!.GetValue<string>());
        Assert.True(json["dadosSimulados"]!.GetValue<bool>());
        Assert.True(json["ativa"]!.GetValue<bool>());
        Assert.Equal(TimeSpan.FromHours(-3), DateTimeOffset.Parse(json["criadoEm"]!.GetValue<string>(), CultureInfo.InvariantCulture).Offset);

        ApiTeste.Resposta publica = await api.GetAsync("/api/v1/linhas?termo=teste-l005");
        JsonNode linha = Assert.Single(publica.Json["itens"]!.AsArray())!;
        Assert.Equal(json["id"]!.GetValue<int>(), linha["id"]!.GetValue<int>());
        Assert.Equal("CadastroAdministrativo", linha["origem"]!.GetValue<string>());
    }

    [Fact]
    public async Task CadastroDeLinha_CadaSentidoEUmCadastroProprio()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();

        ApiTeste.Resposta ida = await CadastrarLinhaAsync(api,
            """{"codigo":"TESTE-L050-1","letreiro":"TESTE-150","terminalPrincipal":"Terminal A","terminalSecundario":"Terminal B","sentido":"PrincipalParaSecundario","dadosSimulados":true}""");
        ApiTeste.Resposta volta = await CadastrarLinhaAsync(api,
            """{"codigo":"TESTE-L050-2","letreiro":"TESTE-150","terminalPrincipal":"Terminal A","terminalSecundario":"Terminal B","sentido":"SecundarioParaPrincipal","dadosSimulados":true}""");

        Assert.Equal(HttpStatusCode.Created, ida.Status);
        Assert.Equal(HttpStatusCode.Created, volta.Status);
        Assert.NotEqual(ida.Json["id"]!.GetValue<int>(), volta.Json["id"]!.GetValue<int>());
    }

    [Theory]
    [InlineData("""{"codigo":"TESTE-L051","letreiro":"T-1","terminalPrincipal":"Terminal A","terminalSecundario":"Terminal B","sentido":"PrincipalParaSecundario"}""", "dadosSimulados")]
    [InlineData("""{"codigo":"TESTE-L052","letreiro":"T-1","terminalPrincipal":"Terminal A","terminalSecundario":"Terminal B","dadosSimulados":true}""", "sentido")]
    [InlineData("""{"codigo":"TESTE-L053","letreiro":"T-1","terminalPrincipal":"Terminal A","terminalSecundario":"Terminal B","sentido":"Diagonal","dadosSimulados":true}""", "sentido")]
    [InlineData("""{"codigo":"TESTE-L054","letreiro":"T-1","terminalPrincipal":"Terminal A","terminalSecundario":"Terminal B","sentido":99,"dadosSimulados":true}""", "sentido")]
    [InlineData("""{"codigo":"SIM-L055","letreiro":"T-1","terminalPrincipal":"Terminal A","terminalSecundario":"Terminal B","sentido":"PrincipalParaSecundario","dadosSimulados":true}""", "codigo")]
    [InlineData("""{"codigo":"TESTE-L056","letreiro":"SIM-156","terminalPrincipal":"Terminal A","terminalSecundario":"Terminal B","sentido":"PrincipalParaSecundario","dadosSimulados":true}""", "letreiro")]
    [InlineData("""{"codigo":"TESTE-L057","letreiro":"","terminalPrincipal":"Terminal A","terminalSecundario":"Terminal B","sentido":"PrincipalParaSecundario","dadosSimulados":true}""", "letreiro")]
    [InlineData("""{"codigo":"TESTE-L058","letreiro":"T-1","terminalPrincipal":"AB","terminalSecundario":"Terminal B","sentido":"PrincipalParaSecundario","dadosSimulados":true}""", "terminalPrincipal")]
    public async Task CadastroDeLinha_ComCampoInvalido_Responde400NoCampo(string json, string campo)
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();

        AssertErroValidacao(await CadastrarLinhaAsync(api, json), campo);
    }

    [Fact]
    public async Task CadastroDeLinha_ComCodigoRepetido_Responde409()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();
        await CriarLinhaFicticiaAsync(api, "TESTE-L060");

        ApiTeste.Resposta repetida = await CadastrarLinhaAsync(api,
            """{"codigo":"TESTE-L060","letreiro":"OUTRO","terminalPrincipal":"Terminal C","terminalSecundario":"Terminal D","sentido":"SecundarioParaPrincipal","dadosSimulados":false}""");

        AssertErro(repetida, HttpStatusCode.Conflict, "CodigoJaCadastrado");
        await using PontoVozDbContext contexto = api.NovoContexto();
        Assert.Equal(1, await contexto.Linhas.CountAsync(l => l.Codigo == "TESTE-L060", ApiTeste.Ct));
    }

    // ----- PUT /api/v1/admin/paradas/{id}/linhas/{linhaId} -----

    [Fact]
    public async Task Associacao_Responde204EApareceNoDetalhePublico()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();
        int paradaId = await CriarParadaFicticiaAsync(api, "TESTE-P070");
        int linhaId = await CriarLinhaFicticiaAsync(api, "TESTE-L070");

        ApiTeste.Resposta resposta = await AssociarAsync(api, paradaId, linhaId);

        Assert.Equal(HttpStatusCode.NoContent, resposta.Status);
        Assert.Empty(resposta.Corpo);
        JsonNode linhas = (await api.GetAsync($"/api/v1/paradas/{Texto(paradaId)}")).Json["linhas"]!;
        Assert.Equal(linhaId, Assert.Single(linhas.AsArray())!["id"]!.GetValue<int>());
    }

    [Fact]
    public async Task Associacao_Repetida_Responde204SemDuplicarNoSqlite()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();
        int paradaId = await api.IdDaParadaAsync("SIM-P001");
        int linhaId = await api.IdDaLinhaAsync("SIM-L004");

        // Cada chamada é uma requisição nova, com outro DbContext: a idempotência não depende de objetos em memória.
        for (int i = 0; i < 3; i++)
        {
            Assert.Equal(HttpStatusCode.NoContent, (await AssociarAsync(api, paradaId, linhaId)).Status);
        }

        int nova = await CriarLinhaFicticiaAsync(api, "TESTE-L071");
        Assert.Equal(HttpStatusCode.NoContent, (await AssociarAsync(api, paradaId, nova)).Status);
        Assert.Equal(HttpStatusCode.NoContent, (await AssociarAsync(api, paradaId, nova)).Status);

        await using PontoVozDbContext contexto = api.NovoContexto();
        Assert.Equal(1, await contexto.ParadasLinhas.CountAsync(a => a.ParadaId == paradaId && a.LinhaId == linhaId, ApiTeste.Ct));
        Assert.Equal(1, await contexto.ParadasLinhas.CountAsync(a => a.ParadaId == paradaId && a.LinhaId == nova, ApiTeste.Ct));
        Assert.Equal(4, await contexto.ParadasLinhas.CountAsync(a => a.ParadaId == paradaId, ApiTeste.Ct));
    }

    [Fact]
    public async Task Associacao_ComParadaOuLinhaInexistente_Responde404()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();
        int paradaId = await api.IdDaParadaAsync("SIM-P001");
        int linhaId = await api.IdDaLinhaAsync("SIM-L004");

        AssertErro(await AssociarAsync(api, paradaId + 1000, linhaId), HttpStatusCode.NotFound, "ParadaNaoEncontrada");
        AssertErro(await AssociarAsync(api, paradaId, linhaId + 1000), HttpStatusCode.NotFound, "LinhaNaoEncontrada");
        AssertErro(await AssociarAsync(api, paradaId + 1000, linhaId + 1000), HttpStatusCode.NotFound, "ParadaNaoEncontrada");
    }

    [Fact]
    public async Task Associacao_ComParadaInativa_Responde422ParadaInativa()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();
        int paradaId = await CriarParadaFicticiaAsync(api, "TESTE-P072");
        int linhaId = await CriarLinhaFicticiaAsync(api, "TESTE-L072");
        await AtualizarParadaAsync(api, paradaId, """{"nome":"Parada Inativa","ativa":false}""");

        ApiTeste.Resposta resposta = await AssociarAsync(api, paradaId, linhaId);

        AssertErro(resposta, (HttpStatusCode)422, "ParadaInativa");
        Assert.Equal("Parada inativa", resposta.Json["title"]!.GetValue<string>());
    }

    [Fact]
    public async Task Associacao_ComLinhaInativa_Responde422LinhaInativa()
    {
        await using ApiTeste api = await ApiTeste.CriarAsync();
        int paradaId = await CriarParadaFicticiaAsync(api, "TESTE-P073");
        int linhaId = await CriarLinhaFicticiaAsync(api, "TESTE-L073");

        // O MVP não tem rota para desativar linha; a desativação é feita pelo próprio domínio, direto no banco do teste.
        await using (PontoVozDbContext contexto = api.NovoContexto())
        {
            Linha linha = await contexto.Linhas.SingleAsync(l => l.Id == linhaId, ApiTeste.Ct);
            linha.Desativar();
            await contexto.SaveChangesAsync(ApiTeste.Ct);
        }

        ApiTeste.Resposta resposta = await AssociarAsync(api, paradaId, linhaId);

        AssertErro(resposta, (HttpStatusCode)422, "LinhaInativa");
        Assert.Equal("Linha inativa", resposta.Json["title"]!.GetValue<string>());
    }
}

using System.Globalization;
using System.Net;
using System.Text.Json.Nodes;

using Microsoft.EntityFrameworkCore;

using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Infrastructure.Persistence;

namespace PontoVoz.Tests.Api;

/// <summary>
/// O fluxo do operador de ponta a ponta, só por HTTP e com o SQLite real: com a chave exigida,
/// cadastra parada e linha fictícias, associa, registra uma observação, confere a consulta
/// pública, desativa a parada e confirma que o histórico continua gravado.
/// </summary>
public sealed class AdministracaoFluxoCompletoTests
{
    /// <summary>Valor fictício, só de teste.</summary>
    private const string ChaveDeTeste = "chave-de-teste-fluxo-nao-real";

    [Fact]
    public async Task FluxoDoOperador_DoCadastroAConsultaPublicaEADesativacao()
    {
        // 1. Autorização conforme o contrato: fora de Development, com a chave configurada.
        await using ApiTeste api = await ApiTeste.CriarAsync(chaveAdministrativa: ChaveDeTeste, ambiente: "Production", registrarManualDoExemplo: false);
        var chave = new Dictionary<string, string> { ["X-PontoVoz-Admin-Key"] = ChaveDeTeste };
        Assert.Equal(HttpStatusCode.Unauthorized, (await api.EnviarAsync(HttpMethod.Post, "/api/v1/admin/paradas", "{}")).Status);

        // 2. Parada fictícia.
        ApiTeste.Resposta parada = await api.EnviarAsync(HttpMethod.Post, "/api/v1/admin/paradas",
            """{"codigoExterno":"TESTE-P900","nome":"Parada Fluxo Completo","endereco":null,"coordenada":null,"dadosSimulados":true}""", chave);
        Assert.Equal(HttpStatusCode.Created, parada.Status);
        int paradaId = parada.Json["id"]!.GetValue<int>();

        // 3. Linha fictícia.
        ApiTeste.Resposta linha = await api.EnviarAsync(HttpMethod.Post, "/api/v1/admin/linhas",
            """{"codigo":"TESTE-L900","letreiro":"TESTE-900","terminalPrincipal":"Terminal Fluxo A","terminalSecundario":"Terminal Fluxo B","sentido":"PrincipalParaSecundario","dadosSimulados":true}""", chave);
        Assert.Equal(HttpStatusCode.Created, linha.Status);
        int linhaId = linha.Json["id"]!.GetValue<int>();

        // 4. Associação.
        Assert.Equal(HttpStatusCode.NoContent,
            (await api.EnviarAsync(HttpMethod.Put, $"/api/v1/admin/paradas/{paradaId}/linhas/{linhaId}", cabecalhos: chave)).Status);

        // 5. Observação manual válida, cinco minutos antes do instante do servidor.
        DateTimeOffset observada = api.Relogio.GetUtcNow().AddMinutes(-5);
        string corpo = new JsonObject
        {
            ["paradaId"] = paradaId,
            ["linhaId"] = linhaId,
            ["observadaEm"] = observada.ToString("yyyy-MM-dd'T'HH:mm:ssK", CultureInfo.InvariantCulture),
            ["prefixoVeiculo"] = "SIM-V9001",
            ["origemRegistro"] = "operador-fluxo",
        }.ToJsonString();
        ApiTeste.Resposta registro = await api.EnviarAsync(HttpMethod.Post, "/api/v1/admin/observacoes-passagem", corpo, chave);
        Assert.Equal(HttpStatusCode.Created, registro.Status);
        int registroId = registro.Json["id"]!.GetValue<int>();

        // 6 a 8. A consulta pública, sem chave, mostra o registro com os dados certos e nunca como verificado.
        ApiTeste.Resposta ultimas = await api.GetAsync($"/api/v1/paradas/{paradaId}/ultimas-passagens");
        Assert.Equal(HttpStatusCode.OK, ultimas.Status);
        JsonNode passagem = Assert.Single(ultimas.Json["passagens"]!.AsArray())!;
        Assert.Equal(registroId, passagem["id"]!.GetValue<int>());
        Assert.Equal("TESTE-L900", passagem["codigoLinha"]!.GetValue<string>());
        Assert.Equal("TESTE-900", passagem["letreiro"]!.GetValue<string>());
        Assert.Equal(5, passagem["minutosDesdeObservacao"]!.GetValue<int>());
        Assert.False(passagem["desatualizada"]!.GetValue<bool>());
        Assert.Equal("SIM-V9001", passagem["prefixoVeiculo"]!.GetValue<string>());
        Assert.Equal("operador-fluxo", passagem["origemRegistro"]!.GetValue<string>());
        Assert.Equal("RegistroManual", passagem["nivelConfirmacao"]!.GetValue<string>());
        Assert.False(passagem["verificada"]!.GetValue<bool>());
        Assert.False(passagem["oficialSptrans"]!.GetValue<bool>());
        Assert.True(passagem["dadosSimulados"]!.GetValue<bool>());
        Assert.Empty(ultimas.Json["linhasSemRegistro"]!.AsArray());
        Assert.Equal(
            ["DadosSimulados", "CadastroNaoVerificado"],
            ultimas.Json["avisos"]!.AsArray().Select(a => a!["codigo"]!.GetValue<string>()));

        // 9 e 10. Desativada, a parada some das consultas públicas.
        ApiTeste.Resposta desativada = await api.EnviarAsync(HttpMethod.Put, $"/api/v1/admin/paradas/{paradaId}",
            """{"nome":"Parada Fluxo Completo","endereco":null,"coordenada":null,"ativa":false}""", chave);
        Assert.Equal(HttpStatusCode.OK, desativada.Status);
        Assert.Equal(0, (await api.GetAsync("/api/v1/paradas?codigoExterno=TESTE-P900")).Json["totalItens"]!.GetValue<int>());
        Assert.Equal(HttpStatusCode.NotFound, (await api.GetAsync($"/api/v1/paradas/{paradaId}")).Status);
        Assert.Equal(HttpStatusCode.NotFound, (await api.GetAsync($"/api/v1/paradas/{paradaId}/ultimas-passagens")).Status);

        // 11. O histórico continua no SQLite: parada inativa, associação e registro.
        await using PontoVozDbContext contexto = api.NovoContexto();
        Parada gravada = await contexto.Paradas.SingleAsync(p => p.Id == paradaId, ApiTeste.Ct);
        Assert.False(gravada.Ativa);
        Assert.True(await contexto.ParadasLinhas.AnyAsync(a => a.ParadaId == paradaId && a.LinhaId == linhaId, ApiTeste.Ct));
        PassagemRegistroManual manual = await contexto.RegistrosPassagem.OfType<PassagemRegistroManual>().SingleAsync(p => p.Id == registroId, ApiTeste.Ct);
        Assert.Equal(observada, manual.ObservadaEmUtc);
    }
}

using Microsoft.EntityFrameworkCore;

using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Infrastructure.Persistence;
using PontoVoz.Api.Infrastructure.Repositories;
using PontoVoz.Tests.Domain;

namespace PontoVoz.Tests.Infrastructure;

/// <summary>Consulta da última passagem por linha (contrato, seção 4.6.1) no SQLite real.</summary>
public class UltimasPassagensTests
{
    private static CancellationToken Ct => BancoSqliteTeste.Ct;

    private static readonly DateTimeOffset Referencia = new(2026, 11, 10, 17, 32, 0, TimeSpan.Zero);

    /// <summary>Parada com três linhas associadas (L001, L002 e L004) e uma quarta linha (L003) que não a atende.</summary>
    private sealed record Cadastro(int ParadaId, int L001, int L002, int L004, int L003);

    private static async Task<Cadastro> CadastrarAsync(BancoSqliteTeste banco)
    {
        await using PontoVozDbContext contexto = banco.NovoContexto();
        Parada parada = Cenario.ParadaSimulada();
        Linha l001 = Cenario.LinhaSimulada("SIM-L001", "SIM-101");
        Linha l002 = Cenario.LinhaSimulada("SIM-L002", "SIM-102");
        Linha l004 = Cenario.LinhaSimulada("SIM-L004", "SIM-104");
        Linha l003 = Cenario.LinhaSimulada("SIM-L003", "SIM-203");
        foreach (Linha linha in new[] { l001, l002, l004 })
        {
            parada.AssociarLinha(linha, Cenario.Agora);
        }

        contexto.Paradas.Add(parada);
        contexto.Linhas.Add(l003);
        await contexto.SaveChangesAsync(Ct);
        return new Cadastro(parada.Id, l001.Id, l002.Id, l004.Id, l003.Id);
    }

    /// <summary>Registra uma passagem manual observada em <paramref name="observadaEm"/>, registrada no mesmo instante (ou em <paramref name="registradaEm"/>).</summary>
    private static async Task<int> RegistrarManualAsync(BancoSqliteTeste banco, int paradaId, int linhaId, DateTimeOffset observadaEm, DateTimeOffset? registradaEm = null, string origem = "operador-local")
    {
        await using PontoVozDbContext contexto = banco.NovoContexto();
        Parada parada = await contexto.Paradas.Include(p => p.LinhasAssociadas).ThenInclude(a => a.Linha).SingleAsync(p => p.Id == paradaId, Ct);
        Linha linha = parada.LinhasAssociadas.Single(a => a.LinhaId == linhaId).Linha;
        var passagem = PassagemRegistroManual.Registrar(parada, linha, observadaEm, null, origem, registradaEm ?? observadaEm);
        contexto.RegistrosPassagem.Add(passagem);
        await contexto.SaveChangesAsync(Ct);
        return passagem.Id;
    }

    private static async Task<IReadOnlyList<RegistroPassagem>> UltimasAsync(BancoSqliteTeste banco, int paradaId, int? linhaId = null, DateTimeOffset? referencia = null)
    {
        await using PontoVozDbContext contexto = banco.NovoContexto();
        return await new RegistroPassagemRepository(contexto).ListarUltimasPorLinhaAsync(paradaId, referencia ?? Referencia, linhaId, Ct);
    }

    [Fact]
    public async Task UmaPassagemPorLinha_AMaisRecente_OrdenadasDaMaisRecenteParaAMaisAntiga()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        Cadastro c = await CadastrarAsync(banco);
        await RegistrarManualAsync(banco, c.ParadaId, c.L002, Referencia.AddMinutes(-90));
        int recenteL002 = await RegistrarManualAsync(banco, c.ParadaId, c.L002, Referencia.AddMinutes(-12));
        await RegistrarManualAsync(banco, c.ParadaId, c.L002, Referencia.AddMinutes(-40));
        int unicaL004 = await RegistrarManualAsync(banco, c.ParadaId, c.L004, Referencia.AddMinutes(-87));

        IReadOnlyList<RegistroPassagem> ultimas = await UltimasAsync(banco, c.ParadaId);

        Assert.Equal(new[] { recenteL002, unicaL004 }, ultimas.Select(p => p.Id));
        Assert.Equal(new[] { "SIM-L002", "SIM-L004" }, ultimas.Select(p => p.Linha.Codigo));
        Assert.Equal(new[] { 12, 87 }, ultimas.Select(p => p.MinutosDesdeObservacao(Referencia)));
    }

    [Fact]
    public async Task SemRegistro_LinhaNaoAparece_EParadaSemPassagensDevolveVazio()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        Cadastro c = await CadastrarAsync(banco);

        Assert.Empty(await UltimasAsync(banco, c.ParadaId));

        await RegistrarManualAsync(banco, c.ParadaId, c.L004, Referencia.AddMinutes(-5));
        Assert.DoesNotContain(await UltimasAsync(banco, c.ParadaId), p => p.LinhaId == c.L001);
    }

    [Fact]
    public async Task MesmoHorarioObservado_DesempataPeloRegistroMaisRecente()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        Cadastro c = await CadastrarAsync(banco);
        DateTimeOffset observadaEm = Referencia.AddMinutes(-20);
        int registradaDepois = await RegistrarManualAsync(banco, c.ParadaId, c.L002, observadaEm, observadaEm.AddMinutes(3));
        await RegistrarManualAsync(banco, c.ParadaId, c.L002, observadaEm, observadaEm.AddMinutes(1));

        RegistroPassagem ultima = Assert.Single(await UltimasAsync(banco, c.ParadaId));

        Assert.Equal(registradaDepois, ultima.Id);
    }

    [Fact]
    public async Task MesmoHorarioObservadoERegistrado_DesempataPeloMaiorId()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        Cadastro c = await CadastrarAsync(banco);
        DateTimeOffset instante = Referencia.AddMinutes(-20);
        int primeiro = await RegistrarManualAsync(banco, c.ParadaId, c.L002, instante);
        int segundo = await RegistrarManualAsync(banco, c.ParadaId, c.L002, instante);

        RegistroPassagem ultima = Assert.Single(await UltimasAsync(banco, c.ParadaId));

        Assert.True(segundo > primeiro);
        Assert.Equal(segundo, ultima.Id);
    }

    [Fact]
    public async Task RegistroAindaNoFuturo_NaoContaAteOHorarioChegar()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        Cadastro c = await CadastrarAsync(banco);
        int anterior = await RegistrarManualAsync(banco, c.ParadaId, c.L002, Referencia.AddMinutes(-30));
        // Aceito pela tolerância de 2 minutos: observado 90 s depois do instante do registro.
        int futuro = await RegistrarManualAsync(banco, c.ParadaId, c.L002, Referencia.AddSeconds(90), Referencia);

        Assert.Equal(anterior, Assert.Single(await UltimasAsync(banco, c.ParadaId)).Id);
        Assert.Equal(anterior, Assert.Single(await UltimasAsync(banco, c.ParadaId, referencia: Referencia.AddSeconds(89))).Id);
        Assert.Equal(futuro, Assert.Single(await UltimasAsync(banco, c.ParadaId, referencia: Referencia.AddSeconds(90))).Id);
    }

    [Fact]
    public async Task FiltroPorLinha_DevolveSoAQueLinha()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        Cadastro c = await CadastrarAsync(banco);
        await RegistrarManualAsync(banco, c.ParadaId, c.L002, Referencia.AddMinutes(-12));
        int l004 = await RegistrarManualAsync(banco, c.ParadaId, c.L004, Referencia.AddMinutes(-3));

        Assert.Equal(l004, Assert.Single(await UltimasAsync(banco, c.ParadaId, c.L004)).Id);
        Assert.Empty(await UltimasAsync(banco, c.ParadaId, c.L001));
        Assert.Empty(await UltimasAsync(banco, c.ParadaId, c.L003));
    }

    [Fact]
    public async Task LinhaInativa_SaiDaConsulta_SemApagarOHistorico()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        Cadastro c = await CadastrarAsync(banco);
        await RegistrarManualAsync(banco, c.ParadaId, c.L002, Referencia.AddMinutes(-12));
        int l004 = await RegistrarManualAsync(banco, c.ParadaId, c.L004, Referencia.AddMinutes(-3));
        await using (PontoVozDbContext contexto = banco.NovoContexto())
        {
            (await contexto.Linhas.SingleAsync(l => l.Id == c.L002, Ct)).Desativar();
            await contexto.SaveChangesAsync(Ct);
        }

        Assert.Equal(l004, Assert.Single(await UltimasAsync(banco, c.ParadaId)).Id);
        await using PontoVozDbContext leitura = banco.NovoContexto();
        Assert.Equal(2, await leitura.RegistrosPassagem.CountAsync(Ct));
    }

    [Fact]
    public async Task TiposConcretos_ENaturezaDoDado_SaoPreservados_SemVerificacaoOficial()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync(provisionarDemonstracao: true);
        int centro, avenida;
        await using (PontoVozDbContext contexto = banco.NovoContexto())
        {
            centro = await contexto.Paradas.Where(p => p.CodigoExterno == "SIM-P001").Select(p => p.Id).SingleAsync(Ct);
            avenida = await contexto.Paradas.Where(p => p.CodigoExterno == "SIM-P002").Select(p => p.Id).SingleAsync(Ct);
        }

        int l004 = await UltimaLinhaAsync(banco, "SIM-L004");
        DateTimeOffset agora = banco.Relogio.GetUtcNow();
        await RegistrarManualAsync(banco, centro, l004, agora.AddMinutes(-8), agora);

        // Parada e linha reais (não fictícias), com passagem manual.
        int real;
        await using (PontoVozDbContext contexto = banco.NovoContexto())
        {
            Parada parada = Parada.CadastrarAdministrativa("PARADA-0001", "Parada Real", null, null, false, agora);
            Linha linha = Linha.CadastrarAdministrativa("LINHA-0001", "8000-10", "Centro", "Sul", SentidoLinha.PrincipalParaSecundario, false, agora);
            parada.AssociarLinha(linha, agora);
            contexto.Paradas.Add(parada);
            contexto.RegistrosPassagem.Add(PassagemRegistroManual.Registrar(parada, linha, agora.AddMinutes(-1), "V-12", "operador-local", agora));
            await contexto.SaveChangesAsync(Ct);
            real = parada.Id;
        }

        IReadOnlyList<RegistroPassagem> doCentro = await UltimasAsync(banco, centro, referencia: agora);
        IReadOnlyList<RegistroPassagem> daAvenida = await UltimasAsync(banco, avenida, referencia: agora);
        RegistroPassagem daParadaReal = Assert.Single(await UltimasAsync(banco, real, referencia: agora));

        var manualFicticia = Assert.IsType<PassagemRegistroManual>(doCentro[0]);
        var simulada = Assert.IsType<PassagemSimulada>(doCentro[1]);
        Assert.Equal((NivelConfirmacao.RegistroManual, true), (manualFicticia.NivelConfirmacao, manualFicticia.DadosSimulados));
        Assert.Equal((NivelConfirmacao.ObservacaoSimulada, true, "SIM-OBS-001"), (simulada.NivelConfirmacao, simulada.DadosSimulados, simulada.ChaveSeed));
        Assert.Equal("SIM-OBS-002", Assert.IsType<PassagemSimulada>(Assert.Single(daAvenida)).ChaveSeed);
        Assert.False(Assert.IsType<PassagemRegistroManual>(daParadaReal).DadosSimulados);
        Assert.All(doCentro.Concat(daAvenida).Append(daParadaReal), p =>
        {
            Assert.False(p.Verificada);
            Assert.False(p.OficialSptrans);
        });
    }

    [Fact]
    public async Task SelecaoDaUltima_AconteceNoSql_EmUmUnicoComando()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        Cadastro c = await CadastrarAsync(banco);
        for (int minutos = 5; minutos <= 50; minutos += 5)
        {
            await RegistrarManualAsync(banco, c.ParadaId, c.L002, Referencia.AddMinutes(-minutos));
        }

        banco.LimparComandosSql();
        IReadOnlyList<RegistroPassagem> ultimas = await UltimasAsync(banco, c.ParadaId);

        string sql = Assert.Single(banco.ComandosSql);
        // Um único comando: agrupa por linha, escolhe a última com desempate e já traz a linha.
        Assert.Contains("GROUP BY \"r0\".\"LinhaId\"", sql);
        Assert.Contains("ORDER BY \"r1\".\"ObservadaEmUtc\" DESC, \"r1\".\"RegistradaEmUtc\" DESC, \"r1\".\"Id\" DESC", sql);
        Assert.Contains("LIMIT 1", sql);
        Assert.Contains("\"ObservadaEmUtc\" <= @referencia", sql);
        Assert.Contains("INNER JOIN \"Linhas\"", sql);
        Assert.Equal(-5, (int)(Assert.Single(ultimas).ObservadaEmUtc - Referencia).TotalMinutes);
    }

    private static async Task<int> UltimaLinhaAsync(BancoSqliteTeste banco, string codigo)
    {
        await using PontoVozDbContext contexto = banco.NovoContexto();
        return await contexto.Linhas.Where(l => l.Codigo == codigo).Select(l => l.Id).SingleAsync(Ct);
    }
}

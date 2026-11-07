using Microsoft.EntityFrameworkCore;

using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Infrastructure.Persistence;
using PontoVoz.Tests.Domain;

namespace PontoVoz.Tests.Infrastructure;

public class PersistenciaPassagensTests
{
    private static CancellationToken Ct => BancoSqliteTeste.Ct;

    private static readonly DateTimeOffset Referencia = new(2026, 11, 10, 17, 32, 0, TimeSpan.Zero);

    /// <summary>Grava parada, linha e as passagens manuais observadas nos instantes informados.</summary>
    private static async Task SalvarPassagensManuaisAsync(BancoSqliteTeste banco, params DateTimeOffset[] observacoes)
    {
        await using PontoVozDbContext contexto = banco.NovoContexto();
        (Parada parada, Linha linha, _) = Cenario.ParadaComLinha();
        contexto.Paradas.Add(parada);
        foreach (DateTimeOffset observadaEm in observacoes)
        {
            contexto.RegistrosPassagem.Add(PassagemRegistroManual.Registrar(parada, linha, observadaEm, null, "operador-local", observadaEm));
        }

        await contexto.SaveChangesAsync(Ct);
    }

    [Fact]
    public async Task Hierarquia_ESalvaNaMesmaTabela_EMaterializaASubclasseCorreta()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        await using (PontoVozDbContext contexto = banco.NovoContexto())
        {
            (Parada parada, Linha linha, ParadaLinha associacao) = Cenario.ParadaComLinha();
            contexto.Paradas.Add(parada);
            contexto.RegistrosPassagem.Add(PassagemSimulada.CriarNoProvisionamento(associacao, "SIM-OBS-001", TimeSpan.FromMinutes(15), Cenario.Agora, "SIM-V2003"));
            contexto.RegistrosPassagem.Add(PassagemRegistroManual.Registrar(parada, linha, Cenario.Agora.AddMinutes(-8), "SIM-V4001", "operador-local", Cenario.Agora));
            await contexto.SaveChangesAsync(Ct);
        }

        await using PontoVozDbContext leitura = banco.NovoContexto();
        List<RegistroPassagem> registros = await leitura.RegistrosPassagem.OrderBy(p => p.ObservadaEmUtc).ToListAsync(Ct);
        List<string> tipos = await leitura.Database.SqlQuery<string>($"SELECT TipoPassagem AS Value FROM RegistrosPassagem ORDER BY ObservadaEmUtc").ToListAsync(Ct);

        Assert.Equal(new[] { "Simulada", "RegistroManual" }, tipos);
        Assert.Collection(registros,
            registro =>
            {
                var simulada = Assert.IsType<PassagemSimulada>(registro);
                Assert.Equal("SIM-OBS-001", simulada.ChaveSeed);
                Assert.Equal(NivelConfirmacao.ObservacaoSimulada, simulada.NivelConfirmacao);
                Assert.Equal("ObservacaoSimulada", simulada.CodigoAviso);
                Assert.Null(simulada.OrigemRegistro);
                Assert.True(simulada.DadosSimulados);
                Assert.Equal("SIM-V2003", simulada.PrefixoVeiculo);
            },
            registro =>
            {
                var manual = Assert.IsType<PassagemRegistroManual>(registro);
                Assert.Equal("operador-local", manual.OrigemRegistro);
                Assert.Equal(NivelConfirmacao.RegistroManual, manual.NivelConfirmacao);
                Assert.Equal("RegistroManualNaoVerificado", manual.CodigoAviso);
                Assert.True(manual.DadosSimulados);
                Assert.Equal(8, manual.MinutosDesdeObservacao(Cenario.Agora));
            });
        Assert.All(registros, registro =>
        {
            Assert.False(registro.Verificada);
            Assert.False(registro.OficialSptrans);
        });
    }

    [Fact]
    public async Task RegistroManual_DeCadastroReal_RecuperaDadosSimuladosFalse()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        await using (PontoVozDbContext contexto = banco.NovoContexto())
        {
            Parada parada = Parada.CadastrarAdministrativa("PARADA-0001", "Parada Real", null, null, false, Cenario.Agora);
            Linha linha = Linha.CadastrarAdministrativa("LINHA-0001", "8000-10", "Centro", "Sul", SentidoLinha.PrincipalParaSecundario, false, Cenario.Agora);
            parada.AssociarLinha(linha, Cenario.Agora);
            contexto.Paradas.Add(parada);
            contexto.RegistrosPassagem.Add(PassagemRegistroManual.Registrar(parada, linha, Cenario.Agora, "V-12", "operador-local", Cenario.Agora));
            await contexto.SaveChangesAsync(Ct);
        }

        await using PontoVozDbContext leitura = banco.NovoContexto();
        var manual = Assert.IsType<PassagemRegistroManual>(await leitura.RegistrosPassagem.SingleAsync(Ct));

        Assert.False(manual.DadosSimulados);
        Assert.Equal("V-12", manual.PrefixoVeiculo);
    }

    [Fact]
    public async Task ChaveSeed_Repetida_ERejeitadaPeloBanco()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        await using PontoVozDbContext contexto = banco.NovoContexto();
        (Parada parada, _, ParadaLinha associacao) = Cenario.ParadaComLinha();
        contexto.Paradas.Add(parada);
        contexto.RegistrosPassagem.Add(PassagemSimulada.CriarNoProvisionamento(associacao, "SIM-OBS-001", TimeSpan.FromMinutes(15), Cenario.Agora, null));
        await contexto.SaveChangesAsync(Ct);

        contexto.RegistrosPassagem.Add(PassagemSimulada.CriarNoProvisionamento(associacao, "SIM-OBS-001", TimeSpan.FromMinutes(30), Cenario.Agora, null));

        var erro = await Assert.ThrowsAsync<DbUpdateException>(() => contexto.SaveChangesAsync(Ct));
        Assert.Contains("UNIQUE constraint failed: RegistrosPassagem.ChaveSeed", erro.InnerException!.Message);
    }

    [Fact]
    public async Task VariosRegistrosManuais_NaoConflitamNoIndiceDaChaveSeed()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();

        await SalvarPassagensManuaisAsync(banco, Referencia, Referencia.AddMinutes(-5), Referencia.AddMinutes(-10));

        await using PontoVozDbContext leitura = banco.NovoContexto();
        Assert.Equal(3, await leitura.RegistrosPassagem.OfType<PassagemRegistroManual>().CountAsync(Ct));
    }

    [Fact]
    public async Task Instantes_SaoPreservadosEmUtcComPrecisaoDeTick()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        DateTimeOffset observadaSaoPaulo = new DateTimeOffset(2026, 11, 10, 14, 24, 0, TimeSpan.FromHours(-3)).AddTicks(1_234_567);
        DateTimeOffset registro = observadaSaoPaulo.AddSeconds(70).AddTicks(9);
        await using (PontoVozDbContext contexto = banco.NovoContexto())
        {
            (Parada parada, Linha linha, _) = Cenario.ParadaComLinha();
            contexto.Paradas.Add(parada);
            contexto.RegistrosPassagem.Add(PassagemRegistroManual.Registrar(parada, linha, observadaSaoPaulo, null, "operador-local", registro));
            await contexto.SaveChangesAsync(Ct);
        }

        await using PontoVozDbContext leitura = banco.NovoContexto();
        RegistroPassagem salva = await leitura.RegistrosPassagem.SingleAsync(Ct);
        long ticksGravados = await leitura.Database.SqlQuery<long>($"SELECT ObservadaEmUtc AS Value FROM RegistrosPassagem").SingleAsync(Ct);

        Assert.Equal(observadaSaoPaulo.UtcTicks, ticksGravados);
        Assert.Equal(observadaSaoPaulo.UtcTicks, salva.ObservadaEmUtc.UtcTicks);
        Assert.Equal(registro.UtcTicks, salva.RegistradaEmUtc.UtcTicks);
        Assert.Equal(TimeSpan.Zero, salva.ObservadaEmUtc.Offset);
        Assert.Equal(TimeSpan.Zero, salva.RegistradaEmUtc.Offset);
        Assert.Equal(17, salva.ObservadaEmUtc.Hour);
    }

    [Fact]
    public async Task Ordenacao_PorObservadaEmUtc_ExecutadaNoBanco()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        DateTimeOffset[] observacoes = [Referencia.AddMinutes(-30), Referencia.AddHours(-2), Referencia.AddTicks(-1), Referencia.AddDays(-1)];
        await SalvarPassagensManuaisAsync(banco, observacoes);

        await using PontoVozDbContext leitura = banco.NovoContexto();
        IQueryable<DateTimeOffset> consulta = leitura.RegistrosPassagem
            .OrderByDescending(passagem => passagem.ObservadaEmUtc)
            .Select(passagem => passagem.ObservadaEmUtc);
        List<DateTimeOffset> resultado = await consulta.ToListAsync(Ct);

        Assert.Contains("ORDER BY \"r\".\"ObservadaEmUtc\" DESC", consulta.ToQueryString());
        Assert.Equal(observacoes.OrderDescending(), resultado);
    }

    [Theory]
    [InlineData(-1L, true)]
    [InlineData(0L, true)]
    [InlineData(1L, false)]
    public async Task Filtro_PorInstanteDeReferencia_UsaOLimiteInclusivoNoBanco(long ticksAposReferencia, bool incluida)
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        DateTimeOffset observadaEm = Referencia.AddTicks(ticksAposReferencia);
        await SalvarPassagensManuaisAsync(banco, observadaEm, Referencia.AddHours(-1));

        await using PontoVozDbContext leitura = banco.NovoContexto();
        DateTimeOffset referencia = Referencia;
        IQueryable<RegistroPassagem> consulta = leitura.RegistrosPassagem.Where(passagem => passagem.ObservadaEmUtc <= referencia);
        List<DateTimeOffset> encontradas = await consulta.Select(passagem => passagem.ObservadaEmUtc).ToListAsync(Ct);

        string sql = consulta.ToQueryString();
        Assert.Contains("WHERE \"r\".\"ObservadaEmUtc\" <= @referencia", sql);
        Assert.Contains($".param set @referencia {Referencia.UtcTicks}", sql);
        Assert.Equal(incluida, encontradas.Contains(observadaEm));
        Assert.Contains(Referencia.AddHours(-1), encontradas);
    }

    [Fact]
    public async Task Filtro_ComReferenciaEmOutroFuso_ComparaOMesmoInstante()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        await SalvarPassagensManuaisAsync(banco, Referencia, Referencia.AddMinutes(1));
        DateTimeOffset referenciaSaoPaulo = Referencia.ToOffset(TimeSpan.FromHours(-3));

        await using PontoVozDbContext leitura = banco.NovoContexto();
        int quantidade = await leitura.RegistrosPassagem.CountAsync(passagem => passagem.ObservadaEmUtc <= referenciaSaoPaulo, Ct);

        Assert.Equal(1, quantidade);
    }
}

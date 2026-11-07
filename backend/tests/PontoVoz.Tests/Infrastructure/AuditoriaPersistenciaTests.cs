using Microsoft.EntityFrameworkCore;

using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Infrastructure.Persistence;
using PontoVoz.Tests.Domain;

namespace PontoVoz.Tests.Infrastructure;

/// <summary>
/// Comportamentos do armazenamento que o B4 precisa respeitar: a natureza das passagens
/// simuladas e a busca sem acentos.
/// </summary>
public class AuditoriaPersistenciaTests
{
    private static CancellationToken Ct => BancoSqliteTeste.Ct;

    private static async Task SalvarPassagensAsync(BancoSqliteTeste banco)
    {
        await using PontoVozDbContext contexto = banco.NovoContexto();
        Parada paradaSimulada = Cenario.ParadaSimulada();
        Linha linhaSimulada = Cenario.LinhaSimulada();
        ParadaLinha associacao = paradaSimulada.AssociarLinha(linhaSimulada, Cenario.Agora);

        Parada paradaReal = Parada.CadastrarAdministrativa("PARADA-0001", "Parada Real", null, null, false, Cenario.Agora);
        Linha linhaReal = Linha.CadastrarAdministrativa("LINHA-0001", "8000-10", "Centro", "Sul", SentidoLinha.PrincipalParaSecundario, false, Cenario.Agora);
        paradaReal.AssociarLinha(linhaReal, Cenario.Agora);

        contexto.Paradas.AddRange(paradaSimulada, paradaReal);
        contexto.RegistrosPassagem.AddRange(
            PassagemSimulada.CriarNoProvisionamento(associacao, "SIM-OBS-001", TimeSpan.FromMinutes(15), Cenario.Agora, null),
            PassagemRegistroManual.Registrar(paradaSimulada, linhaSimulada, Cenario.Agora, null, "operador-local", Cenario.Agora),
            PassagemRegistroManual.Registrar(paradaReal, linhaReal, Cenario.Agora, null, "operador-local", Cenario.Agora));
        await contexto.SaveChangesAsync(Ct);
    }

    [Fact]
    public async Task PassagemSimulada_GravaDadosSimuladosNulo_MasERecuperadaComoFicticiaPeloTipo()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        await SalvarPassagensAsync(banco);

        await using PontoVozDbContext leitura = banco.NovoContexto();
        List<string> colunas = await leitura.Database
            .SqlQuery<string>($"SELECT TipoPassagem || ':' || COALESCE(CAST(DadosSimulados AS TEXT), 'NULL') AS Value FROM RegistrosPassagem")
            .ToListAsync(Ct);
        List<RegistroPassagem> registros = await leitura.RegistrosPassagem.ToListAsync(Ct);

        // A coluna é do registro manual; a passagem simulada é fictícia pelo próprio tipo (discriminador).
        Assert.Equal(new[] { "RegistroManual:0", "RegistroManual:1", "Simulada:NULL" }, colunas.Order(StringComparer.Ordinal));
        Assert.True(registros.OfType<PassagemSimulada>().Single().DadosSimulados);
        Assert.Equal(new[] { false, true }, registros.OfType<PassagemRegistroManual>().Select(p => p.DadosSimulados).Order());
    }

    [Fact]
    public async Task FiltroPelaPropriedadeCalculada_NaoETraduzido_EntaoNaoHaFiltroSilenciosoComNulo()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        await SalvarPassagensAsync(banco);
        await using PontoVozDbContext leitura = banco.NovoContexto();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            leitura.RegistrosPassagem.Where(p => p.DadosSimulados).ToListAsync(Ct));
    }

    [Fact]
    public async Task FiltroDePassagensFicticias_PeloDiscriminadorEPelaColunaDoRegistroManual_RodaNoBanco()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        await SalvarPassagensAsync(banco);
        await using PontoVozDbContext leitura = banco.NovoContexto();

        // Forma que o B4 deve usar, se precisar filtrar no SQL.
        IQueryable<RegistroPassagem> ficticias = leitura.RegistrosPassagem.Where(p =>
            p is PassagemSimulada
            || (p is PassagemRegistroManual && EF.Property<bool>((PassagemRegistroManual)p, "_dadosSimulados")));
        List<RegistroPassagem> resultado = await ficticias.ToListAsync(Ct);

        Assert.Contains("\"TipoPassagem\" = 'Simulada'", ficticias.ToQueryString());
        Assert.Equal(2, resultado.Count);
        Assert.All(resultado, p => Assert.True(p.DadosSimulados));
    }

    [Fact]
    public async Task BuscaComLikeDoSqlite_NaoIgnoraAcentos_PorIssoOB4PrecisaDeNormalizacao()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync(provisionarDemonstracao: true);
        await using PontoVozDbContext leitura = banco.NovoContexto();

        int semAcento = await leitura.Paradas.CountAsync(p => EF.Functions.Like(p.Nome, "%demonstracao%"), Ct);
        int comAcento = await leitura.Paradas.CountAsync(p => EF.Functions.Like(p.Nome, "%demonstração%"), Ct);
        int maiusculasComAcento = await leitura.Paradas.CountAsync(p => EF.Functions.Like(p.Nome, "%DEMONSTRAÇÃO%"), Ct);

        Assert.Equal(0, semAcento);
        Assert.Equal(2, comAcento);
        Assert.Equal(0, maiusculasComAcento);
    }
}

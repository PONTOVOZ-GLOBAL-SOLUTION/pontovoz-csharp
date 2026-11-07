using Microsoft.EntityFrameworkCore;

using PontoVoz.Api.Infrastructure.Persistence;

namespace PontoVoz.Tests.Infrastructure;

public class EsquemaMigrationTests
{
    private static Task<List<string>> ConsultarAsync(PontoVozDbContext contexto, FormattableString sql) =>
        contexto.Database.SqlQuery<string>(sql).ToListAsync(BancoSqliteTeste.Ct);

    [Fact]
    public async Task Migration_CriaAsTabelasEsperadas_ERegistraOHistorico()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        await using PontoVozDbContext contexto = banco.NovoContexto();

        List<string> tabelas = await ConsultarAsync(contexto,
            $"SELECT name AS Value FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%' AND name NOT LIKE '__EF%' ORDER BY name");

        Assert.Equal(new[] { "Destinos", "Linhas", "Paradas", "ParadasLinhas", "PercursosEstimados", "RegistrosPassagem" }, tabelas);
        Assert.Equal(new[] { "20261107211456_InitialCreate" }, await contexto.Database.GetAppliedMigrationsAsync(BancoSqliteTeste.Ct));
        Assert.Empty(await contexto.Database.GetPendingMigrationsAsync(BancoSqliteTeste.Ct));
    }

    [Fact]
    public async Task Migration_CorrespondeAoModeloAtual()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        await using PontoVozDbContext contexto = banco.NovoContexto();

        Assert.False(contexto.Database.HasPendingModelChanges());
    }

    [Fact]
    public async Task Migration_CriaOsIndicesUnicosEOFiltroDaChaveSeed()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        await using PontoVozDbContext contexto = banco.NovoContexto();

        List<string> indices = await ConsultarAsync(contexto,
            $"SELECT sql AS Value FROM sqlite_master WHERE type = 'index' AND sql LIKE 'CREATE UNIQUE INDEX%' ORDER BY name");

        Assert.Collection(indices,
            indice => Assert.Contains("\"Destinos\" (\"Nome\")", indice),
            indice => Assert.Contains("\"Linhas\" (\"Codigo\")", indice),
            indice => Assert.Contains("\"Paradas\" (\"CodigoExterno\")", indice),
            indice => Assert.Contains("\"RegistrosPassagem\" (\"ChaveSeed\") WHERE \"ChaveSeed\" IS NOT NULL", indice));
    }

    [Fact]
    public async Task Migration_NaoCriaExclusaoEmCascata()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync();
        await using PontoVozDbContext contexto = banco.NovoContexto();

        List<string> acoes = await ConsultarAsync(contexto,
            $"SELECT DISTINCT fk.on_delete AS Value FROM sqlite_master t, pragma_foreign_key_list(t.name) fk WHERE t.type = 'table'");

        Assert.Equal(new[] { "RESTRICT" }, acoes);
    }
}

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;

using PontoVoz.Api.Infrastructure.Persistence;
using PontoVoz.Api.Infrastructure.Persistence.Seed;

namespace PontoVoz.Tests.Infrastructure;

/// <summary>
/// Banco SQLite real e isolado para cada teste. Por padrão fica em memória, com a conexão aberta
/// durante o teste; o esquema é criado pela migration, não por <c>EnsureCreated</c>.
/// </summary>
internal sealed class BancoSqliteTeste : IAsyncDisposable
{
    public static readonly DateTimeOffset InicioRelogio = new(2026, 11, 8, 12, 0, 0, TimeSpan.Zero);

    private readonly SqliteConnection _conexao;
    private readonly ProvisionamentoDemonstracao? _provisionamento;

    private BancoSqliteTeste(string connectionString, bool provisionarDemonstracao)
    {
        _conexao = new SqliteConnection(connectionString);
        Relogio = new FakeTimeProvider(InicioRelogio);
        _provisionamento = provisionarDemonstracao ? new ProvisionamentoDemonstracao(Relogio) : null;
    }

    public FakeTimeProvider Relogio { get; }

    public static CancellationToken Ct => TestContext.Current.CancellationToken;

    /// <summary>Cria um banco em memória e aplica as migrations (e o seed, se pedido).</summary>
    public static Task<BancoSqliteTeste> CriarAsync(bool provisionarDemonstracao = false) =>
        CriarAsync("Data Source=:memory:", provisionarDemonstracao);

    public static async Task<BancoSqliteTeste> CriarAsync(string connectionString, bool provisionarDemonstracao)
    {
        var banco = new BancoSqliteTeste(connectionString, provisionarDemonstracao);
        try
        {
            await banco._conexao.OpenAsync(Ct);
            await using PontoVozDbContext contexto = banco.NovoContexto();
            await contexto.Database.MigrateAsync(Ct);
            return banco;
        }
        catch
        {
            // Fecha a conexão para que um arquivo temporário possa ser apagado pelo teste.
            await banco.DisposeAsync();
            throw;
        }
    }

    /// <summary>Um contexto novo sobre a mesma conexão, como uma nova requisição ou reinício.</summary>
    public PontoVozDbContext NovoContexto()
    {
        var options = new DbContextOptionsBuilder<PontoVozDbContext>();
        options.UsarSqlitePontoVoz(_conexao, _provisionamento);
        return new PontoVozDbContext(options.Options);
    }

    public async ValueTask DisposeAsync() => await _conexao.DisposeAsync();
}

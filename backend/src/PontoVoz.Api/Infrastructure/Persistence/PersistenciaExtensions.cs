using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using PontoVoz.Api.Infrastructure.Persistence.Seed;

namespace PontoVoz.Api.Infrastructure.Persistence;

public static class PersistenciaExtensions
{
    public const string NomeConnectionString = "PontoVoz";

    /// <summary>
    /// Registra o <see cref="PontoVozDbContext"/> com SQLite. Um caminho relativo em
    /// <c>Data Source</c> é resolvido a partir da pasta do projeto (content root), para que a API e
    /// o <c>dotnet ef</c> usem o mesmo arquivo.
    /// </summary>
    public static IServiceCollection AddPersistenciaPontoVoz(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        string connectionString = configuration.GetConnectionString(NomeConnectionString)
            ?? throw new InvalidOperationException($"A connection string '{NomeConnectionString}' não foi configurada.");
        string resolvida = ResolverCaminho(connectionString, environment.ContentRootPath);

        services.AddDbContext<PontoVozDbContext>((provider, options) =>
            options.UsarSqlitePontoVoz(resolvida, new ProvisionamentoDemonstracao(provider.GetRequiredService<TimeProvider>())));

        // O /health passa a verificar também o banco (contrato, seções 1.10 e 6).
        services.AddHealthChecks().AddCheck<BancoDadosHealthCheck>(BancoDadosHealthCheck.Nome);

        return services;
    }

    /// <summary>
    /// Configuração única do SQLite, usada pela API e pelos testes de integração. Com
    /// <paramref name="provisionamento"/>, os dados de demonstração são criados depois das
    /// migrations, nos fluxos síncrono e assíncrono.
    /// </summary>
    public static DbContextOptionsBuilder UsarSqlitePontoVoz(this DbContextOptionsBuilder options, string connectionString, ProvisionamentoDemonstracao? provisionamento) =>
        options.UseSqlite(connectionString).ComProvisionamento(provisionamento);

    /// <inheritdoc cref="UsarSqlitePontoVoz(DbContextOptionsBuilder, string, ProvisionamentoDemonstracao?)"/>
    public static DbContextOptionsBuilder UsarSqlitePontoVoz(this DbContextOptionsBuilder options, SqliteConnection connection, ProvisionamentoDemonstracao? provisionamento) =>
        options.UseSqlite(connection).ComProvisionamento(provisionamento);

    private static DbContextOptionsBuilder ComProvisionamento(this DbContextOptionsBuilder options, ProvisionamentoDemonstracao? provisionamento)
    {
        if (provisionamento is null)
        {
            return options;
        }

        return options
            .UseSeeding((contexto, _) => provisionamento.Executar(contexto))
            .UseAsyncSeeding((contexto, _, cancellationToken) => provisionamento.ExecutarAsync(contexto, cancellationToken));
    }

    private static string ResolverCaminho(string connectionString, string contentRoot)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);
        if (!string.IsNullOrEmpty(builder.DataSource)
            && builder.DataSource != ":memory:"
            && builder.Mode != SqliteOpenMode.Memory
            && !Path.IsPathRooted(builder.DataSource))
        {
            builder.DataSource = Path.Combine(contentRoot, builder.DataSource);
        }

        return builder.ToString();
    }
}

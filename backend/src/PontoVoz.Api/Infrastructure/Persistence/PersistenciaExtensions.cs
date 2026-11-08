using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using PontoVoz.Api.Application.Interfaces;
using PontoVoz.Api.Infrastructure.Persistence.Busca;
using PontoVoz.Api.Infrastructure.Persistence.Seed;
using PontoVoz.Api.Infrastructure.Repositories;

namespace PontoVoz.Api.Infrastructure.Persistence;

public static class PersistenciaExtensions
{
    public const string NomeConnectionString = "PontoVoz";

    /// <summary>Configuração que liga a aplicação das migrations ao iniciar (só vale em Development).</summary>
    public const string ChaveAplicarMigracoesAoIniciar = "Banco:AplicarMigracoesAoIniciar";

    /// <summary>
    /// Em Development, com <c>Banco:AplicarMigracoesAoIniciar</c> verdadeira, aplica as migrations
    /// pendentes e o seed de demonstração antes de a API atender requisições. Assim, quem executa o
    /// projeto pela primeira vez não depende do <c>dotnet ef database update</c>. Usa as mesmas
    /// migrations do comando (não usa <c>EnsureCreated</c>) e é idempotente: num banco já
    /// atualizado e provisionado, nada é gravado. Fora de Development o banco nunca é alterado na
    /// inicialização: a opção é ignorada, com um aviso no log.
    /// </summary>
    public static async Task AplicarMigracoesEmDesenvolvimentoAsync(this IHost host, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(host);
        IConfiguration configuracao = host.Services.GetRequiredService<IConfiguration>();
        if (!configuracao.GetValue<bool>(ChaveAplicarMigracoesAoIniciar))
        {
            return;
        }

        ILogger logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("PontoVoz.Api.Infrastructure.Persistence.Migracoes");
        IHostEnvironment ambiente = host.Services.GetRequiredService<IHostEnvironment>();
        if (!ambiente.IsDevelopment())
        {
            logger.LogWarning("{Chave} ignorada no ambiente {Ambiente}: as migrations só são aplicadas ao iniciar em Development.",
                ChaveAplicarMigracoesAoIniciar, ambiente.EnvironmentName);
            return;
        }

        await using AsyncServiceScope escopo = host.Services.CreateAsyncScope();
        PontoVozDbContext contexto = escopo.ServiceProvider.GetRequiredService<PontoVozDbContext>();
        await contexto.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("Banco verificado na inicialização: migrations pendentes e dados de demonstração aplicados quando necessário.");
    }

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

        // Repositórios de consulta: mesmo tempo de vida do DbContext.
        services.AddScoped<IParadaRepository, ParadaRepository>();
        services.AddScoped<ILinhaRepository, LinhaRepository>();
        services.AddScoped<IDestinoRepository, DestinoRepository>();
        services.AddScoped<IRegistroPassagemRepository, RegistroPassagemRepository>();

        // Escrita das rotas administrativas: as mesmas classes, pelas interfaces de cadastro.
        services.AddScoped<ICadastroParadaRepository, ParadaRepository>();
        services.AddScoped<ICadastroLinhaRepository, LinhaRepository>();
        services.AddScoped<ICadastroPassagemRepository, RegistroPassagemRepository>();
        services.AddScoped<IUnidadeTrabalho, UnidadeTrabalhoEf>();

        // O /health passa a verificar também o banco (contrato, seções 1.10 e 6).
        services.AddHealthChecks().AddCheck<BancoDadosHealthCheck>(BancoDadosHealthCheck.Nome);

        return services;
    }

    /// <summary>
    /// Configuração única do SQLite, usada pela API e pelos testes de integração. Depois das
    /// migrations, nos fluxos síncrono e assíncrono, preenche a coluna de busca dos registros
    /// antigos e, com <paramref name="provisionamento"/>, cria os dados de demonstração.
    /// </summary>
    public static DbContextOptionsBuilder UsarSqlitePontoVoz(this DbContextOptionsBuilder options, string connectionString, ProvisionamentoDemonstracao? provisionamento) =>
        options.UseSqlite(connectionString).ComEtapasPosMigracao(provisionamento);

    /// <inheritdoc cref="UsarSqlitePontoVoz(DbContextOptionsBuilder, string, ProvisionamentoDemonstracao?)"/>
    public static DbContextOptionsBuilder UsarSqlitePontoVoz(this DbContextOptionsBuilder options, SqliteConnection connection, ProvisionamentoDemonstracao? provisionamento) =>
        options.UseSqlite(connection).ComEtapasPosMigracao(provisionamento);

    private static DbContextOptionsBuilder ComEtapasPosMigracao(this DbContextOptionsBuilder options, ProvisionamentoDemonstracao? provisionamento) =>
        options
            .UseSeeding((contexto, _) =>
            {
                PreenchimentoTextoBusca.Executar(Contexto(contexto));
                provisionamento?.Executar(contexto);
            })
            .UseAsyncSeeding(async (contexto, _, cancellationToken) =>
            {
                await PreenchimentoTextoBusca.ExecutarAsync(Contexto(contexto), cancellationToken);
                if (provisionamento is not null)
                {
                    await provisionamento.ExecutarAsync(contexto, cancellationToken);
                }
            });

    private static PontoVozDbContext Contexto(DbContext contexto) =>
        contexto as PontoVozDbContext
        ?? throw new ArgumentException($"A configuração exige um {nameof(PontoVozDbContext)}.", nameof(contexto));

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

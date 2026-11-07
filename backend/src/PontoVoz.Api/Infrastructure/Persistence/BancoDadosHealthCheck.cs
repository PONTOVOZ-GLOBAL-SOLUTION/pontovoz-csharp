using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PontoVoz.Api.Infrastructure.Persistence;

/// <summary>
/// Verifica se o SQLite está acessível e com todas as migrations aplicadas. Só lê o banco: não
/// cria o arquivo, não aplica migrations e não roda o seed. As descrições são genéricas para não
/// expor caminhos nem a connection string.
/// </summary>
public sealed class BancoDadosHealthCheck(PontoVozDbContext contexto) : IHealthCheck
{
    public const string Nome = "banco-de-dados";

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await contexto.Database.CanConnectAsync(cancellationToken))
            {
                return HealthCheckResult.Unhealthy("Banco de dados indisponível.");
            }

            IEnumerable<string> pendentes = await contexto.Database.GetPendingMigrationsAsync(cancellationToken);
            return pendentes.Any()
                ? HealthCheckResult.Unhealthy("Banco de dados sem todas as migrations aplicadas.")
                : HealthCheckResult.Healthy("Banco de dados acessível.");
        }
        catch (Exception erro) when (erro is not OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy("Banco de dados indisponível.");
        }
    }
}

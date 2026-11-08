namespace PontoVoz.Api.Security;

/// <summary>
/// Registra no log, a cada inicialização, quando as operações administrativas estão sem
/// proteção por chave (Development sem <c>Admin:Chave</c>, seção 5.1 do contrato). Fora de
/// Development sem chave, avisa que elas estão desabilitadas. Nunca registra o valor da chave.
/// </summary>
public sealed class AvisoChaveAdministrativa(
    IConfiguration configuracao,
    IHostEnvironment ambiente,
    ILogger<AvisoChaveAdministrativa> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (ChaveAdministrativaFilter.LerChave(configuracao) is null)
        {
            if (ambiente.IsDevelopment())
            {
                logger.LogWarning(
                    "As operações administrativas (/api/v1/admin) estão SEM proteção por chave: ambiente Development sem {Secao}. "
                    + "Use só para demonstração local; configure a chave com dotnet user-secrets ou variável de ambiente.",
                    ChaveAdministrativaFilter.SecaoChave);
            }
            else
            {
                logger.LogWarning(
                    "As operações administrativas (/api/v1/admin) estão desabilitadas: nenhuma chave em {Secao} no ambiente {Ambiente}.",
                    ChaveAdministrativaFilter.SecaoChave, ambiente.EnvironmentName);
            }
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

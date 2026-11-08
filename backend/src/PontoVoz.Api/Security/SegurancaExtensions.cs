namespace PontoVoz.Api.Security;

public static class SegurancaExtensions
{
    /// <summary>
    /// Registra o filtro da chave administrativa (aplicado pelas rotas <c>/api/v1/admin</c>) e o
    /// aviso de inicialização. A chave é lida de <c>Admin:Chave</c> a cada requisição, pela
    /// configuração do .NET (user-secrets em Development ou variável de ambiente <c>Admin__Chave</c>).
    /// </summary>
    public static IServiceCollection AddSegurancaAdministrativa(this IServiceCollection services)
    {
        services.AddScoped<ChaveAdministrativaFilter>();
        services.AddHostedService<AvisoChaveAdministrativa>();
        return services;
    }
}

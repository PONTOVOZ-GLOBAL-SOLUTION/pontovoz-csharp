using PontoVoz.Api.Application.Interfaces;
using PontoVoz.Api.Application.Services;

namespace PontoVoz.Api.Application;

public static class AplicacaoExtensions
{
    /// <summary>
    /// Registra os services de consulta. São <c>scoped</c>, como os repositórios e o
    /// <c>DbContext</c> que eles usam; o <see cref="TimeProvider"/> já é registrado no <c>Program.cs</c>.
    /// </summary>
    public static IServiceCollection AddAplicacaoPontoVoz(this IServiceCollection services)
    {
        services.AddScoped<IParadaService, ParadaService>();
        services.AddScoped<ILinhaService, LinhaService>();
        services.AddScoped<IDestinoService, DestinoService>();
        services.AddScoped<IUltimasPassagensService, UltimasPassagensService>();
        return services;
    }
}

using System.Net;

using Microsoft.Extensions.Options;

using PontoVoz.Api.Application.Interfaces;

namespace PontoVoz.Api.Infrastructure.Providers;

public static class ProvedoresExtensions
{
    /// <summary>
    /// Registra os provedores de previsão (B8): a configuração <c>Previsoes</c>, o provedor
    /// simulado, o adaptador Olho Vivo (HttpClient próprio, HTTPS, prazo explícito e sem os loggers
    /// padrão, que gravariam a URL de autenticação com o token) e o seletor exposto como
    /// <see cref="IProvedorPrevisaoChegada"/>.
    /// </summary>
    public static IServiceCollection AddProvedoresPrevisaoPontoVoz(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OpcoesPrevisoes>(configuration.GetSection(OpcoesPrevisoes.Secao));
        services.AddSingleton<ProvedorPrevisaoSimulado>();

        services.AddHttpClient<ProvedorOlhoVivo>((provider, cliente) =>
            {
                OpcoesOlhoVivo configuracao = provider.GetRequiredService<IOptionsMonitor<OpcoesPrevisoes>>().CurrentValue.OlhoVivo;
                var endereco = new Uri(configuracao.UrlBase.EndsWith('/') ? configuracao.UrlBase : configuracao.UrlBase + "/");
                if (endereco.Scheme != Uri.UriSchemeHttps)
                {
                    throw new InvalidOperationException("O adaptador Olho Vivo só aceita endereço HTTPS.");
                }

                cliente.BaseAddress = endereco;

                // O prazo de cada consulta é controlado pelo adaptador; este é só um teto de segurança.
                cliente.Timeout = configuracao.Prazo + TimeSpan.FromSeconds(5);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                // A sessão da Olho Vivo usa um cookie devolvido pelo login.
                UseCookies = true,
                CookieContainer = new CookieContainer(),
            })
            .RemoveAllLoggers();

        services.AddScoped<IProvedorPrevisaoChegada, SeletorProvedorPrevisao>();
        return services;
    }
}

using System.Security.Cryptography;
using System.Text;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace PontoVoz.Api.Security;

/// <summary>
/// Protege as rotas <c>/api/v1/admin</c> com a chave do cabeçalho <c>X-PontoVoz-Admin-Key</c>
/// (contracts/api-v1.md, seção 5.1):
/// <list type="table">
/// <item><term>Development, sem <c>Admin:Chave</c></term><description>permitido (aviso no log na inicialização)</description></item>
/// <item><term>Development, com chave</term><description>exige o cabeçalho; sem ele ou errado: 401</description></item>
/// <item><term>Outros ambientes, sem chave</term><description>403 <c>OperacoesAdministrativasDesabilitadas</c></description></item>
/// <item><term>Outros ambientes, com chave</term><description>exige o cabeçalho; sem ele ou errado: 401</description></item>
/// </list>
/// É um filtro de <b>autorização</b> do MVC, que roda antes do model binding: a leitura e a
/// validação do corpo só acontecem depois. Por isso uma requisição sem autorização recebe 401 ou
/// 403, nunca 400. O valor da chave não vai para o log nem para a resposta.
/// </summary>
public sealed class ChaveAdministrativaFilter(
    IConfiguration configuracao,
    IHostEnvironment ambiente,
    ProblemDetailsFactory problemas,
    ILogger<ChaveAdministrativaFilter> logger) : IAsyncAuthorizationFilter
{
    public const string Cabecalho = "X-PontoVoz-Admin-Key";
    public const string SecaoChave = "Admin:Chave";

    /// <summary>Valor de <c>WWW-Authenticate</c> no 401 (seção 5.1).</summary>
    public const string DesafioWwwAuthenticate = "ApiKey header=\"X-PontoVoz-Admin-Key\"";

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        string? chaveConfigurada = LerChave(configuracao);

        if (chaveConfigurada is null)
        {
            if (!ambiente.IsDevelopment())
            {
                logger.LogWarning("Operação administrativa recusada: nenhuma chave configurada fora de Development.");
                context.Result = Recusar(context.HttpContext, StatusCodes.Status403Forbidden, "Operações administrativas desabilitadas",
                    "OperacoesAdministrativasDesabilitadas",
                    "As operações administrativas estão desabilitadas porque nenhuma chave foi configurada neste ambiente.");
            }

            // Development sem chave: permitido pelo contrato, com o aviso de AvisoChaveAdministrativa.
            return Task.CompletedTask;
        }

        string? recebida = context.HttpContext.Request.Headers[Cabecalho].Count == 1
            ? context.HttpContext.Request.Headers[Cabecalho].ToString()
            : null;

        if (recebida is null || !ChavesIguais(recebida, chaveConfigurada))
        {
            logger.LogWarning("Operação administrativa recusada: chave ausente ou incorreta no cabeçalho {Cabecalho}.", Cabecalho);
            context.HttpContext.Response.Headers.WWWAuthenticate = DesafioWwwAuthenticate;
            context.Result = Recusar(context.HttpContext, StatusCodes.Status401Unauthorized, "Chave administrativa inválida",
                "ChaveAdministrativaInvalida",
                $"Informe a chave administrativa correta no cabeçalho {Cabecalho}.");
        }

        return Task.CompletedTask;
    }

    /// <summary>A chave configurada, ou <c>null</c> quando ausente, vazia ou só com espaços.</summary>
    internal static string? LerChave(IConfiguration configuracao)
    {
        string? chave = configuracao[SecaoChave];
        return string.IsNullOrWhiteSpace(chave) ? null : chave;
    }

    /// <summary>
    /// Comparação em tempo constante. Os dois lados passam antes por SHA-256, para que a
    /// comparação tenha sempre o mesmo tamanho e o tempo não revele nem o comprimento da chave.
    /// </summary>
    internal static bool ChavesIguais(string recebida, string configurada) =>
        CryptographicOperations.FixedTimeEquals(
            SHA256.HashData(Encoding.UTF8.GetBytes(recebida)),
            SHA256.HashData(Encoding.UTF8.GetBytes(configurada)));

    private ObjectResult Recusar(HttpContext httpContext, int status, string titulo, string codigo, string detalhe)
    {
        ProblemDetails problema = problemas.CreateProblemDetails(httpContext, status, titulo, detail: detalhe);
        problema.Extensions["codigo"] = codigo;
        return new ObjectResult(problema)
        {
            StatusCode = status,
            ContentTypes = { "application/problem+json" },
        };
    }
}

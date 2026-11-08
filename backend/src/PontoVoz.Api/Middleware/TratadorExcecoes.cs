using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace PontoVoz.Api.Middleware;

/// <summary>
/// Tratamento global de exceções. Erros esperados (de aplicação e de domínio) viram o
/// ProblemDetails do catálogo; qualquer outra exceção vira 500 <c>ErroInterno</c> genérico e é
/// registrada no log, sem chegar ao cliente.
/// <para>
/// Requisição cancelada pelo cliente não chega aqui: o <c>ExceptionHandlerMiddleware</c> do
/// ASP.NET Core reconhece o cancelamento (<c>RequestAborted</c>), registra só em Debug e não
/// escreve resposta. Por isso o cancelamento não vira 500 (testado em InfraestruturaHttpTests).
/// </para>
/// </summary>
internal sealed class TratadorExcecoes(IProblemDetailsService problemDetails, ILogger<TratadorExcecoes> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ProblemDetails? esperado = ProblemasHttp.DeExcecao(exception);
        if (esperado is null)
        {
            logger.LogError(exception, "Erro inesperado em {Metodo} {Caminho}.", httpContext.Request.Method, httpContext.Request.Path);
        }

        ProblemDetails problema = esperado ?? ProblemasHttp.ErroInterno();
        httpContext.Response.StatusCode = problema.Status!.Value;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problema,
        });
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PontoVoz.Api.Controllers;

/// <summary>
/// Informa se o processo da API está em execução e o resultado de cada verificação registrada.
/// A verificação <c>banco-de-dados</c> confere o SQLite; com ela, o escopo é
/// <c>processo-e-dependencias</c>, e uma falha devolve 503.
/// </summary>
[ApiController]
[Route("health")]
public sealed class HealthController(HealthCheckService healthCheckService, TimeProvider timeProvider) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<SaudeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<SaudeResponse>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<SaudeResponse>> Obter(CancellationToken cancellationToken)
    {
        HealthReport relatorio = await healthCheckService.CheckHealthAsync(cancellationToken);

        var verificacoes = relatorio.Entries
            .Select(entrada => new VerificacaoSaudeResponse(entrada.Key, entrada.Value.Status.ToString()))
            .ToList();

        var resposta = new SaudeResponse(
            Status: relatorio.Status.ToString(),
            Escopo: verificacoes.Count == 0 ? "processo" : "processo-e-dependencias",
            VerificadoEm: timeProvider.GetUtcNow(),
            Verificacoes: verificacoes);

        return relatorio.Status == HealthStatus.Unhealthy
            ? StatusCode(StatusCodes.Status503ServiceUnavailable, resposta)
            : Ok(resposta);
    }
}

public sealed record SaudeResponse(
    string Status,
    string Escopo,
    DateTimeOffset VerificadoEm,
    IReadOnlyList<VerificacaoSaudeResponse> Verificacoes);

public sealed record VerificacaoSaudeResponse(string Nome, string Status);

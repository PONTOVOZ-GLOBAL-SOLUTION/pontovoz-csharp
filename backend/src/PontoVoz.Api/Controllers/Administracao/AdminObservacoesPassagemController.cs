using Microsoft.AspNetCore.Mvc;

using PontoVoz.Api.Application.Dtos;
using PontoVoz.Api.Application.Interfaces;

namespace PontoVoz.Api.Controllers.Administracao;

/// <summary>Registro manual de passagens observadas (seção 5.6).</summary>
[Tags(TagAdministracao)]
public sealed class AdminObservacoesPassagemController(IObservacoesPassagemService observacoes) : AdministracaoControllerBase
{
    /// <summary>Registra que um ônibus de uma linha foi visto na parada.</summary>
    /// <remarks>
    /// observadaEm precisa trazer o fuso e estar entre 24 horas antes e 2 minutos depois do horário
    /// do servidor. O registro sai como RegistroManual, com verificada e oficialSptrans sempre false.
    /// Um horário ainda no futuro só aparece em ultimas-passagens quando chegar.
    /// </remarks>
    /// <param name="requisicao">Dados da observação.</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    [HttpPost("observacoes-passagem")]
    [EndpointName("RegistrarObservacaoPassagem")]
    [ProducesResponseType<PassagemRegistradaDto>(StatusCodes.Status201Created, "application/json")]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
    public async Task<IActionResult> Registrar([FromBody] ObservacaoPassagemDto? requisicao, CancellationToken cancellationToken) =>
        StatusCode(StatusCodes.Status201Created, await observacoes.RegistrarAsync(requisicao!, cancellationToken));
}

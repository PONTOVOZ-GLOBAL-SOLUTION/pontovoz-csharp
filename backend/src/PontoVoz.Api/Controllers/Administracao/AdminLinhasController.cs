using Microsoft.AspNetCore.Mvc;

using PontoVoz.Api.Application.Dtos;
using PontoVoz.Api.Application.Interfaces;

namespace PontoVoz.Api.Controllers.Administracao;

/// <summary>Cadastro de linhas (seção 5.4).</summary>
[Tags(TagAdministracao)]
public sealed class AdminLinhasController(IAdministracaoLinhasService linhas) : AdministracaoControllerBase
{
    /// <summary>Cadastra uma linha em um sentido. Ela nasce ativa, com origem CadastroAdministrativo e o dadosSimulados declarado.</summary>
    /// <remarks>
    /// Cada sentido é um cadastro próprio. Código e letreiro não podem usar o prefixo reservado SIM-.
    /// Sem Location, porque o MVP não tem consulta pública de linha por id; a linha aparece em GET /api/v1/linhas.
    /// </remarks>
    /// <param name="requisicao">Dados da linha.</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    [HttpPost("linhas")]
    [EndpointName("CadastrarLinha")]
    [ProducesResponseType<LinhaAdministrativaDto>(StatusCodes.Status201Created, "application/json")]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<IActionResult> Cadastrar([FromBody] CadastroLinhaDto? requisicao, CancellationToken cancellationToken) =>
        StatusCode(StatusCodes.Status201Created, await linhas.CadastrarAsync(requisicao!, cancellationToken));
}

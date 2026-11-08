using Microsoft.AspNetCore.Mvc;

using PontoVoz.Api.Application.Dtos;
using PontoVoz.Api.Application.Interfaces;

namespace PontoVoz.Api.Controllers.Administracao;

/// <summary>Cadastro, atualização e associação de linhas a paradas (seções 5.2, 5.3 e 5.5).</summary>
[Tags(TagAdministracao)]
public sealed class AdminParadasController(IAdministracaoParadasService paradas) : AdministracaoControllerBase
{
    /// <summary>Cadastra uma parada. Ela nasce ativa, com origem CadastroAdministrativo e o dadosSimulados declarado.</summary>
    /// <remarks>
    /// O operador precisa declarar dadosSimulados (true para um cadastro fictício); a API não presume
    /// nenhum dos dois. O prefixo SIM- é reservado aos dados de demonstração. Um cadastro fictício
    /// não pode ter endereço nem coordenada. Nenhum cadastro administrativo é verificado.
    /// </remarks>
    /// <param name="requisicao">Dados da parada.</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    [HttpPost("paradas")]
    [EndpointName("CadastrarParada")]
    [ProducesResponseType<ParadaAdministrativaDto>(StatusCodes.Status201Created, "application/json")]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<IActionResult> Cadastrar([FromBody] CadastroParadaDto? requisicao, CancellationToken cancellationToken)
    {
        ParadaAdministrativaDto parada = await paradas.CadastrarAsync(requisicao!, cancellationToken);
        return Created($"/api/v1/paradas/{parada.Id}", parada);
    }

    /// <summary>Atualiza nome, endereço, coordenada e situação (ativa) de uma parada, inclusive inativa.</summary>
    /// <remarks>Substituição completa. codigoExterno, origem e dadosSimulados não mudam. Desativar substitui a exclusão: o histórico é preservado.</remarks>
    /// <param name="id">Id interno da parada (inteiro ≥ 1).</param>
    /// <param name="requisicao">Novos dados editáveis.</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    [HttpPut("paradas/{id:int:min(1)}")]
    [EndpointName("AtualizarParada")]
    [ProducesResponseType<ParadaAdministrativaDto>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public Task<ParadaAdministrativaDto> Atualizar(int id, [FromBody] AtualizacaoParadaDto? requisicao, CancellationToken cancellationToken) =>
        paradas.AtualizarAsync(id, requisicao!, cancellationToken);

    /// <summary>Indica que a linha atende a parada. Idempotente: repetir a chamada também devolve 204.</summary>
    /// <param name="id">Id interno da parada (inteiro ≥ 1).</param>
    /// <param name="linhaId">Id interno da linha (inteiro ≥ 1).</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    [HttpPut("paradas/{id:int:min(1)}/linhas/{linhaId:int:min(1)}")]
    [EndpointName("AssociarLinhaAParada")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
    public async Task<IActionResult> AssociarLinha(int id, int linhaId, CancellationToken cancellationToken)
    {
        await paradas.AssociarLinhaAsync(id, linhaId, cancellationToken);
        return NoContent();
    }
}

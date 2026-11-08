using Microsoft.AspNetCore.Mvc;

using PontoVoz.Api.Application.Consultas;
using PontoVoz.Api.Application.Dtos;
using PontoVoz.Api.Application.Interfaces;

namespace PontoVoz.Api.Controllers;

/// <summary>
/// Paradas e últimas passagens (contracts/api-v1.md, seções 4.1, 4.2 e 4.6). O controller só
/// recebe os parâmetros e chama os services; validação, regras e erros ficam no B5 e no
/// tratamento global de erros.
/// </summary>
[ApiController]
[Route("api/v1/paradas")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, "application/problem+json")]
public sealed class ParadasController(IParadaService paradas, IUltimasPassagensService passagens, IPrevisoesService previsoes) : ControllerBase
{
    /// <summary>Busca paradas ativas por nome ou código, ou identifica a parada do totem pelo <c>codigoExterno</c>.</summary>
    /// <param name="termo">Trecho do nome ou do código, de 2 a 60 caracteres, sem diferenciar maiúsculas e acentos.</param>
    /// <param name="codigoExterno">Código da parada, exato e sem diferenciar maiúsculas (por exemplo, SIM-P001).</param>
    /// <param name="pagina">Página, a partir de 1. Padrão: 1.</param>
    /// <param name="tamanhoPagina">Itens por página, de 1 a 50. Padrão: 20.</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    [HttpGet]
    [EndpointName("BuscarParadas")]
    [ProducesResponseType<PaginaDto<ParadaDto>>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public Task<PaginaDto<ParadaDto>> Buscar(
        [FromQuery] string? termo,
        [FromQuery] string? codigoExterno,
        [FromQuery] int? pagina,
        [FromQuery] int? tamanhoPagina,
        CancellationToken cancellationToken) =>
        paradas.BuscarAsync(new ConsultaParadas(termo, codigoExterno, pagina, tamanhoPagina), cancellationToken);

    /// <summary>Detalhe de uma parada ativa, com as linhas ativas que a atendem.</summary>
    /// <param name="id">Id interno da parada (inteiro ≥ 1).</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    [HttpGet("{id:int:min(1)}")]
    [EndpointName("ObterParada")]
    [ProducesResponseType<ParadaDetalheDto>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public Task<ParadaDetalheDto> Obter(int id, CancellationToken cancellationToken) =>
        paradas.ObterDetalheAsync(id, cancellationToken);

    /// <summary>Última passagem registrada de cada linha da parada, com nível de confirmação e desatualização.</summary>
    /// <param name="id">Id interno da parada (inteiro ≥ 1).</param>
    /// <param name="linhaId">Opcional: limita a uma linha que atende a parada.</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    [HttpGet("{id:int:min(1)}/ultimas-passagens")]
    [EndpointName("ObterUltimasPassagens")]
    [ProducesResponseType<UltimasPassagensParadaDto>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
    public Task<UltimasPassagensParadaDto> ObterUltimasPassagens(int id, [FromQuery] int? linhaId, CancellationToken cancellationToken) =>
        passagens.ObterAsync(id, linhaId, cancellationToken);

    /// <summary>Próximas chegadas previstas nas linhas da parada.</summary>
    /// <remarks>
    /// Uma previsão é uma estimativa do provedor (nivelConfirmacao = Previsao), não uma passagem
    /// observada: ela nunca aparece em ultimas-passagens. Por padrão o provedor é simulado (origem
    /// Simulado), com horários fictícios calculados no instante da consulta; nenhuma previsão é
    /// oficial da SPTrans. Previsões vencidas não são devolvidas. Com o provedor indisponível, a
    /// resposta é 503 ProvedorPrevisoesIndisponivel com Retry-After: 30.
    /// </remarks>
    /// <param name="id">Id interno da parada (inteiro ≥ 1).</param>
    /// <param name="linhaId">Opcional: limita a uma linha que atende a parada.</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    [HttpGet("{id:int:min(1)}/previsoes")]
    [EndpointName("ObterPrevisoes")]
    [ProducesResponseType<PrevisoesParadaDto>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable, "application/problem+json")]
    public Task<PrevisoesParadaDto> ObterPrevisoes(int id, [FromQuery] int? linhaId, CancellationToken cancellationToken) =>
        previsoes.ObterAsync(id, linhaId, cancellationToken);
}

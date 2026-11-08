using Microsoft.AspNetCore.Mvc;

using PontoVoz.Api.Application.Consultas;
using PontoVoz.Api.Application.Dtos;
using PontoVoz.Api.Application.Interfaces;

namespace PontoVoz.Api.Controllers;

/// <summary>Busca de destinos (contracts/api-v1.md, seção 4.4).</summary>
[ApiController]
[Route("api/v1/destinos")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, "application/problem+json")]
public sealed class DestinosController(IDestinoService destinos) : ControllerBase
{
    /// <summary>Destinos ativos pelo nome, sem diferenciar maiúsculas e acentos, ordenados pelo nome.</summary>
    /// <param name="termo">Trecho do nome do destino, de 2 a 60 caracteres (por exemplo, "norte").</param>
    /// <param name="pagina">Página, a partir de 1. Padrão: 1.</param>
    /// <param name="tamanhoPagina">Itens por página, de 1 a 50. Padrão: 20.</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    [HttpGet]
    [EndpointName("BuscarDestinos")]
    [ProducesResponseType<PaginaDto<DestinoDto>>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public Task<PaginaDto<DestinoDto>> Buscar([FromQuery] string? termo, [FromQuery] int? pagina, [FromQuery] int? tamanhoPagina, CancellationToken cancellationToken) =>
        destinos.BuscarAsync(new ConsultaPaginada(termo, pagina, tamanhoPagina), cancellationToken);
}

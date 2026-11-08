using Microsoft.AspNetCore.Mvc;

using PontoVoz.Api.Application.Consultas;
using PontoVoz.Api.Application.Dtos;
using PontoVoz.Api.Application.Interfaces;

namespace PontoVoz.Api.Controllers;

/// <summary>Busca de linhas (contracts/api-v1.md, seção 4.3).</summary>
[ApiController]
[Route("api/v1/linhas")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, "application/problem+json")]
public sealed class LinhasController(ILinhaService linhas) : ControllerBase
{
    /// <summary>Linhas ativas, por código, letreiro ou terminais, ordenadas pelo código.</summary>
    /// <param name="termo">Trecho do código, do letreiro ou de um terminal, de 2 a 60 caracteres.</param>
    /// <param name="pagina">Página, a partir de 1. Padrão: 1.</param>
    /// <param name="tamanhoPagina">Itens por página, de 1 a 50. Padrão: 20.</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    [HttpGet]
    [EndpointName("BuscarLinhas")]
    [ProducesResponseType<PaginaDto<LinhaDto>>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public Task<PaginaDto<LinhaDto>> Buscar([FromQuery] string? termo, [FromQuery] int? pagina, [FromQuery] int? tamanhoPagina, CancellationToken cancellationToken) =>
        linhas.BuscarAsync(new ConsultaPaginada(termo, pagina, tamanhoPagina), cancellationToken);
}

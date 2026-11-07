namespace PontoVoz.Api.Application.Consultas;

/// <summary>Parâmetros de <c>GET /api/v1/paradas</c> (contrato, seção 4.1), como chegam da query string.</summary>
public sealed record ConsultaParadas(string? Termo = null, string? CodigoExterno = null, int? Pagina = null, int? TamanhoPagina = null);

/// <summary>Parâmetros de <c>GET /api/v1/linhas</c> e <c>GET /api/v1/destinos</c> (seções 4.3 e 4.4).</summary>
public sealed record ConsultaPaginada(string? Termo = null, int? Pagina = null, int? TamanhoPagina = null);

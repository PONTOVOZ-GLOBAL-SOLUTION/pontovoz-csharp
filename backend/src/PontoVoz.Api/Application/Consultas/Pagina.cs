namespace PontoVoz.Api.Application.Consultas;

/// <summary>
/// Resultado paginado (envelope <c>Pagina&lt;T&gt;</c> do contrato, seção 3.1). Uma página além
/// do total vem com <see cref="Itens"/> vazio, não com erro.
/// </summary>
public sealed record Pagina<T>(IReadOnlyList<T> Itens, int NumeroPagina, int TamanhoPagina, int TotalItens)
{
    /// <summary><c>ceil(totalItens / tamanhoPagina)</c>; 0 quando não há itens.</summary>
    public int TotalPaginas => TotalItens == 0 ? 0 : (int)Math.Ceiling(TotalItens / (double)TamanhoPagina);
}

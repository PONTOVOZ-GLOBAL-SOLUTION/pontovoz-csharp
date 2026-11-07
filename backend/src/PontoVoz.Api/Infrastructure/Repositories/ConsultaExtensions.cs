using Microsoft.EntityFrameworkCore;

using PontoVoz.Api.Application.Consultas;
using PontoVoz.Api.Infrastructure.Persistence.Busca;

namespace PontoVoz.Api.Infrastructure.Repositories;

internal static class ConsultaExtensions
{
    /// <summary>
    /// Filtra pela coluna <c>TextoBusca</c> com o termo normalizado (<c>LIKE '%termo%'</c> no SQL).
    /// Termo vazio ou só com espaços não filtra, como define o contrato (seção 1.6).
    /// </summary>
    public static IQueryable<T> ContendoTexto<T>(this IQueryable<T> consulta, string? termo)
        where T : class
    {
        string normalizado = TextoBusca.Normalizar(termo);
        if (normalizado.Length == 0)
        {
            return consulta;
        }

        string padrao = TextoBusca.PadraoContem(normalizado);
        return consulta.Where(entidade =>
            EF.Functions.Like(EF.Property<string>(entidade, TextoBusca.Coluna), padrao, TextoBusca.CaractereEscape));
    }

    /// <summary>
    /// Conta e pagina no banco (<c>COUNT</c>, depois <c>LIMIT</c>/<c>OFFSET</c>). A consulta já deve
    /// estar ordenada com um critério final de desempate.
    /// </summary>
    public static async Task<Pagina<T>> PaginarAsync<T>(this IOrderedQueryable<T> consulta, Paginacao paginacao, CancellationToken cancellationToken)
    {
        int total = await consulta.CountAsync(cancellationToken);
        List<T> itens = await consulta
            .Skip(paginacao.Deslocamento)
            .Take(paginacao.TamanhoPagina)
            .ToListAsync(cancellationToken);
        return new Pagina<T>(itens, paginacao.Pagina, paginacao.TamanhoPagina, total);
    }
}

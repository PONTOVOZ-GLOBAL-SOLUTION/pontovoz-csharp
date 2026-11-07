using Microsoft.EntityFrameworkCore;

using PontoVoz.Api.Application.Consultas;
using PontoVoz.Api.Application.Interfaces;
using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Infrastructure.Persistence;

namespace PontoVoz.Api.Infrastructure.Repositories;

public sealed class ParadaRepository(PontoVozDbContext contexto) : IParadaRepository
{
    public async Task<Pagina<Parada>> BuscarAtivasAsync(string? termo, string? codigoExterno, Paginacao paginacao, CancellationToken cancellationToken)
    {
        IQueryable<Parada> consulta = contexto.Paradas
            .AsNoTracking()
            .Where(parada => parada.Ativa)
            .ContendoTexto(termo);

        if (!string.IsNullOrWhiteSpace(codigoExterno))
        {
            // Os códigos são gravados em maiúsculas (formato do domínio); a comparação usa o índice único.
            string codigo = codigoExterno.Trim().ToUpperInvariant();
            consulta = consulta.Where(parada => parada.CodigoExterno == codigo);
        }

        return await consulta
            .OrderBy(parada => parada.Nome)
            .ThenBy(parada => parada.Id)
            .PaginarAsync(paginacao, cancellationToken);
    }

    public Task<Parada?> ObterAtivaAsync(int id, CancellationToken cancellationToken) =>
        contexto.Paradas
            .AsNoTracking()
            .FirstOrDefaultAsync(parada => parada.Id == id && parada.Ativa, cancellationToken);
}

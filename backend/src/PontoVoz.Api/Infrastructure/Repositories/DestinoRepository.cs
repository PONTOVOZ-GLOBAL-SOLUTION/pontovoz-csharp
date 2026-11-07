using Microsoft.EntityFrameworkCore;

using PontoVoz.Api.Application.Consultas;
using PontoVoz.Api.Application.Interfaces;
using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Infrastructure.Persistence;

namespace PontoVoz.Api.Infrastructure.Repositories;

public sealed class DestinoRepository(PontoVozDbContext contexto) : IDestinoRepository
{
    public async Task<Pagina<Destino>> BuscarAtivosAsync(string? termo, Paginacao paginacao, CancellationToken cancellationToken) =>
        await contexto.Destinos
            .AsNoTracking()
            .Where(destino => destino.Ativo)
            .ContendoTexto(termo)
            .OrderBy(destino => destino.Nome)
            .ThenBy(destino => destino.Id)
            .PaginarAsync(paginacao, cancellationToken);

    public Task<Destino?> ObterAtivoAsync(int id, CancellationToken cancellationToken) =>
        contexto.Destinos
            .AsNoTracking()
            .FirstOrDefaultAsync(destino => destino.Id == id && destino.Ativo, cancellationToken);

    public async Task<IReadOnlyList<PercursoEstimado>> ListarPercursosDiretosAsync(int paradaId, int destinoId, CancellationToken cancellationToken) =>
        await contexto.PercursosEstimados
            .AsNoTracking()
            .Include(percurso => percurso.Linha)
            .Where(percurso => percurso.ParadaId == paradaId
                && percurso.DestinoId == destinoId
                && percurso.Parada.Ativa
                && percurso.Linha.Ativa
                && percurso.Destino.Ativo)
            .OrderBy(percurso => percurso.MinutosEstimados)
            .ThenBy(percurso => percurso.Linha.Codigo)
            .ToListAsync(cancellationToken);
}

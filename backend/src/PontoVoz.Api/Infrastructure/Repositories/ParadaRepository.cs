using Microsoft.EntityFrameworkCore;

using PontoVoz.Api.Application.Consultas;
using PontoVoz.Api.Application.Interfaces;
using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Infrastructure.Persistence;

namespace PontoVoz.Api.Infrastructure.Repositories;

/// <summary>
/// Consultas públicas de paradas (só ativas, sem rastreamento) e a escrita administrativa do B7
/// (ativas e inativas, rastreadas para o EF Core gravar as mudanças).
/// </summary>
public sealed class ParadaRepository(PontoVozDbContext contexto) : IParadaRepository, ICadastroParadaRepository
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

    public Task<bool> CodigoExternoExisteAsync(string codigoExterno, CancellationToken cancellationToken) =>
        contexto.Paradas.AnyAsync(parada => parada.CodigoExterno == codigoExterno, cancellationToken);

    /// <summary>
    /// Carrega as associações porque o domínio as usa para a idempotência da associação e para
    /// conferir se a linha atende a parada antes de registrar uma passagem.
    /// </summary>
    public Task<Parada?> ObterParaAlteracaoAsync(int id, CancellationToken cancellationToken) =>
        contexto.Paradas
            .Include(parada => parada.LinhasAssociadas)
            .FirstOrDefaultAsync(parada => parada.Id == id, cancellationToken);

    public void Adicionar(Parada parada) => contexto.Paradas.Add(parada);
}

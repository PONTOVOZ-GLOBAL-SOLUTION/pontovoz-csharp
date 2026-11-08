using Microsoft.EntityFrameworkCore;

using PontoVoz.Api.Application.Consultas;
using PontoVoz.Api.Application.Interfaces;
using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Infrastructure.Persistence;

namespace PontoVoz.Api.Infrastructure.Repositories;

/// <summary>Consultas públicas de linhas (só ativas) e a escrita administrativa do B7 (ativas e inativas).</summary>
public sealed class LinhaRepository(PontoVozDbContext contexto) : ILinhaRepository, ICadastroLinhaRepository
{
    public async Task<Pagina<Linha>> BuscarAtivasAsync(string? termo, Paginacao paginacao, CancellationToken cancellationToken) =>
        await contexto.Linhas
            .AsNoTracking()
            .Where(linha => linha.Ativa)
            .ContendoTexto(termo)
            .OrderBy(linha => linha.Codigo)
            .ThenBy(linha => linha.Id)
            .PaginarAsync(paginacao, cancellationToken);

    public Task<Linha?> ObterAtivaAsync(int id, CancellationToken cancellationToken) =>
        contexto.Linhas
            .AsNoTracking()
            .FirstOrDefaultAsync(linha => linha.Id == id && linha.Ativa, cancellationToken);

    public async Task<IReadOnlyList<Linha>> ListarAtivasDaParadaAsync(int paradaId, CancellationToken cancellationToken) =>
        await contexto.ParadasLinhas
            .AsNoTracking()
            .Where(associacao => associacao.ParadaId == paradaId && associacao.Linha.Ativa)
            .Select(associacao => associacao.Linha)
            .OrderBy(linha => linha.Codigo)
            .ThenBy(linha => linha.Id)
            .ToListAsync(cancellationToken);

    public Task<bool> AtendeParadaAsync(int paradaId, int linhaId, CancellationToken cancellationToken) =>
        contexto.ParadasLinhas
            .AsNoTracking()
            .AnyAsync(associacao => associacao.ParadaId == paradaId && associacao.LinhaId == linhaId, cancellationToken);

    public Task<bool> CodigoExisteAsync(string codigo, CancellationToken cancellationToken) =>
        contexto.Linhas.AnyAsync(linha => linha.Codigo == codigo, cancellationToken);

    public Task<Linha?> ObterParaAlteracaoAsync(int id, CancellationToken cancellationToken) =>
        contexto.Linhas.FirstOrDefaultAsync(linha => linha.Id == id, cancellationToken);

    public void Adicionar(Linha linha) => contexto.Linhas.Add(linha);
}

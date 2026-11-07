using Microsoft.EntityFrameworkCore;

using PontoVoz.Api.Application.Interfaces;
using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Infrastructure.Persistence;

namespace PontoVoz.Api.Infrastructure.Repositories;

public sealed class RegistroPassagemRepository(PontoVozDbContext contexto) : IRegistroPassagemRepository
{
    public async Task<IReadOnlyList<RegistroPassagem>> ListarUltimasPorLinhaAsync(int paradaId, DateTimeOffset referencia, int? linhaId, CancellationToken cancellationToken)
    {
        // Registros futuros (aceitos pela tolerância de 2 minutos) ainda não contam como passagem.
        IQueryable<RegistroPassagem> candidatas = contexto.RegistrosPassagem
            .AsNoTracking()
            .Where(passagem => passagem.ParadaId == paradaId
                && passagem.ObservadaEmUtc <= referencia
                && passagem.Linha.Ativa);

        if (linhaId is int id)
        {
            candidatas = candidatas.Where(passagem => passagem.LinhaId == id);
        }

        // Uma passagem por linha, escolhida no SQL: o EF Core traduz para GROUP BY LinhaId com uma
        // subconsulta ORDER BY ObservadaEmUtc DESC, RegistradaEmUtc DESC, Id DESC LIMIT 1.
        // O agrupamento devolve só o Id; a consulta externa carrega a entidade e a linha, no mesmo comando.
        IQueryable<int> idsDasUltimas = candidatas
            .GroupBy(passagem => passagem.LinhaId)
            .Select(grupo => grupo
                .OrderByDescending(passagem => passagem.ObservadaEmUtc)
                .ThenByDescending(passagem => passagem.RegistradaEmUtc)
                .ThenByDescending(passagem => passagem.Id)
                .Select(passagem => passagem.Id)
                .First());

        return await contexto.RegistrosPassagem
            .AsNoTracking()
            .Include(passagem => passagem.Linha)
            .Where(passagem => idsDasUltimas.Contains(passagem.Id))
            .OrderByDescending(passagem => passagem.ObservadaEmUtc)
            .ThenBy(passagem => passagem.Linha.Codigo)
            .ToListAsync(cancellationToken);
    }
}

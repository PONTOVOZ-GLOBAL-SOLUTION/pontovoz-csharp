using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using PontoVoz.Api.Domain.Entities;

namespace PontoVoz.Api.Infrastructure.Persistence.Busca;

/// <summary>
/// Preenche <c>TextoBusca</c> dos registros gravados antes da migration <c>AddSearchColumns</c>,
/// que cria a coluna com o valor vazio. Usa a mesma normalização da gravação, que o SQL de uma
/// migration não conseguiria reproduzir. Roda depois das migrations
/// (<c>dotnet ef database update</c> ou <c>Database.Migrate</c>) e é idempotente: num banco já
/// preenchido, não encontra nada e não grava nada.
/// </summary>
public static class PreenchimentoTextoBusca
{
    /// <summary>Quantidade de registros preenchidos.</summary>
    public static int Executar(PontoVozDbContext contexto)
    {
        ArgumentNullException.ThrowIfNull(contexto);
        var pendentes = new List<object>();
        pendentes.AddRange(contexto.Paradas.Where(Pendente<Parada>()));
        pendentes.AddRange(contexto.Linhas.Where(Pendente<Linha>()));
        pendentes.AddRange(contexto.Destinos.Where(Pendente<Destino>()));
        return Marcar(contexto, pendentes) ? contexto.SaveChanges() : 0;
    }

    /// <inheritdoc cref="Executar"/>
    public static async Task<int> ExecutarAsync(PontoVozDbContext contexto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(contexto);
        var pendentes = new List<object>();
        pendentes.AddRange(await contexto.Paradas.Where(Pendente<Parada>()).ToListAsync(cancellationToken));
        pendentes.AddRange(await contexto.Linhas.Where(Pendente<Linha>()).ToListAsync(cancellationToken));
        pendentes.AddRange(await contexto.Destinos.Where(Pendente<Destino>()).ToListAsync(cancellationToken));
        return Marcar(contexto, pendentes) ? await contexto.SaveChangesAsync(cancellationToken) : 0;
    }

    private static Expression<Func<T, bool>> Pendente<T>()
        where T : class =>
        entidade => EF.Property<string>(entidade, TextoBusca.Coluna) == string.Empty;

    private static bool Marcar(PontoVozDbContext contexto, List<object> pendentes)
    {
        foreach (object entidade in pendentes)
        {
            PontoVozDbContext.DefinirTextoBusca(contexto.Entry(entidade));
        }

        return pendentes.Count > 0;
    }
}

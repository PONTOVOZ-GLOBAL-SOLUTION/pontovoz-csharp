using Microsoft.EntityFrameworkCore;

namespace PontoVoz.Api.Infrastructure.Persistence.Seed;

/// <summary>
/// Provisionamento dos dados de demonstração (contracts/api-v1.md, seção 4.6.2).
/// <list type="number">
/// <item>Banco vazio (nenhuma parada do seed): cria todos os dados de uma vez, com as passagens
/// simuladas calculadas em relação a este instante.</item>
/// <item>Banco já provisionado: não faz nada. Não cria, não renova e não apaga passagens, e não
/// toca nos registros manuais.</item>
/// </list>
/// É chamado pelo EF Core depois de aplicar as migrations (<c>dotnet ef database update</c> ou
/// <c>Database.Migrate</c>), pelos ganchos <c>UseSeeding</c> e <c>UseAsyncSeeding</c>, e nunca
/// por requisição HTTP.
/// </summary>
public sealed class ProvisionamentoDemonstracao(TimeProvider relogio)
{
    /// <summary>Fluxo síncrono, usado pelo <c>dotnet ef database update</c>.</summary>
    public int Executar(DbContext contexto)
    {
        PontoVozDbContext banco = Contexto(contexto);
        if (banco.Paradas.Any(parada => DadosDemonstracao.CodigosParadas.Contains(parada.CodigoExterno)))
        {
            return 0;
        }

        DadosDemonstracao dados = DadosDemonstracao.Criar(relogio.GetUtcNow());
        dados.AdicionarA(banco);
        banco.SaveChanges();
        return dados.TotalRegistros;
    }

    /// <summary>Fluxo assíncrono, usado por <c>Database.MigrateAsync</c>.</summary>
    public async Task<int> ExecutarAsync(DbContext contexto, CancellationToken cancellationToken)
    {
        PontoVozDbContext banco = Contexto(contexto);
        if (await banco.Paradas.AnyAsync(parada => DadosDemonstracao.CodigosParadas.Contains(parada.CodigoExterno), cancellationToken))
        {
            return 0;
        }

        DadosDemonstracao dados = DadosDemonstracao.Criar(relogio.GetUtcNow());
        dados.AdicionarA(banco);
        await banco.SaveChangesAsync(cancellationToken);
        return dados.TotalRegistros;
    }

    private static PontoVozDbContext Contexto(DbContext contexto) =>
        contexto as PontoVozDbContext
        ?? throw new ArgumentException($"O provisionamento exige um {nameof(PontoVozDbContext)}.", nameof(contexto));
}

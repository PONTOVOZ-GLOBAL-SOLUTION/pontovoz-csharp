using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Infrastructure.Persistence.Busca;
using PontoVoz.Api.Infrastructure.Persistence.Conversores;

namespace PontoVoz.Api.Infrastructure.Persistence;

/// <summary>
/// Contexto do EF Core para o SQLite do PontoVoz. Os mapeamentos de cada entidade ficam em
/// <c>Configuracoes/</c>; aqui ficam só as convenções comuns e a manutenção da coluna de busca.
/// </summary>
public sealed class PontoVozDbContext(DbContextOptions<PontoVozDbContext> options) : DbContext(options)
{
    public DbSet<Parada> Paradas => Set<Parada>();

    public DbSet<Linha> Linhas => Set<Linha>();

    public DbSet<ParadaLinha> ParadasLinhas => Set<ParadaLinha>();

    public DbSet<Destino> Destinos => Set<Destino>();

    public DbSet<PercursoEstimado> PercursosEstimados => Set<PercursoEstimado>();

    /// <summary>Passagens simuladas e manuais, numa única tabela (TPH).</summary>
    public DbSet<RegistroPassagem> RegistrosPassagem => Set<RegistroPassagem>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        AtualizarTextosBusca();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        AtualizarTextosBusca();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// Define <c>TextoBusca</c> a partir dos campos originais de cada parada, linha ou destino.
    /// Os campos exibidos não mudam.
    /// </summary>
    internal static void DefinirTextoBusca(EntityEntry entrada)
    {
        string? texto = entrada.Entity switch
        {
            Parada parada => TextoBusca.DeParada(parada),
            Linha linha => TextoBusca.DeLinha(linha),
            Destino destino => TextoBusca.DeDestino(destino),
            _ => null,
        };

        if (texto is not null)
        {
            entrada.Property(TextoBusca.Coluna).CurrentValue = texto;
        }
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTimeOffset>().HaveConversion<InstanteUtcParaTicksConverter>();

        // Enums gravados como texto, com os mesmos nomes do contrato.
        configurationBuilder.Properties<OrigemDado>().HaveConversion<string>().HaveMaxLength(30);
        configurationBuilder.Properties<SentidoLinha>().HaveConversion<string>().HaveMaxLength(30);
        configurationBuilder.Properties<TipoDestino>().HaveConversion<string>().HaveMaxLength(30);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PontoVozDbContext).Assembly);

    private void AtualizarTextosBusca()
    {
        ChangeTracker.DetectChanges();
        foreach (var entrada in ChangeTracker.Entries().Where(e => e.State is EntityState.Added or EntityState.Modified))
        {
            DefinirTextoBusca(entrada);
        }
    }
}

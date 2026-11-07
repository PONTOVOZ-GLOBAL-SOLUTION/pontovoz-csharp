using Microsoft.EntityFrameworkCore;

using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Infrastructure.Persistence.Conversores;

namespace PontoVoz.Api.Infrastructure.Persistence;

/// <summary>
/// Contexto do EF Core para o SQLite do PontoVoz. Os mapeamentos de cada entidade ficam em
/// <c>Configuracoes/</c>; aqui ficam só as convenções comuns.
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
}

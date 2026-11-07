using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PontoVoz.Api.Domain.Entities;

namespace PontoVoz.Api.Infrastructure.Persistence.Configuracoes;

/// <summary>
/// Percurso de uma linha, a partir de uma parada, até um destino. A chave estrangeira composta
/// para <see cref="ParadaLinha"/> garante no banco que a linha atende a parada.
/// </summary>
internal sealed class PercursoEstimadoConfiguration : IEntityTypeConfiguration<PercursoEstimado>
{
    public void Configure(EntityTypeBuilder<PercursoEstimado> builder)
    {
        builder.ToTable("PercursosEstimados", tabela => tabela.HasCheckConstraint(
            "CK_PercursosEstimados_MinutosEstimados",
            $"\"MinutosEstimados\" BETWEEN {PercursoEstimado.MinutosMinimos} AND {PercursoEstimado.MinutosMaximos}"));

        builder.HasKey(percurso => new { percurso.ParadaId, percurso.LinhaId, percurso.DestinoId });

        builder.Property(percurso => percurso.Origem).IsRequired();

        builder.HasOne<ParadaLinha>()
            .WithMany()
            .HasForeignKey(percurso => new { percurso.ParadaId, percurso.LinhaId })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(percurso => percurso.Parada).WithMany().HasForeignKey(percurso => percurso.ParadaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(percurso => percurso.Linha).WithMany().HasForeignKey(percurso => percurso.LinhaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(percurso => percurso.Destino).WithMany().HasForeignKey(percurso => percurso.DestinoId).OnDelete(DeleteBehavior.Restrict);
    }
}

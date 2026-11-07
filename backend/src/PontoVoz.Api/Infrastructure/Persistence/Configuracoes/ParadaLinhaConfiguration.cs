using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PontoVoz.Api.Domain.Entities;

namespace PontoVoz.Api.Infrastructure.Persistence.Configuracoes;

/// <summary>
/// Associação N:N entre parada e linha. Nenhuma exclusão em cascata: cadastros são desativados,
/// não apagados, e as associações sustentam percursos e passagens históricas.
/// </summary>
internal sealed class ParadaLinhaConfiguration : IEntityTypeConfiguration<ParadaLinha>
{
    public void Configure(EntityTypeBuilder<ParadaLinha> builder)
    {
        builder.ToTable("ParadasLinhas");
        builder.HasKey(associacao => new { associacao.ParadaId, associacao.LinhaId });

        builder.HasOne(associacao => associacao.Parada)
            .WithMany(parada => parada.LinhasAssociadas)
            .HasForeignKey(associacao => associacao.ParadaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(associacao => associacao.Linha)
            .WithMany()
            .HasForeignKey(associacao => associacao.LinhaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

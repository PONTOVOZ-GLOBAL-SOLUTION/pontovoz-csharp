using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PontoVoz.Api.Domain.Entities;

namespace PontoVoz.Api.Infrastructure.Persistence.Configuracoes;

internal sealed class LinhaConfiguration : IEntityTypeConfiguration<Linha>
{
    public void Configure(EntityTypeBuilder<Linha> builder)
    {
        builder.ToTable("Linhas");
        builder.HasKey(linha => linha.Id);

        builder.Property(linha => linha.Codigo).HasMaxLength(30).IsRequired();
        builder.HasIndex(linha => linha.Codigo).IsUnique();

        builder.Property(linha => linha.Letreiro).HasMaxLength(30).IsRequired();
        builder.Property(linha => linha.TerminalPrincipal).HasMaxLength(120).IsRequired();
        builder.Property(linha => linha.TerminalSecundario).HasMaxLength(120).IsRequired();
        builder.Property(linha => linha.Sentido).IsRequired();
        builder.Property(linha => linha.Origem).IsRequired();

        builder.Ignore(linha => linha.Persistida);
    }
}

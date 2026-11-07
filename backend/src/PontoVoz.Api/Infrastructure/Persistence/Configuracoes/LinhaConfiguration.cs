using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Infrastructure.Persistence.Busca;

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

        // Texto normalizado para a busca sem acentos (R7). É preenchido pelo PontoVozDbContext ao salvar.
        builder.Property<string>(TextoBusca.Coluna).HasMaxLength(320).IsRequired();
    }
}

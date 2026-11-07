using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Infrastructure.Persistence.Busca;

namespace PontoVoz.Api.Infrastructure.Persistence.Configuracoes;

internal sealed class DestinoConfiguration : IEntityTypeConfiguration<Destino>
{
    public void Configure(EntityTypeBuilder<Destino> builder)
    {
        builder.ToTable("Destinos");
        builder.HasKey(destino => destino.Id);

        // O destino não tem código próprio no contrato; o nome é a chave natural que o seed usa.
        builder.Property(destino => destino.Nome).HasMaxLength(120).IsRequired();
        builder.HasIndex(destino => destino.Nome).IsUnique();

        builder.Property(destino => destino.Tipo).IsRequired();
        builder.Property(destino => destino.Origem).IsRequired();

        builder.Ignore(destino => destino.Persistida);

        // Texto normalizado para a busca sem acentos (R7). É preenchido pelo PontoVozDbContext ao salvar.
        builder.Property<string>(TextoBusca.Coluna).HasMaxLength(130).IsRequired();
    }
}

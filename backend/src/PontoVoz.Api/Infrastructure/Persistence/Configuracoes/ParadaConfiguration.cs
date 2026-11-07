using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Infrastructure.Persistence.Busca;

namespace PontoVoz.Api.Infrastructure.Persistence.Configuracoes;

internal sealed class ParadaConfiguration : IEntityTypeConfiguration<Parada>
{
    public void Configure(EntityTypeBuilder<Parada> builder)
    {
        builder.ToTable("Paradas");
        builder.HasKey(parada => parada.Id);

        builder.Property(parada => parada.CodigoExterno).HasMaxLength(30).IsRequired();
        builder.HasIndex(parada => parada.CodigoExterno).IsUnique();

        builder.Property(parada => parada.Nome).HasMaxLength(120).IsRequired();
        builder.Property(parada => parada.Endereco).HasMaxLength(200);
        builder.Property(parada => parada.Origem).IsRequired();

        // Objeto de valor opcional: duas colunas anuláveis na própria tabela de paradas.
        builder.ComplexProperty(parada => parada.Coordenada, coordenada =>
        {
            coordenada.Property(c => c.Latitude).HasColumnName("Latitude");
            coordenada.Property(c => c.Longitude).HasColumnName("Longitude");
        });

        builder.Ignore(parada => parada.Persistida);

        // Texto normalizado para a busca sem acentos (R7). É preenchido pelo PontoVozDbContext ao salvar.
        builder.Property<string>(TextoBusca.Coluna).HasMaxLength(160).IsRequired();

        // A coleção é exposta só para leitura; o EF Core usa o campo privado.
        builder.Navigation(parada => parada.LinhasAssociadas)
            .HasField("_linhasAssociadas")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

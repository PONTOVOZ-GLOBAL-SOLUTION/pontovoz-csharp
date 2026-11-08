using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PontoVoz.Api.Domain.Entities;

namespace PontoVoz.Api.Infrastructure.Persistence.Configuracoes;

/// <summary>
/// Hierarquia de passagens em uma única tabela (TPH), identificada pela coluna
/// <c>TipoPassagem</c>. Nível de confirmação, aviso e verificação são calculados pelo tipo
/// concreto e não são gravados.
/// </summary>
internal sealed class RegistroPassagemConfiguration : IEntityTypeConfiguration<RegistroPassagem>
{
    public const string ColunaDiscriminador = "TipoPassagem";
    public const string TipoSimulada = "Simulada";
    public const string TipoRegistroManual = "RegistroManual";

    public void Configure(EntityTypeBuilder<RegistroPassagem> builder)
    {
        builder.ToTable("RegistrosPassagem");
        builder.HasKey(passagem => passagem.Id);

        builder.HasDiscriminator<string>(ColunaDiscriminador)
            .HasValue<PassagemSimulada>(TipoSimulada)
            .HasValue<PassagemRegistroManual>(TipoRegistroManual);
        builder.Property<string>(ColunaDiscriminador).HasMaxLength(20);

        builder.Property(passagem => passagem.ObservadaEmUtc).IsRequired();
        builder.Property(passagem => passagem.RegistradaEmUtc).IsRequired();
        builder.Property(passagem => passagem.PrefixoVeiculo).HasMaxLength(20);

        builder.Ignore(passagem => passagem.NivelConfirmacao);
        builder.Ignore(passagem => passagem.CodigoAviso);
        builder.Ignore(passagem => passagem.Verificada);
        builder.Ignore(passagem => passagem.OficialSptrans);
        builder.Ignore(passagem => passagem.DadosSimulados);
        builder.Ignore(passagem => passagem.OrigemRegistro);
        builder.Ignore(passagem => passagem.Persistida);

        // A passagem só existe para uma linha que atende a parada. Restrict preserva o histórico.
        builder.HasOne<ParadaLinha>()
            .WithMany()
            .HasForeignKey(passagem => new { passagem.ParadaId, passagem.LinhaId })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(passagem => passagem.Parada).WithMany().HasForeignKey(passagem => passagem.ParadaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(passagem => passagem.Linha).WithMany().HasForeignKey(passagem => passagem.LinhaId).OnDelete(DeleteBehavior.Restrict);

        // Consulta da última passagem por linha de uma parada.
        builder.HasIndex(passagem => new { passagem.ParadaId, passagem.LinhaId, passagem.ObservadaEmUtc });
    }
}

internal sealed class PassagemSimuladaConfiguration : IEntityTypeConfiguration<PassagemSimulada>
{
    public void Configure(EntityTypeBuilder<PassagemSimulada> builder)
    {
        builder.Property(passagem => passagem.ChaveSeed).HasMaxLength(40).IsRequired();

        // Só as linhas simuladas têm chave; o filtro deixa as manuais (NULL) fora do índice.
        builder.HasIndex(passagem => passagem.ChaveSeed)
            .IsUnique()
            .HasFilter("\"ChaveSeed\" IS NOT NULL");
    }
}

internal sealed class PassagemRegistroManualConfiguration : IEntityTypeConfiguration<PassagemRegistroManual>
{
    public void Configure(EntityTypeBuilder<PassagemRegistroManual> builder)
    {
        // Campos privados que sustentam as propriedades sobrescritas do registro manual.
        builder.Property<string>("_origemRegistro").HasColumnName("OrigemRegistro").HasMaxLength(40).IsRequired();
        builder.Property<bool>("_dadosSimulados").HasColumnName("DadosSimulados").IsRequired();
    }
}

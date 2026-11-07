using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.Rules;

namespace PontoVoz.Api.Domain.ValueObjects;

/// <summary>
/// Chegada prevista de um ônibus. É uma estimativa do provedor, nunca uma passagem confirmada,
/// por isso o nível de confirmação é sempre <see cref="Enums.NivelConfirmacao.Previsao"/>.
/// </summary>
public sealed record PrevisaoChegada
{
    public PrevisaoChegada(ReferenciaLinha linha, DateTimeOffset previstaPara, string? prefixoVeiculo)
    {
        ArgumentNullException.ThrowIfNull(linha);
        Linha = linha;
        PrevistaParaUtc = RegrasCadastro.Utc(previstaPara, "previstaPara");
        PrefixoVeiculo = RegrasCadastro.PrefixoVeiculo(prefixoVeiculo);
    }

    public ReferenciaLinha Linha { get; }

    public DateTimeOffset PrevistaParaUtc { get; }

    public string? PrefixoVeiculo { get; }

    public NivelConfirmacao NivelConfirmacao => NivelConfirmacao.Previsao;

    /// <summary>
    /// <c>true</c> quando o horário previsto já passou. Uma previsão exatamente em
    /// <paramref name="referencia"/> não está vencida: é um ônibus chegando, com espera 0.
    /// </summary>
    public bool VencidaEm(DateTimeOffset referencia) => PrevistaParaUtc < referencia;

    /// <summary>
    /// Minutos inteiros até a chegada, arredondados para baixo. <c>0</c> significa
    /// "chegando, em menos de 1 minuto"; nunca é negativo.
    /// </summary>
    public int EsperaMinutos(DateTimeOffset referencia)
    {
        TimeSpan espera = PrevistaParaUtc - referencia;
        return espera <= TimeSpan.Zero ? 0 : (int)Math.Floor(espera.TotalMinutes);
    }
}

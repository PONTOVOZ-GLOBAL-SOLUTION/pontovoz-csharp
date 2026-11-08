using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.Exceptions;

namespace PontoVoz.Api.Domain.ValueObjects;

/// <summary>
/// Resultado calculado para uma linha direta até um destino (<c>Alternativa</c> no contrato).
/// Não é persistido. A ordenação e a posição de cada item ficam com o serviço de alternativas.
/// </summary>
public sealed record AlternativaDeslocamento
{
    private AlternativaDeslocamento(ReferenciaLinha linha, int percursoEstimadoMinutos, OrigemDado origemPercurso, PrevisaoChegada? proximaChegada, DateTimeOffset referencia)
    {
        Linha = linha;
        PercursoEstimadoMinutos = percursoEstimadoMinutos;
        OrigemPercurso = origemPercurso;

        // Uma previsão vencida na referência do cálculo não é uma chegada válida.
        if (proximaChegada is null || proximaChegada.VencidaEm(referencia))
        {
            Situacao = SituacaoAlternativa.SemPrevisao;
            return;
        }

        Situacao = SituacaoAlternativa.Comparavel;
        PrevistaParaUtc = proximaChegada.PrevistaParaUtc;
        EsperaMinutos = proximaChegada.EsperaMinutos(referencia);
        TempoTotalEstimadoMinutos = EsperaMinutos + percursoEstimadoMinutos;
        ChegadaEstimadaAoDestinoUtc = proximaChegada.PrevistaParaUtc.AddMinutes(percursoEstimadoMinutos);
    }

    public ReferenciaLinha Linha { get; }

    public SituacaoAlternativa Situacao { get; }

    public DateTimeOffset? PrevistaParaUtc { get; }

    public int? EsperaMinutos { get; }

    public int PercursoEstimadoMinutos { get; }

    public int? TempoTotalEstimadoMinutos { get; }

    public DateTimeOffset? ChegadaEstimadaAoDestinoUtc { get; }

    /// <summary>Só trajetos diretos no MVP.</summary>
    public int Baldeacoes => 0;

    public OrigemDado OrigemPercurso { get; }

    /// <summary>
    /// Combina o percurso cadastrado com a próxima chegada da linha. Sem previsão, ou com uma
    /// previsão já vencida em <paramref name="referencia"/>, a alternativa fica
    /// <see cref="SituacaoAlternativa.SemPrevisao"/>, com tempos nulos, e não pode ser
    /// apresentada como a mais rápida.
    /// </summary>
    public static AlternativaDeslocamento Calcular(PercursoEstimado percurso, PrevisaoChegada? proximaChegada, DateTimeOffset referencia)
    {
        ArgumentNullException.ThrowIfNull(percurso);
        ReferenciaLinha linha = ReferenciaLinha.De(percurso.Linha);
        if (proximaChegada is not null && proximaChegada.Linha.LinhaId != linha.LinhaId)
        {
            throw new ValidacaoDominioException("linhaId", "A previsão informada é de outra linha.");
        }

        return new AlternativaDeslocamento(linha, percurso.MinutosEstimados, percurso.Origem, proximaChegada, referencia);
    }
}

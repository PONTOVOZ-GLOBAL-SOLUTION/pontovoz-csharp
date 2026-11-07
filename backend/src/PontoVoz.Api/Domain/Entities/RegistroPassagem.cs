using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.Rules;

namespace PontoVoz.Api.Domain.Entities;

/// <summary>
/// Registro de que um ônibus de uma linha passou pela parada. É histórico: os horários são
/// definidos na criação e não mudam. Cada subclasse define o próprio nível de confirmação, o
/// aviso que acompanha o registro e a natureza do dado.
/// </summary>
public abstract class RegistroPassagem : EntidadeBase
{
    /// <summary>Usado pelo EF Core ao materializar a entidade.</summary>
    protected RegistroPassagem()
    {
        Parada = null!;
        Linha = null!;
    }

    protected RegistroPassagem(ParadaLinha associacao, DateTimeOffset observadaEm, DateTimeOffset registradaEm, string? prefixoVeiculo)
        : base(registradaEm)
    {
        ArgumentNullException.ThrowIfNull(associacao);
        Parada = associacao.Parada;
        ParadaId = associacao.ParadaId;
        Linha = associacao.Linha;
        LinhaId = associacao.LinhaId;
        ObservadaEmUtc = RegrasCadastro.Utc(observadaEm, "observadaEm");
        RegistradaEmUtc = RegrasCadastro.Utc(registradaEm, "registradaEm");
        PrefixoVeiculo = RegrasCadastro.PrefixoVeiculo(prefixoVeiculo);
    }

    public int ParadaId { get; private set; }

    public Parada Parada { get; private set; }

    public int LinhaId { get; private set; }

    public Linha Linha { get; private set; }

    /// <summary>Quando o ônibus foi visto.</summary>
    public DateTimeOffset ObservadaEmUtc { get; private set; }

    /// <summary>Quando o registro entrou no sistema.</summary>
    public DateTimeOffset RegistradaEmUtc { get; private set; }

    public string? PrefixoVeiculo { get; private set; }

    public abstract NivelConfirmacao NivelConfirmacao { get; }

    /// <summary>Código do aviso que explica o nível de confirmação (seção 8 do contrato).</summary>
    public abstract string CodigoAviso { get; }

    /// <summary>Se o registro, a linha ou a parada são fictícios.</summary>
    public abstract bool DadosSimulados { get; }

    /// <summary>Rótulo técnico de quem registrou; <c>null</c> quando o registro não é manual.</summary>
    public virtual string? OrigemRegistro => null;

    /// <summary>
    /// Só uma confirmação por fonte confiável tornaria o registro verificado. Nenhuma subclasse
    /// do MVP tem esse nível, então o valor é sempre <c>false</c>.
    /// </summary>
    public bool Verificada => NivelConfirmacao == NivelConfirmacao.VerificadaFonteConfiavel;

    /// <summary>Não há integração oficial de passagens no MVP.</summary>
    public bool OficialSptrans => false;

    public int MinutosDesdeObservacao(DateTimeOffset referencia) =>
        RegraTemporalPassagem.MinutosDesdeObservacao(ObservadaEmUtc, referencia);

    public bool EstaDesatualizada(DateTimeOffset referencia) =>
        RegraTemporalPassagem.EstaDesatualizada(ObservadaEmUtc, referencia);

    /// <summary><c>false</c> enquanto o horário observado ainda está no futuro.</summary>
    public bool JaOcorreu(DateTimeOffset referencia) =>
        RegraTemporalPassagem.JaOcorreu(ObservadaEmUtc, referencia);
}

using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.Exceptions;
using PontoVoz.Api.Domain.Rules;

namespace PontoVoz.Api.Domain.Entities;

/// <summary>
/// Observação informada por um operador (<c>POST /admin/observacoes-passagem</c>).
/// Não é verificada nem oficial. É o único jeito de criar uma passagem depois do provisionamento.
/// </summary>
public sealed class PassagemRegistroManual : RegistroPassagem
{
    public const string CodigoAvisoRegistroManual = "RegistroManualNaoVerificado";

    /// <summary>Mensagem do 400 quando <c>observadaEm</c> está fora da janela de registro (seção 4.6.3).</summary>
    public const string MensagemForaDaJanela = "A observação deve estar entre 24 horas antes e 2 minutos depois do horário do servidor.";

    private string _origemRegistro;
    private bool _dadosSimulados;

    private PassagemRegistroManual()
    {
        _origemRegistro = string.Empty;
    }

    private PassagemRegistroManual(ParadaLinha associacao, DateTimeOffset observadaEm, DateTimeOffset registradaEm, string? prefixoVeiculo, string origemRegistro, bool dadosSimulados)
        : base(associacao, observadaEm, registradaEm, prefixoVeiculo)
    {
        _origemRegistro = origemRegistro;
        _dadosSimulados = dadosSimulados;
    }

    public override string OrigemRegistro => _origemRegistro;

    public override NivelConfirmacao NivelConfirmacao => NivelConfirmacao.RegistroManual;

    public override string CodigoAviso => CodigoAvisoRegistroManual;

    /// <summary>Herdado da parada e da linha no momento do registro; as duas não mudam de natureza.</summary>
    public override bool DadosSimulados => _dadosSimulados;

    /// <summary>
    /// Registra a observação. Primeiro valida os dados (400): rótulo, prefixo do veículo
    /// (com <c>SIM-</c> quando a parada ou a linha é fictícia, e sem ele quando as duas são reais) e a janela de
    /// 24 horas antes a 2 minutos depois de <paramref name="instanteRegistro"/>. Depois, o estado
    /// (422): parada e linha ativas e linha que atende a parada.
    /// </summary>
    public static PassagemRegistroManual Registrar(Parada parada, Linha linha, DateTimeOffset observadaEm, string? prefixoVeiculo, string origemRegistro, DateTimeOffset instanteRegistro)
    {
        ArgumentNullException.ThrowIfNull(parada);
        ArgumentNullException.ThrowIfNull(linha);

        bool dadosSimulados = parada.DadosSimulados || linha.DadosSimulados;
        string origemValida = RegrasCadastro.OrigemRegistro(origemRegistro);
        RegrasCadastro.PrefixoVeiculo(prefixoVeiculo, dadosSimulados);
        DateTimeOffset observada = RegrasCadastro.Utc(observadaEm, "observadaEm");
        DateTimeOffset registro = RegrasCadastro.Utc(instanteRegistro, "registradaEm");

        if (!RegraTemporalPassagem.ObservacaoAceitaNoRegistro(observada, registro))
        {
            throw new ValidacaoDominioException("observadaEm", MensagemForaDaJanela);
        }

        if (!parada.Ativa)
        {
            throw new RegraNegocioException(RegraNegocioException.ParadaInativa, "A parada está inativa.");
        }

        if (!linha.Ativa)
        {
            throw new RegraNegocioException(RegraNegocioException.LinhaInativa, "A linha está inativa.");
        }

        ParadaLinha associacao = parada.ObterAssociacao(linha);
        return new PassagemRegistroManual(associacao, observada, registro, prefixoVeiculo, origemValida, dadosSimulados);
    }
}

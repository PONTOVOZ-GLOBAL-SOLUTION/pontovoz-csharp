using System.Text.RegularExpressions;

using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.Exceptions;
using PontoVoz.Api.Domain.Rules;

namespace PontoVoz.Api.Domain.Entities;

/// <summary>
/// Observação de demonstração criada uma única vez, no provisionamento inicial de um banco vazio
/// (contracts/api-v1.md, 4.6.2). O horário é calculado nesse momento e não é renovado depois.
/// <see cref="ChaveSeed"/> permite ao seed conferir se ela já existe; não aparece nos DTOs públicos.
/// </summary>
public sealed partial class PassagemSimulada : RegistroPassagem
{
    public const string CodigoAvisoObservacaoSimulada = "ObservacaoSimulada";

    private PassagemSimulada()
    {
        ChaveSeed = string.Empty;
    }

    private PassagemSimulada(ParadaLinha associacao, string chaveSeed, DateTimeOffset observadaEm, DateTimeOffset instanteProvisionamento, string? prefixoVeiculo)
        : base(associacao, observadaEm, instanteProvisionamento, prefixoVeiculo)
    {
        ChaveSeed = chaveSeed;
    }

    public string ChaveSeed { get; private set; }

    public override NivelConfirmacao NivelConfirmacao => NivelConfirmacao.ObservacaoSimulada;

    public override string CodigoAviso => CodigoAvisoObservacaoSimulada;

    public override bool DadosSimulados => true;

    [GeneratedRegex(@"\ASIM-[A-Z0-9-]{3,36}\z")]
    private static partial Regex FormatoChaveSeed();

    /// <summary>
    /// Cria a observação no provisionamento: <c>observadaEm = instanteProvisionamento − antecedencia</c>
    /// e <c>registradaEm = instanteProvisionamento</c>.
    /// </summary>
    public static PassagemSimulada CriarNoProvisionamento(ParadaLinha associacao, string chaveSeed, TimeSpan antecedencia, DateTimeOffset instanteProvisionamento, string? prefixoVeiculo)
    {
        ArgumentNullException.ThrowIfNull(associacao);

        if (chaveSeed is null || !FormatoChaveSeed().IsMatch(chaveSeed))
        {
            throw new ValidacaoDominioException("chaveSeed", "A chave do seed deve começar com SIM- e ter até 40 caracteres entre letras maiúsculas, dígitos e hífen.");
        }

        if (antecedencia < TimeSpan.Zero || antecedencia > TimeSpan.FromHours(24))
        {
            throw new ValidacaoDominioException("observadaEm", "A observação simulada deve ficar entre o instante do provisionamento e 24 horas antes dele.");
        }

        RegrasCadastro.PrefixoVeiculo(prefixoVeiculo, dadosSimulados: true);

        return new PassagemSimulada(associacao, chaveSeed, instanteProvisionamento - antecedencia, instanteProvisionamento, prefixoVeiculo);
    }
}

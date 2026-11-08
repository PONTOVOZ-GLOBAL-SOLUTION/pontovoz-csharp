using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.Exceptions;

namespace PontoVoz.Api.Domain.Entities;

/// <summary>
/// Tempo estimado de uma linha, a partir de uma parada que ela atende, até um destino.
/// É criado a partir da associação <see cref="ParadaLinha"/>, então não existe percurso de uma
/// linha que não atende a parada. No banco, a chave é composta por (<see cref="ParadaId"/>,
/// <see cref="LinhaId"/>, <see cref="DestinoId"/>).
/// </summary>
public sealed class PercursoEstimado
{
    public const int MinutosMinimos = 1;
    public const int MinutosMaximos = 240;

    private PercursoEstimado()
    {
        Parada = null!;
        Linha = null!;
        Destino = null!;
    }

    private PercursoEstimado(ParadaLinha associacao, Destino destino, int minutosEstimados)
    {
        if (minutosEstimados is < MinutosMinimos or > MinutosMaximos)
        {
            throw new ValidacaoDominioException("percursoEstimadoMinutos", $"O percurso estimado deve ter entre {MinutosMinimos} e {MinutosMaximos} minutos.");
        }

        Parada = associacao.Parada;
        ParadaId = associacao.ParadaId;
        Linha = associacao.Linha;
        LinhaId = associacao.LinhaId;
        Destino = destino;
        DestinoId = destino.Id;
        MinutosEstimados = minutosEstimados;
        Origem = OrigemDado.Simulado;
        DadosSimulados = true;
    }

    public int ParadaId { get; private set; }

    public Parada Parada { get; private set; }

    public int LinhaId { get; private set; }

    public Linha Linha { get; private set; }

    public int DestinoId { get; private set; }

    public Destino Destino { get; private set; }

    public int MinutosEstimados { get; private set; }

    /// <summary>Sempre <see cref="OrigemDado.Simulado"/> no MVP (<c>origemPercurso</c> no contrato).</summary>
    public OrigemDado Origem { get; private set; }

    public bool DadosSimulados { get; private set; }

    /// <summary>Percurso de demonstração criado pelo seed para uma linha que atende a parada.</summary>
    public static PercursoEstimado CriarSimulado(ParadaLinha associacao, Destino destino, int minutosEstimados)
    {
        ArgumentNullException.ThrowIfNull(associacao);
        ArgumentNullException.ThrowIfNull(destino);
        return new PercursoEstimado(associacao, destino, minutosEstimados);
    }
}

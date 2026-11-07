namespace PontoVoz.Api.Domain.Entities;

/// <summary>
/// Indica que uma linha atende uma parada. Só é criada por <see cref="Parada.AssociarLinha"/>,
/// que impede associações duplicadas. A chave será composta por (<see cref="ParadaId"/>,
/// <see cref="LinhaId"/>) no B3.
/// </summary>
public sealed class ParadaLinha
{
    private ParadaLinha()
    {
        Parada = null!;
        Linha = null!;
    }

    internal ParadaLinha(Parada parada, Linha linha, DateTimeOffset associadaEmUtc)
    {
        Parada = parada;
        ParadaId = parada.Id;
        Linha = linha;
        LinhaId = linha.Id;
        AssociadaEmUtc = associadaEmUtc;
    }

    public int ParadaId { get; private set; }

    public Parada Parada { get; private set; }

    public int LinhaId { get; private set; }

    public Linha Linha { get; private set; }

    public DateTimeOffset AssociadaEmUtc { get; private set; }

    /// <summary>
    /// Compara pelo <see cref="LinhaId"/> quando a linha já foi persistida, para funcionar mesmo
    /// sem a navegação carregada. Antes da persistência, compara pela instância ou pelo código,
    /// que é a chave natural e única da linha.
    /// </summary>
    public bool Envolve(Linha linha) =>
        linha.Persistida && LinhaId > 0
            ? LinhaId == linha.Id
            : ReferenceEquals(Linha, linha) || string.Equals(Linha?.Codigo, linha.Codigo, StringComparison.Ordinal);
}

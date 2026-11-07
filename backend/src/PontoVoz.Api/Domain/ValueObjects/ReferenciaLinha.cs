using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Exceptions;

namespace PontoVoz.Api.Domain.ValueObjects;

/// <summary>
/// Forma curta de uma linha (<c>ReferenciaLinha</c> no contrato), usada em previsões e
/// alternativas sem carregar a entidade inteira.
/// </summary>
public sealed record ReferenciaLinha
{
    public ReferenciaLinha(int linhaId, string codigoLinha, string letreiro)
    {
        if (linhaId < 1)
        {
            throw new ValidacaoDominioException("linhaId", "A referência de linha exige uma linha já persistida.");
        }

        if (string.IsNullOrWhiteSpace(codigoLinha) || string.IsNullOrWhiteSpace(letreiro))
        {
            throw new ValidacaoDominioException("codigoLinha", "A referência de linha exige código e letreiro.");
        }

        LinhaId = linhaId;
        CodigoLinha = codigoLinha;
        Letreiro = letreiro;
    }

    public int LinhaId { get; }

    public string CodigoLinha { get; }

    public string Letreiro { get; }

    public static ReferenciaLinha De(Linha linha) => new(linha.Id, linha.Codigo, linha.Letreiro);
}

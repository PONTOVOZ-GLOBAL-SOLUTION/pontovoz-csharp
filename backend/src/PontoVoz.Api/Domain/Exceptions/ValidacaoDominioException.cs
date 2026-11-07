namespace PontoVoz.Api.Domain.Exceptions;

/// <summary>
/// Um valor informado não respeita uma regra de formato ou de faixa (HTTP 400,
/// <c>RequisicaoInvalida</c>). <see cref="Campo"/> é o nome do campo no contrato, em camelCase.
/// </summary>
public sealed class ValidacaoDominioException(string campo, string mensagem)
    : DominioException(CodigoRequisicaoInvalida, mensagem)
{
    public const string CodigoRequisicaoInvalida = "RequisicaoInvalida";

    public string Campo { get; } = campo;
}

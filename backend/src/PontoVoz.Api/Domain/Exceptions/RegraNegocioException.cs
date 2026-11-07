namespace PontoVoz.Api.Domain.Exceptions;

/// <summary>
/// Os dados são válidos, mas o estado atual impede a operação (HTTP 422).
/// </summary>
public sealed class RegraNegocioException : DominioException
{
    public const string ParadaInativa = "ParadaInativa";
    public const string LinhaInativa = "LinhaInativa";
    public const string LinhaNaoAtendeParada = "LinhaNaoAtendeParada";

    public RegraNegocioException(string codigo, string mensagem)
        : base(codigo, mensagem)
    {
    }
}

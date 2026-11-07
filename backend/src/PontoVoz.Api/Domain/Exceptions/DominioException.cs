namespace PontoVoz.Api.Domain.Exceptions;

/// <summary>
/// Base das exceções lançadas pelo domínio. <see cref="Codigo"/> usa os códigos do catálogo
/// de erros do contrato (contracts/api-v1.md, seção 7), para o tratador global do B6
/// convertê-los em ProblemDetails sem conhecer cada regra.
/// </summary>
public abstract class DominioException(string codigo, string mensagem) : Exception(mensagem)
{
    public string Codigo { get; } = codigo;
}

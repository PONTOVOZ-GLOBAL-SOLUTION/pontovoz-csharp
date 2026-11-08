namespace PontoVoz.Api.Application.Erros;

/// <summary>
/// O banco recusou a gravação por uma chave única ou primária já existente, por exemplo quando
/// duas requisições cadastram o mesmo código ao mesmo tempo. O service decide o significado:
/// 409 <c>CodigoJaCadastrado</c> num cadastro, ou sucesso numa associação idempotente. A mensagem
/// é genérica; o erro original fica em <see cref="Exception.InnerException"/>, só para o log.
/// </summary>
public sealed class ViolacaoUnicidadeException(Exception erroOriginal)
    : Exception("O banco recusou um valor que já existe numa chave única.", erroOriginal);

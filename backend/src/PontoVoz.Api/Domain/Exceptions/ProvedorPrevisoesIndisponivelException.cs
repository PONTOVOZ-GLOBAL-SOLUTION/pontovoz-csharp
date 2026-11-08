namespace PontoVoz.Api.Domain.Exceptions;

/// <summary>
/// O provedor de previsões falhou, não respondeu no prazo ou está desligado (contrato, seções
/// 1.10 e 4.5; divergência D2). Só o endpoint de previsões a lança: lá a previsão é todo o
/// conteúdo, e o tratador global a converte em 503 com <c>Retry-After</c>. O resumo e as
/// alternativas (B9) tratam a mesma situação como status, sem exceção.
/// </summary>
public sealed class ProvedorPrevisoesIndisponivelException()
    : DominioException(CodigoProvedorIndisponivel, "O serviço de previsões não está disponível no momento. Tente novamente em instantes.")
{
    public const string CodigoProvedorIndisponivel = "ProvedorPrevisoesIndisponivel";

    /// <summary>Segundos sugeridos no cabeçalho <c>Retry-After</c> (seção 4.5).</summary>
    public const int SegundosParaNovaTentativa = 30;
}

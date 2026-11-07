namespace PontoVoz.Api.Domain.Rules;

/// <summary>
/// Regra temporal das observações de passagem (contracts/api-v1.md, seção 4.6.3).
/// Todos os cálculos usam o intervalo exato <c>referência − observadaEm</c>, sem arredondar
/// os instantes. A referência é sempre informada por quem chama (vinda do <see cref="TimeProvider"/>),
/// nunca lida do relógio aqui dentro.
/// </summary>
public static class RegraTemporalPassagem
{
    /// <summary>Quanto uma observação manual pode estar no futuro, para tolerar diferenças de relógio.</summary>
    public static readonly TimeSpan ToleranciaFuturo = TimeSpan.FromMinutes(2);

    /// <summary>Quanto uma observação manual pode estar no passado no momento do registro.</summary>
    public static readonly TimeSpan JanelaPassado = TimeSpan.FromHours(24);

    /// <summary>Acima deste intervalo, a passagem é marcada como desatualizada.</summary>
    public static readonly TimeSpan LimiteDesatualizacao = TimeSpan.FromMinutes(60);

    /// <summary>Valor de <c>limiteDesatualizacaoMinutos</c> nas respostas.</summary>
    public const int LimiteDesatualizacaoMinutos = 60;

    /// <summary>
    /// Indica se uma observação pode ser registrada agora: o intervalo deve estar entre
    /// −2 minutos e +24 horas, inclusive.
    /// </summary>
    public static bool ObservacaoAceitaNoRegistro(DateTimeOffset observadaEm, DateTimeOffset instanteRegistro)
    {
        TimeSpan intervalo = instanteRegistro - observadaEm;
        return intervalo >= -ToleranciaFuturo && intervalo <= JanelaPassado;
    }

    /// <summary>Minutos inteiros transcorridos, arredondados para baixo e nunca negativos.</summary>
    public static int MinutosDesdeObservacao(DateTimeOffset observadaEm, DateTimeOffset referencia)
    {
        TimeSpan intervalo = referencia - observadaEm;
        return intervalo <= TimeSpan.Zero ? 0 : (int)Math.Floor(intervalo.TotalMinutes);
    }

    /// <summary>
    /// <c>true</c> quando o intervalo exato passa de 60 minutos. Com 60 min e 1 s, os minutos
    /// exibidos ainda são 60, mas a passagem já é desatualizada.
    /// </summary>
    public static bool EstaDesatualizada(DateTimeOffset observadaEm, DateTimeOffset referencia) =>
        referencia - observadaEm > LimiteDesatualizacao;

    /// <summary>
    /// Uma observação só conta como passagem ocorrida quando o horário dela já chegou.
    /// Registros aceitos pela tolerância, ainda no futuro, ficam fora das consultas até lá.
    /// </summary>
    public static bool JaOcorreu(DateTimeOffset observadaEm, DateTimeOffset referencia) =>
        observadaEm <= referencia;
}

namespace PontoVoz.Api.Application.Mapeamentos;

/// <summary>
/// Converte instantes UTC do domínio para o horário de São Paulo, usado na saída dos endpoints
/// de negócio (contracts/api-v1.md, seção 1.3). O instante não muda; só o deslocamento exibido.
/// </summary>
public static class HorarioSaoPaulo
{
    private static readonly TimeZoneInfo Fuso = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public static DateTimeOffset Converter(DateTimeOffset instante) => TimeZoneInfo.ConvertTime(instante, Fuso);
}

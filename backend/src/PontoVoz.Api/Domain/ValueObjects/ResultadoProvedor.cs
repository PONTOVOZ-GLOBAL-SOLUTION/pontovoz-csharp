using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.Exceptions;
using PontoVoz.Api.Domain.Rules;

namespace PontoVoz.Api.Domain.ValueObjects;

/// <summary>
/// Resultado de uma consulta ao provedor de previsões. O status é derivado do conteúdo,
/// para que um resultado <see cref="StatusPrevisao.Disponivel"/> sempre tenha previsões e um
/// resultado indisponível nunca as tenha.
/// </summary>
public sealed record ResultadoProvedor
{
    private ResultadoProvedor(StatusPrevisao status, OrigemDado origem, DateTimeOffset consultadoEm, IReadOnlyList<PrevisaoChegada> previsoes)
    {
        if (origem is not (OrigemDado.Simulado or OrigemDado.OlhoVivo))
        {
            throw new ValidacaoDominioException("origem", "Previsões vêm apenas do provedor simulado ou da Olho Vivo.");
        }

        Status = status;
        Origem = origem;
        ConsultadoEmUtc = RegrasCadastro.Utc(consultadoEm, "consultadoEm");
        Previsoes = previsoes;
    }

    public StatusPrevisao Status { get; }

    public OrigemDado Origem { get; }

    public DateTimeOffset ConsultadoEmUtc { get; }

    public IReadOnlyList<PrevisaoChegada> Previsoes { get; }

    public bool ProvedorDisponivel => Status != StatusPrevisao.ProvedorIndisponivel;

    /// <summary>
    /// O provedor respondeu. Previsões vencidas (antes de <paramref name="consultadoEm"/>) são
    /// descartadas, porque o contrato não as devolve. O status é calculado depois do descarte:
    /// <c>Disponivel</c> com ao menos uma previsão válida, <c>SemPrevisoes</c> sem nenhuma.
    /// </summary>
    public static ResultadoProvedor Respondido(OrigemDado origem, DateTimeOffset consultadoEm, IEnumerable<PrevisaoChegada> previsoes)
    {
        ArgumentNullException.ThrowIfNull(previsoes);
        PrevisaoChegada[] lista = [.. previsoes.Where(previsao => !previsao.VencidaEm(consultadoEm))];
        StatusPrevisao status = lista.Length > 0 ? StatusPrevisao.Disponivel : StatusPrevisao.SemPrevisoes;
        return new ResultadoProvedor(status, origem, consultadoEm, lista);
    }

    /// <summary>O provedor falhou, não respondeu no prazo ou está desligado.</summary>
    public static ResultadoProvedor Indisponivel(OrigemDado origem, DateTimeOffset consultadoEm) =>
        new(StatusPrevisao.ProvedorIndisponivel, origem, consultadoEm, []);

    /// <summary>A próxima chegada prevista de uma linha, ou <c>null</c> quando não há previsão para ela.</summary>
    public PrevisaoChegada? ProximaChegada(int linhaId) =>
        Previsoes.Where(p => p.Linha.LinhaId == linhaId).MinBy(p => p.PrevistaParaUtc);
}

using Microsoft.Extensions.Options;

using PontoVoz.Api.Application.Interfaces;
using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.ValueObjects;

namespace PontoVoz.Api.Infrastructure.Providers;

/// <summary>
/// O <see cref="IProvedorPrevisaoChegada"/> usado pela aplicação. Escolhe, por configuração, quem
/// responde por uma parada, sem mudar services nem controllers:
/// <list type="bullet">
/// <item>padrão: o <see cref="ProvedorPrevisaoSimulado"/>;</item>
/// <item>Olho Vivo só quando habilitado, com token, e a parada e ao menos uma linha têm código
/// oficial declarado; dados <c>SIM-</c> ficam sempre no simulado (R8).</item>
/// </list>
/// Uma falha da Olho Vivo continua indisponibilidade (503): não há troca silenciosa para o
/// simulado, que apresentaria horários fictícios no lugar de uma previsão real.
/// </summary>
public sealed class SeletorProvedorPrevisao(
    ProvedorPrevisaoSimulado simulado,
    ProvedorOlhoVivo olhoVivo,
    IOptionsMonitor<OpcoesPrevisoes> opcoes) : IProvedorPrevisaoChegada
{
    public OrigemDado OrigemPara(Parada parada) =>
        UsaOlhoVivo(parada, linhas: null) ? OrigemDado.OlhoVivo : OrigemDado.Simulado;

    public Task<ResultadoProvedor> ConsultarAsync(Parada parada, IReadOnlyList<Linha> linhas, DateTimeOffset referencia, CancellationToken cancellationToken) =>
        UsaOlhoVivo(parada, linhas)
            ? olhoVivo.ConsultarAsync(parada, linhas, referencia, cancellationToken)
            : simulado.ConsultarAsync(parada, linhas, referencia, cancellationToken);

    private bool UsaOlhoVivo(Parada parada, IReadOnlyList<Linha>? linhas)
    {
        ArgumentNullException.ThrowIfNull(parada);
        OpcoesOlhoVivo configuracao = opcoes.CurrentValue.OlhoVivo;
        if (!configuracao.PodeConsultar || ProvedorOlhoVivo.EhSimulado(parada) || !configuracao.Paradas.ContainsKey(parada.CodigoExterno))
        {
            return false;
        }

        return linhas is null || linhas.Any(linha => !ProvedorOlhoVivo.EhSimulada(linha) && configuracao.Linhas.ContainsKey(linha.Codigo));
    }
}

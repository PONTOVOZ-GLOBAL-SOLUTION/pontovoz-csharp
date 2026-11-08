using PontoVoz.Api.Application.Dtos;
using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.ValueObjects;

namespace PontoVoz.Api.Application.Interfaces;

/// <summary>
/// Fonte de previsões de chegada (contrato, seção 4.5). Trocar a implementação (simulada ou Olho
/// Vivo) não muda o service nem os controllers. Uma falha do provedor não é exceção: volta como
/// <see cref="ResultadoProvedor.Indisponivel"/>, para que cada consumidor decida o que fazer
/// (503 no endpoint de previsões, status no resumo e nas alternativas). Só o cancelamento
/// da requisição é propagado como <see cref="OperationCanceledException"/>.
/// </summary>
public interface IProvedorPrevisaoChegada
{
    /// <summary>
    /// Próximas chegadas das <paramref name="linhas"/> na <paramref name="parada"/>, calculadas em
    /// relação a <paramref name="referencia"/>, o instante único da consulta. Previsões vencidas
    /// não são devolvidas.
    /// </summary>
    Task<ResultadoProvedor> ConsultarAsync(Parada parada, IReadOnlyList<Linha> linhas, DateTimeOffset referencia, CancellationToken cancellationToken);

    /// <summary>A origem do provedor que responderia por esta parada, mesmo sem consultá-lo (parada sem linhas).</summary>
    OrigemDado OrigemPara(Parada parada);
}

/// <summary><c>GET /api/v1/paradas/{id}/previsoes</c> (seção 4.5).</summary>
public interface IPrevisoesService
{
    Task<PrevisoesParadaDto> ObterAsync(int paradaId, int? linhaId, CancellationToken cancellationToken);
}

/// <summary><c>GET /api/v1/paradas/{id}/alternativas?destinoId=</c> (seção 4.7).</summary>
public interface IAlternativasService
{
    Task<AlternativasDeslocamentoDto> ObterAsync(int paradaId, int? destinoId, CancellationToken cancellationToken);
}

/// <summary><c>GET /api/v1/paradas/{id}/resumo</c> (seção 4.8).</summary>
public interface IResumoParadaService
{
    Task<ResumoParadaDto> ObterAsync(int paradaId, CancellationToken cancellationToken);
}

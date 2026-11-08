using PontoVoz.Api.Domain.Entities;

namespace PontoVoz.Api.Application.Interfaces;

/// <summary>Consultas de passagens registradas (observações simuladas e manuais).</summary>
public interface IRegistroPassagemRepository
{
    /// <summary>
    /// A última passagem de cada linha ativa da parada, até <paramref name="referencia"/>
    /// (contrato, seção 4.6.1): o maior <c>ObservadaEmUtc</c> que não esteja no futuro e, em empate,
    /// o maior <c>RegistradaEmUtc</c> e o maior <c>Id</c>. Sem limite para o passado. Cada item vem
    /// com a linha carregada e no tipo concreto (simulada ou manual). Ordem: do mais recente para o
    /// mais antigo, depois pelo código da linha. Desatualização e avisos ficam com o service.
    /// </summary>
    Task<IReadOnlyList<RegistroPassagem>> ListarUltimasPorLinhaAsync(int paradaId, DateTimeOffset referencia, int? linhaId, CancellationToken cancellationToken);
}

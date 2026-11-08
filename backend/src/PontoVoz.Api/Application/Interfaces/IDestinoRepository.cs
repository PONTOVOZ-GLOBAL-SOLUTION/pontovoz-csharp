using PontoVoz.Api.Application.Consultas;
using PontoVoz.Api.Domain.Entities;

namespace PontoVoz.Api.Application.Interfaces;

/// <summary>Consultas de destinos e dos percursos diretos até eles.</summary>
public interface IDestinoRepository
{
    /// <summary>Destinos ativos, ordenados por nome e id (contrato, seção 4.4), com busca sem acentos no nome.</summary>
    Task<Pagina<Destino>> BuscarAtivosAsync(string? termo, Paginacao paginacao, CancellationToken cancellationToken);

    /// <summary>O destino ativo com o <paramref name="id"/>, ou <c>null</c>.</summary>
    Task<Destino?> ObterAtivoAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Percursos diretos cadastrados da parada até o destino, só de linhas ativas que atendem a
    /// parada, com a linha carregada. Ordenados pelo tempo estimado e pelo código da linha. São os
    /// dados de entrada do cálculo de alternativas, feito no service.
    /// </summary>
    Task<IReadOnlyList<PercursoEstimado>> ListarPercursosDiretosAsync(int paradaId, int destinoId, CancellationToken cancellationToken);
}

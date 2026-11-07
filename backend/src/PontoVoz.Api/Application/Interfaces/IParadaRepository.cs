using PontoVoz.Api.Application.Consultas;
using PontoVoz.Api.Domain.Entities;

namespace PontoVoz.Api.Application.Interfaces;

/// <summary>Consultas de paradas para as rotas públicas: só paradas ativas.</summary>
public interface IParadaRepository
{
    /// <summary>
    /// Paradas ativas, ordenadas por nome e id (contrato, seção 4.1). <paramref name="termo"/>
    /// procura no nome e no código, sem diferenciar maiúsculas e acentos;
    /// <paramref name="codigoExterno"/> é exato, sem diferenciar maiúsculas. Os filtros se somam.
    /// </summary>
    Task<Pagina<Parada>> BuscarAtivasAsync(string? termo, string? codigoExterno, Paginacao paginacao, CancellationToken cancellationToken);

    /// <summary>A parada ativa com o <paramref name="id"/>, ou <c>null</c> se não existe ou está inativa.</summary>
    Task<Parada?> ObterAtivaAsync(int id, CancellationToken cancellationToken);
}

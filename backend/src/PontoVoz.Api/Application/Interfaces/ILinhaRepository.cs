using PontoVoz.Api.Application.Consultas;
using PontoVoz.Api.Domain.Entities;

namespace PontoVoz.Api.Application.Interfaces;

/// <summary>Consultas de linhas e das associações entre paradas e linhas.</summary>
public interface ILinhaRepository
{
    /// <summary>
    /// Linhas ativas, ordenadas por código e id (contrato, seção 4.3). <paramref name="termo"/>
    /// procura no código, no letreiro e nos dois terminais.
    /// </summary>
    Task<Pagina<Linha>> BuscarAtivasAsync(string? termo, Paginacao paginacao, CancellationToken cancellationToken);

    /// <summary>A linha ativa com o <paramref name="id"/>, ou <c>null</c>.</summary>
    Task<Linha?> ObterAtivaAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Linhas ativas que atendem a parada, por uma associação persistida, ordenadas por código
    /// e id. É a lista de <c>ParadaDetalhe.linhas</c>.
    /// </summary>
    Task<IReadOnlyList<Linha>> ListarAtivasDaParadaAsync(int paradaId, CancellationToken cancellationToken);

    /// <summary>Se existe a associação persistida entre a parada e a linha (regra do 422 <c>LinhaNaoAtendeParada</c>).</summary>
    Task<bool> AtendeParadaAsync(int paradaId, int linhaId, CancellationToken cancellationToken);
}

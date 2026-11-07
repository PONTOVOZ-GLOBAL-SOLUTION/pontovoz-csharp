using PontoVoz.Api.Application.Consultas;
using PontoVoz.Api.Application.Interfaces;
using PontoVoz.Api.Domain.Entities;

namespace PontoVoz.Tests.Application;

/// <summary>
/// Repositórios em memória para testar as regras dos services sem banco. Guardam os argumentos
/// recebidos, para os testes confirmarem que uma requisição inválida não chega a consultar.
/// As consultas reais (filtros, ordem, SQL) são testadas contra o SQLite em outras classes.
/// </summary>
internal sealed class DublesRepositorio : IParadaRepository, ILinhaRepository, IDestinoRepository, IRegistroPassagemRepository
{
    public List<Parada> Paradas { get; } = [];

    public List<Linha> Linhas { get; } = [];

    public List<Destino> Destinos { get; } = [];

    public List<RegistroPassagem> Passagens { get; } = [];

    public List<string> Chamadas { get; } = [];

    public (string? Termo, string? Codigo, Paginacao? Paginacao) UltimaBuscaParadas { get; private set; }

    public DateTimeOffset? UltimaReferenciaPassagens { get; private set; }

    Task<Pagina<Parada>> IParadaRepository.BuscarAtivasAsync(string? termo, string? codigoExterno, Paginacao paginacao, CancellationToken cancellationToken)
    {
        Chamadas.Add("Paradas.Buscar");
        UltimaBuscaParadas = (termo, codigoExterno, paginacao);
        List<Parada> ativas = [.. Paradas.Where(p => p.Ativa)];
        return Task.FromResult(new Pagina<Parada>([.. ativas.Skip(paginacao.Deslocamento).Take(paginacao.TamanhoPagina)], paginacao.Pagina, paginacao.TamanhoPagina, ativas.Count));
    }

    Task<Parada?> IParadaRepository.ObterAtivaAsync(int id, CancellationToken cancellationToken)
    {
        Chamadas.Add("Paradas.Obter");
        return Task.FromResult(Paradas.SingleOrDefault(p => p.Id == id && p.Ativa));
    }

    Task<Pagina<Linha>> ILinhaRepository.BuscarAtivasAsync(string? termo, Paginacao paginacao, CancellationToken cancellationToken)
    {
        Chamadas.Add("Linhas.Buscar");
        List<Linha> ativas = [.. Linhas.Where(l => l.Ativa)];
        return Task.FromResult(new Pagina<Linha>(ativas, paginacao.Pagina, paginacao.TamanhoPagina, ativas.Count));
    }

    Task<Linha?> ILinhaRepository.ObterAtivaAsync(int id, CancellationToken cancellationToken)
    {
        Chamadas.Add("Linhas.Obter");
        return Task.FromResult(Linhas.SingleOrDefault(l => l.Id == id && l.Ativa));
    }

    Task<IReadOnlyList<Linha>> ILinhaRepository.ListarAtivasDaParadaAsync(int paradaId, CancellationToken cancellationToken)
    {
        Chamadas.Add("Linhas.DaParada");
        IReadOnlyList<Linha> resultado = [.. Paradas.Single(p => p.Id == paradaId).LinhasAssociadas
            .Select(a => a.Linha).Where(l => l.Ativa).OrderBy(l => l.Codigo, StringComparer.Ordinal)];
        return Task.FromResult(resultado);
    }

    Task<bool> ILinhaRepository.AtendeParadaAsync(int paradaId, int linhaId, CancellationToken cancellationToken)
    {
        Chamadas.Add("Linhas.Atende");
        return Task.FromResult(Paradas.Single(p => p.Id == paradaId).LinhasAssociadas.Any(a => a.LinhaId == linhaId));
    }

    Task<Pagina<Destino>> IDestinoRepository.BuscarAtivosAsync(string? termo, Paginacao paginacao, CancellationToken cancellationToken)
    {
        Chamadas.Add("Destinos.Buscar");
        List<Destino> ativos = [.. Destinos.Where(d => d.Ativo)];
        return Task.FromResult(new Pagina<Destino>(ativos, paginacao.Pagina, paginacao.TamanhoPagina, ativos.Count));
    }

    Task<Destino?> IDestinoRepository.ObterAtivoAsync(int id, CancellationToken cancellationToken) =>
        throw new NotSupportedException("Não usado pelos services do B5.");

    Task<IReadOnlyList<PercursoEstimado>> IDestinoRepository.ListarPercursosDiretosAsync(int paradaId, int destinoId, CancellationToken cancellationToken) =>
        throw new NotSupportedException("Não usado pelos services do B5.");

    /// <summary>Devolve as passagens cadastradas como "últimas"; a seleção real é testada no SQLite.</summary>
    Task<IReadOnlyList<RegistroPassagem>> IRegistroPassagemRepository.ListarUltimasPorLinhaAsync(int paradaId, DateTimeOffset referencia, int? linhaId, CancellationToken cancellationToken)
    {
        Chamadas.Add("Passagens.Ultimas");
        UltimaReferenciaPassagens = referencia;
        IReadOnlyList<RegistroPassagem> resultado = [.. Passagens.Where(p => p.ParadaId == paradaId && (linhaId is null || p.LinhaId == linhaId))];
        return Task.FromResult(resultado);
    }
}

/// <summary>Relógio fixo que conta quantas vezes o instante foi lido.</summary>
internal sealed class RelogioContador(DateTimeOffset agora) : TimeProvider
{
    public int Leituras { get; private set; }

    public override DateTimeOffset GetUtcNow()
    {
        Leituras++;
        return agora;
    }
}

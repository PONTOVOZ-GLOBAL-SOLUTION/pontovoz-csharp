namespace PontoVoz.Api.Application.Erros;

/// <summary>Categoria do erro, que o tratamento HTTP do B6 converte em status.</summary>
public enum TipoErroAplicacao
{
    /// <summary>Parâmetro com formato ou faixa inválidos (400).</summary>
    Validacao = 1,

    /// <summary>Recurso inexistente ou inativo nas rotas públicas (404).</summary>
    NaoEncontrado = 2,

    /// <summary>Requisição bem formada que contradiz o estado dos dados (422).</summary>
    RegraNegocio = 3,
}

/// <summary>
/// Erro esperado de um service, com o <see cref="Codigo"/> estável do catálogo de erros do
/// contrato (contracts/api-v1.md, seção 7). Não carrega detalhes de HTTP: o B6 monta o
/// ProblemDetails a partir de <see cref="Tipo"/>, <see cref="Titulo"/>, <see cref="Message"/> e
/// <see cref="Erros"/>. Erros inesperados não usam esta classe.
/// </summary>
public sealed class ErroAplicacaoException : Exception
{
    private ErroAplicacaoException(TipoErroAplicacao tipo, string codigo, string titulo, string detalhe, IReadOnlyDictionary<string, string[]>? erros = null)
        : base(detalhe)
    {
        Tipo = tipo;
        Codigo = codigo;
        Titulo = titulo;
        Erros = erros ?? new Dictionary<string, string[]>();
    }

    public TipoErroAplicacao Tipo { get; }

    public string Codigo { get; }

    public string Titulo { get; }

    /// <summary>Só em erros de validação: campo em camelCase e suas mensagens.</summary>
    public IReadOnlyDictionary<string, string[]> Erros { get; }

    public static ErroAplicacaoException Validacao(IReadOnlyDictionary<string, string[]> erros) =>
        new(TipoErroAplicacao.Validacao, "RequisicaoInvalida", "A requisição contém parâmetros inválidos.", "Um ou mais parâmetros estão fora do formato ou da faixa aceita.", erros);

    public static ErroAplicacaoException ParadaNaoEncontrada(int id) =>
        new(TipoErroAplicacao.NaoEncontrado, "ParadaNaoEncontrada", "Parada não encontrada", $"Não existe parada ativa com o id {id}.");

    public static ErroAplicacaoException LinhaNaoEncontrada(int id) =>
        new(TipoErroAplicacao.NaoEncontrado, "LinhaNaoEncontrada", "Linha não encontrada", $"Não existe linha ativa com o id {id}.");

    public static ErroAplicacaoException DestinoNaoEncontrado(int id) =>
        new(TipoErroAplicacao.NaoEncontrado, "DestinoNaoEncontrado", "Destino não encontrado", $"Não existe destino ativo com o id {id}.");

    public static ErroAplicacaoException LinhaNaoAtendeParada(int paradaId, int linhaId) =>
        new(TipoErroAplicacao.RegraNegocio, "LinhaNaoAtendeParada", "Linha não atende a parada", $"A linha {linhaId} não atende a parada {paradaId}.");
}

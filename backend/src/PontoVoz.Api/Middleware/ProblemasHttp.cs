using Microsoft.AspNetCore.Mvc;

using PontoVoz.Api.Application.Erros;
using PontoVoz.Api.Domain.Exceptions;

namespace PontoVoz.Api.Middleware;

/// <summary>
/// Monta os ProblemDetails do catálogo de erros (contracts/api-v1.md, seção 7). O <c>type</c>, o
/// <c>traceId</c> e o <c>instance</c> são completados pelo <c>IProblemDetailsService</c> ao escrever.
/// </summary>
internal static class ProblemasHttp
{
    public const string CodigoRequisicaoInvalida = "RequisicaoInvalida";
    public const string TituloRequisicaoInvalida = "A requisição contém parâmetros inválidos.";

    /// <summary>400 <c>RequisicaoInvalida</c>, com os erros agrupados por campo (camelCase).</summary>
    public static HttpValidationProblemDetails Validacao(IDictionary<string, string[]> erros) =>
        Criar(new HttpValidationProblemDetails(erros), StatusCodes.Status400BadRequest, TituloRequisicaoInvalida, CodigoRequisicaoInvalida, detalhe: null);

    /// <summary>500 <c>ErroInterno</c>, sem nenhum detalhe da exceção.</summary>
    public static ProblemDetails ErroInterno() =>
        Criar(new ProblemDetails(), StatusCodes.Status500InternalServerError, "Erro interno", "ErroInterno",
            "Ocorreu um erro inesperado. Tente novamente mais tarde.");

    /// <summary>
    /// Completa os ProblemDetails que o ASP.NET Core gera sem passar pelo catálogo: o 404 de um
    /// caminho que não corresponde a nenhuma rota (inclusive <c>id</c> não numérico ou menor que 1)
    /// e o 405 de um método que a rota não aceita. Assim todo erro tem <c>codigo</c>. Os
    /// ProblemDetails que já têm <c>codigo</c> não mudam.
    /// </summary>
    public static void CompletarCodigo(ProblemDetails problema)
    {
        if (problema.Extensions.ContainsKey("codigo"))
        {
            return;
        }

        switch (problema.Status)
        {
            case StatusCodes.Status404NotFound:
                problema.Title = "Rota não encontrada";
                problema.Detail = "Nenhuma rota da API corresponde ao caminho informado.";
                problema.Extensions["codigo"] = "RotaNaoEncontrada";
                break;
            case StatusCodes.Status405MethodNotAllowed:
                problema.Title = "Método não permitido";
                problema.Detail = "A rota não aceita o método HTTP usado. Os métodos aceitos estão no cabeçalho Allow.";
                problema.Extensions["codigo"] = "MetodoNaoPermitido";
                break;
        }
    }

    /// <summary>ProblemDetails de um erro esperado, ou <c>null</c> se a exceção não é um erro do catálogo.</summary>
    public static ProblemDetails? DeExcecao(Exception excecao) => excecao switch
    {
        ErroAplicacaoException { Tipo: TipoErroAplicacao.Validacao } erro =>
            Validacao(erro.Erros.ToDictionary(par => par.Key, par => par.Value)),
        ErroAplicacaoException erro =>
            Criar(new ProblemDetails(), Status(erro.Tipo), erro.Titulo, erro.Codigo, erro.Message),
        ValidacaoDominioException erro =>
            Validacao(new Dictionary<string, string[]> { [erro.Campo] = [erro.Message] }),
        RegraNegocioException erro =>
            Criar(new ProblemDetails(), StatusCodes.Status422UnprocessableEntity, TituloRegraNegocio(erro.Codigo), erro.Codigo, erro.Message),
        ProvedorPrevisoesIndisponivelException erro =>
            Criar(new ProblemDetails(), StatusCodes.Status503ServiceUnavailable, "Previsões indisponíveis", erro.Codigo, erro.Message),
        _ => null,
    };

    private static int Status(TipoErroAplicacao tipo) => tipo switch
    {
        TipoErroAplicacao.NaoEncontrado => StatusCodes.Status404NotFound,
        TipoErroAplicacao.RegraNegocio => StatusCodes.Status422UnprocessableEntity,
        TipoErroAplicacao.Conflito => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status400BadRequest,
    };

    /// <summary>Título legível dos 422 lançados pelo domínio (catálogo da seção 7).</summary>
    private static string TituloRegraNegocio(string codigo) => codigo switch
    {
        RegraNegocioException.ParadaInativa => "Parada inativa",
        RegraNegocioException.LinhaInativa => "Linha inativa",
        RegraNegocioException.LinhaNaoAtendeParada => "Linha não atende a parada",
        _ => "Regra de negócio não atendida",
    };

    private static T Criar<T>(T problema, int status, string titulo, string codigo, string? detalhe)
        where T : ProblemDetails
    {
        problema.Status = status;
        problema.Title = titulo;
        problema.Detail = detalhe;
        problema.Extensions["codigo"] = codigo;
        return problema;
    }
}

using System.Text.Json;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using PontoVoz.Api.Application.Dtos;

namespace PontoVoz.Api.Middleware;

/// <summary>Configuração da camada HTTP: controllers, JSON do contrato, ProblemDetails e CORS.</summary>
public static class WebApiExtensions
{
    public const string PoliticaCors = "Totem";
    public const string SecaoOrigensPermitidas = "Cors:OrigensPermitidas";

    public static IServiceCollection AddWebApiPontoVoz(this IServiceCollection services, IConfiguration configuration)
    {
        // Um só padrão de JSON (JsonContrato) para controllers, ProblemDetails e documento OpenAPI.
        services.AddControllers(opcoes =>
            {
                // Corpo com Content-Type que não é JSON: em vez do 415 sem código, o erro fica no
                // ModelState e sai como 400 RequisicaoInvalida em "corpo" (contrato, seção 7).
                foreach (IFilterMetadata filtro in opcoes.Filters.Where(filtro => filtro is UnsupportedContentTypeFilter).ToList())
                {
                    opcoes.Filters.Remove(filtro);
                }
            })
            .AddJsonOptions(opcoes => JsonContrato.Configurar(opcoes.JsonSerializerOptions))
            .ConfigureApiBehaviorOptions(opcoes => opcoes.InvalidModelStateResponseFactory = RespostaDeModelBindingInvalido);
        services.ConfigureHttpJsonOptions(opcoes => JsonContrato.Configurar(opcoes.SerializerOptions));

        services.AddProblemDetails(opcoes => opcoes.CustomizeProblemDetails = contexto =>
        {
            contexto.ProblemDetails.Instance ??= contexto.HttpContext.Request.Path;
            ProblemasHttp.CompletarCodigo(contexto.ProblemDetails);
        });
        services.AddExceptionHandler<TratadorExcecoes>();

        // Só as origens listadas; sem configuração, nenhuma origem externa é aceita.
        string[] origens = configuration.GetSection(SecaoOrigensPermitidas).Get<string[]>() ?? [];
        services.AddCors(opcoes => opcoes.AddPolicy(PoliticaCors, politica =>
        {
            if (origens.Length > 0)
            {
                politica.WithOrigins(origens).WithMethods(HttpMethods.Get).WithHeaders("Accept", "Content-Type");
            }
        }));

        return services;
    }

    /// <summary>
    /// Erros de model binding (por exemplo, <c>pagina=abc</c>) no mesmo formato dos erros de
    /// validação dos services: 400 <c>RequisicaoInvalida</c>, com mensagens em português e sem
    /// repetir o valor recebido. O <c>ProblemDetailsFactory</c> completa <c>type</c>, <c>traceId</c> e
    /// <c>instance</c>, como nos demais erros.
    /// </summary>
    private static IActionResult RespostaDeModelBindingInvalido(ActionContext contexto)
    {
        var erros = new ModelStateDictionary();
        foreach (string chave in contexto.ModelState.Where(par => par.Value?.Errors.Count > 0).Select(par => par.Key))
        {
            string campo = Campo(chave, contexto);
            if (erros.ContainsKey(campo))
            {
                continue;
            }

            erros.AddModelError(campo, campo == CampoCorpo
                ? "O corpo da requisição deve ser um JSON válido, enviado com Content-Type: application/json."
                : $"O valor informado para {campo} é inválido.");
        }

        ValidationProblemDetails problema = contexto.HttpContext.RequestServices
            .GetRequiredService<ProblemDetailsFactory>()
            .CreateValidationProblemDetails(contexto.HttpContext, erros, StatusCodes.Status400BadRequest, ProblemasHttp.TituloRequisicaoInvalida);
        problema.Extensions["codigo"] = ProblemasHttp.CodigoRequisicaoInvalida;

        return new ObjectResult(problema)
        {
            StatusCode = StatusCodes.Status400BadRequest,
            ContentTypes = { "application/problem+json" },
        };
    }

    private const string CampoCorpo = "corpo";

    /// <summary>
    /// Nome do campo no contrato a partir da chave do ModelState: <c>pagina</c> na query,
    /// <c>$.dadosSimulados</c> ou <c>$.coordenada.latitude</c> num corpo JSON. Erros do corpo
    /// inteiro (JSON malformado, Content-Type errado, chave vazia ou com o nome do parâmetro)
    /// ficam em <c>corpo</c>.
    /// </summary>
    private static string Campo(string chave, ActionContext contexto)
    {
        bool parametroDeCorpo = contexto.ActionDescriptor.Parameters.Any(parametro =>
            parametro.BindingInfo?.BindingSource == BindingSource.Body && string.Equals(parametro.Name, chave, StringComparison.Ordinal));
        if (chave.Length == 0 || chave == "$" || parametroDeCorpo)
        {
            return CampoCorpo;
        }

        string caminho = chave.StartsWith("$.", StringComparison.Ordinal) ? chave[2..] : chave;
        return string.Join('.', caminho.Split('.').Select(CamelCase));
    }

    private static string CamelCase(string nome) => JsonNamingPolicy.CamelCase.ConvertName(nome);
}

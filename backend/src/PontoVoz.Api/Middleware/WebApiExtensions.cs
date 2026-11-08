using System.Text.Json;

using Microsoft.AspNetCore.Mvc;
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
        services.AddControllers()
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
            string campo = CamelCase(chave);
            erros.AddModelError(campo, $"O valor informado para {campo} é inválido.");
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

    private static string CamelCase(string nome) => JsonNamingPolicy.CamelCase.ConvertName(nome);
}

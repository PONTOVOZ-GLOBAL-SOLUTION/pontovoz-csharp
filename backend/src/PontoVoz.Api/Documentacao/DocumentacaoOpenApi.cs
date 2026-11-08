using System.Reflection;
using System.Text.Json;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

using PontoVoz.Api.Application.Dtos;
using PontoVoz.Api.Security;

namespace PontoVoz.Api.Documentacao;

/// <summary>
/// Documento OpenAPI gerado pela API (<c>/openapi/v1.json</c>): título, descrição, a chave
/// administrativa como esquema de segurança das rotas <c>/api/v1/admin</c> e os campos
/// obrigatórios dos corpos administrativos. Nenhum valor de chave aparece no documento.
/// </summary>
public static class DocumentacaoOpenApi
{
    public const string EsquemaChaveAdministrativa = "ChaveAdministrativa";

    private const string PrefixoAdministrativo = "api/v1/admin/";

    public static IServiceCollection AddDocumentacaoPontoVoz(this IServiceCollection services) =>
        services.AddOpenApi(opcoes =>
        {
            opcoes.AddDocumentTransformer(DescreverDocumento);
            opcoes.AddDocumentTransformer(RetirarNuloDosEnumsObrigatorios);
            opcoes.AddOperationTransformer(ExigirChaveNasRotasAdministrativas);
            opcoes.AddOperationTransformer(DocumentarRetryAfter);
            opcoes.AddSchemaTransformer(MarcarCamposObrigatorios);
        });

    private static Task DescreverDocumento(OpenApiDocument documento, OpenApiDocumentTransformerContext contexto, CancellationToken cancellationToken)
    {
        documento.Info.Title = "PontoVoz API";
        documento.Info.Version = "v1";
        documento.Info.Description =
            "API do totem PontoVoz (CitizenTech FIAP). Lista só os endpoints já implementados: consultas públicas (B6), "
            + "operações administrativas do operador (B7) e previsões de chegada (B8, provedor simulado por padrão). "
            + "Alternativas e resumo estão planejados em contracts/api-v1.md. "
            + "Todos os dados de demonstração são simulados e marcados com dadosSimulados.";

        documento.Components ??= new OpenApiComponents();
        documento.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        documento.Components.SecuritySchemes[EsquemaChaveAdministrativa] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Name = ChaveAdministrativaFilter.Cabecalho,
            Description = "Chave das rotas /api/v1/admin, configurada em Admin:Chave (user-secrets ou variável de ambiente). "
                + "Em Development sem chave configurada, as rotas ficam abertas para a demonstração local; nos demais ambientes, "
                + "sem chave configurada, respondem 403.",
        };
        return Task.CompletedTask;
    }

    /// <summary>
    /// Um enum anulável referenciado (como <c>sentido</c>) só vira <c>oneOf: [null, enum]</c> quando
    /// o documento é montado, depois dos transformadores de esquema. Aqui, já com os esquemas
    /// prontos, o campo obrigatório passa a apontar só para o enum.
    /// </summary>
    private static Task RetirarNuloDosEnumsObrigatorios(OpenApiDocument documento, OpenApiDocumentTransformerContext contexto, CancellationToken cancellationToken)
    {
        foreach (OpenApiSchema esquema in documento.Components?.Schemas?.Values.OfType<OpenApiSchema>() ?? [])
        {
            if (esquema.Required is not { Count: > 0 } || esquema.Properties is null)
            {
                continue;
            }

            foreach (string nome in esquema.Required.Where(esquema.Properties.ContainsKey).ToList())
            {
                if (esquema.Properties[nome] is OpenApiSchema { OneOf: { Count: 2 } alternativas }
                    && alternativas.Count(a => a.Type == JsonSchemaType.Null) == 1)
                {
                    esquema.Properties[nome] = alternativas.Single(a => a.Type != JsonSchemaType.Null);
                }
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>O 503 de previsões traz <c>Retry-After: 30</c> (seção 4.5).</summary>
    private static Task DocumentarRetryAfter(OpenApiOperation operacao, OpenApiOperationTransformerContext contexto, CancellationToken cancellationToken)
    {
        if (operacao.Responses?.TryGetValue("503", out IOpenApiResponse? resposta) == true && resposta is OpenApiResponse editavel)
        {
            editavel.Headers ??= new Dictionary<string, IOpenApiHeader>();
            editavel.Headers["Retry-After"] = new OpenApiHeader
            {
                Description = "Segundos até uma nova tentativa (sempre 30).",
                Schema = new OpenApiSchema { Type = JsonSchemaType.Integer },
            };
        }

        return Task.CompletedTask;
    }

    private static Task ExigirChaveNasRotasAdministrativas(OpenApiOperation operacao, OpenApiOperationTransformerContext contexto, CancellationToken cancellationToken)
    {
        if (contexto.Description.RelativePath?.StartsWith(PrefixoAdministrativo, StringComparison.OrdinalIgnoreCase) == true)
        {
            operacao.Security ??= [];
            operacao.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(EsquemaChaveAdministrativa, contexto.Document)] = [],
            });
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Os campos de entrada são anuláveis no C# para o service detectar a ausência. Nos corpos
    /// administrativos, o documento marca como obrigatórios exatamente os campos com
    /// <see cref="CampoObrigatorioAttribute"/> e retira deles o tipo <c>null</c>.
    /// </summary>
    private static Task MarcarCamposObrigatorios(OpenApiSchema esquema, OpenApiSchemaTransformerContext contexto, CancellationToken cancellationToken)
    {
        PropertyInfo[] obrigatorios = [.. contexto.JsonTypeInfo.Type.GetProperties().Where(p => p.IsDefined(typeof(CampoObrigatorioAttribute)))];
        if (obrigatorios.Length == 0 || esquema.Properties is null)
        {
            return Task.CompletedTask;
        }

        // Só os marcados: os parâmetros do construtor do record não são todos obrigatórios (endereco e coordenada são opcionais).
        esquema.Required = new HashSet<string>();
        foreach (PropertyInfo propriedade in obrigatorios)
        {
            string nome = JsonNamingPolicy.CamelCase.ConvertName(propriedade.Name);
            if (!esquema.Properties.TryGetValue(nome, out IOpenApiSchema? campo))
            {
                continue;
            }

            esquema.Required.Add(nome);
            if (campo is not OpenApiSchema editavel)
            {
                continue;
            }

            if (editavel.Type is JsonSchemaType tipoCampo)
            {
                editavel.Type = tipoCampo & ~JsonSchemaType.Null;
            }
            else if (editavel.Format == "date-time")
            {
                // observadaEm usa um conversor próprio (exige o fuso), e o gerador perde o tipo.
                editavel.Type = JsonSchemaType.String;
            }

        }

        return Task.CompletedTask;
    }
}

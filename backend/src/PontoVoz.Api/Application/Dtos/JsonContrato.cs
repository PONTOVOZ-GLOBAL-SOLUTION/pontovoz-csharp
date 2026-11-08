using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace PontoVoz.Api.Application.Dtos;

/// <summary>
/// Regras de serialização do contrato: nomes em camelCase, enums como texto e nulos sempre
/// presentes (seções 1.2 e 1.4). Os testes de compatibilidade usam estas opções; o B6 aplica as
/// mesmas regras aos controllers.
/// </summary>
public static class JsonContrato
{
    public static JsonSerializerOptions Opcoes { get; } = Criar();

    public static void Configurar(JsonSerializerOptions opcoes)
    {
        ArgumentNullException.ThrowIfNull(opcoes);
        opcoes.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        opcoes.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
        // Números só como números (contrato, seção 1.2): "5" não é aceito onde se espera um inteiro.
        opcoes.NumberHandling = JsonNumberHandling.Strict;
        opcoes.Converters.Add(new JsonStringEnumConverter());
    }

    private static JsonSerializerOptions Criar()
    {
        var opcoes = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        };
        Configurar(opcoes);
        opcoes.MakeReadOnly();
        return opcoes;
    }
}

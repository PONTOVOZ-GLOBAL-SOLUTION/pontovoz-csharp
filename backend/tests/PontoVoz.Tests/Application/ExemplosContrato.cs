using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

using PontoVoz.Api.Application.Dtos;

namespace PontoVoz.Tests.Application;

/// <summary>
/// Lê os exemplos de <c>contracts/</c> para comparar com os DTOs serializados. Os ids internos
/// (<c>id</c>, <c>paradaId</c>, <c>linhaId</c>) são retirados dos dois lados, porque são gerados
/// pelo banco e não são estáveis (contrato, seção 1.2).
/// </summary>
internal static partial class ExemplosContrato
{
    private static readonly string[] CamposDeId = ["id", "paradaId", "linhaId"];

    private static readonly Lazy<string> PastaContratos = new(() =>
    {
        for (var pasta = new DirectoryInfo(AppContext.BaseDirectory); pasta is not null; pasta = pasta.Parent)
        {
            string candidata = Path.Combine(pasta.FullName, "contracts");
            if (File.Exists(Path.Combine(candidata, "api-v1.md")))
            {
                return candidata;
            }
        }

        throw new DirectoryNotFoundException("Pasta contracts/ não encontrada acima do diretório dos testes.");
    });

    /// <summary>Caminho de um arquivo dentro de <c>contracts/</c> (por exemplo, <c>openapi/pontovoz-v1.json</c>).</summary>
    public static string CaminhoNoContrato(string relativo) => Path.Combine(PastaContratos.Value, relativo);

    public static JsonNode Exemplo(string arquivo) =>
        JsonNode.Parse(File.ReadAllText(Path.Combine(PastaContratos.Value, "exemplos", arquivo)))!;

    /// <summary>Bloco JSON do <c>api-v1.md</c> que vem logo depois do texto <paramref name="titulo"/>.</summary>
    public static JsonNode BlocoDoDocumento(string titulo)
    {
        string documento = File.ReadAllText(Path.Combine(PastaContratos.Value, "api-v1.md"));
        int inicio = documento.IndexOf(titulo, StringComparison.Ordinal);
        Match bloco = BlocoJson().Match(documento, inicio);
        return JsonNode.Parse(bloco.Groups[1].Value)!;
    }

    public static JsonNode Serializar<T>(T dto) =>
        JsonSerializer.SerializeToNode(dto, JsonContrato.Opcoes)!;

    /// <summary>Compara o DTO serializado com o exemplo, campo a campo, sem os ids internos.</summary>
    public static void AssertIgualAoExemplo<T>(JsonNode exemplo, T dto) =>
        AssertJsonIgualAoExemplo(exemplo, Serializar(dto));

    /// <summary>Compara um JSON já pronto (por exemplo, o corpo de uma resposta HTTP) com o exemplo, sem os ids internos.</summary>
    public static void AssertJsonIgualAoExemplo(JsonNode exemplo, JsonNode json)
    {
        JsonNode esperado = SemIds(exemplo.DeepClone());
        JsonNode obtido = SemIds(json.DeepClone());
        Assert.True(JsonNode.DeepEquals(esperado, obtido), $"Diferente do contrato.\nEsperado: {esperado.ToJsonString()}\nObtido:   {obtido.ToJsonString()}");
    }

    private static JsonNode SemIds(JsonNode no)
    {
        if (no is JsonObject objeto)
        {
            foreach (string campo in CamposDeId)
            {
                objeto.Remove(campo);
            }

            foreach (var (_, valor) in objeto)
            {
                if (valor is not null)
                {
                    SemIds(valor);
                }
            }
        }
        else if (no is JsonArray lista)
        {
            foreach (JsonNode? item in lista)
            {
                if (item is not null)
                {
                    SemIds(item);
                }
            }
        }

        return no;
    }

    [GeneratedRegex(@"```json\r?\n(.*?)```", RegexOptions.Singleline)]
    private static partial Regex BlocoJson();
}

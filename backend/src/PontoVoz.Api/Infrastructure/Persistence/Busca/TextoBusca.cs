using System.Globalization;
using System.Text;

using PontoVoz.Api.Domain.Entities;

namespace PontoVoz.Api.Infrastructure.Persistence.Busca;

/// <summary>
/// Normalização usada na coluna <c>TextoBusca</c> e no termo das consultas (requisito R7 do
/// contrato). A mesma função é aplicada na gravação e na consulta, então as duas sempre comparam
/// textos no mesmo formato.
/// </summary>
public static class TextoBusca
{
    /// <summary>Nome da propriedade sombra e da coluna em <c>Paradas</c>, <c>Linhas</c> e <c>Destinos</c>.</summary>
    public const string Coluna = "TextoBusca";

    /// <summary>
    /// Separa os campos de uma mesma linha da tabela. Como <see cref="Normalizar"/> transforma
    /// qualquer espaço em branco do termo em um espaço simples, uma busca nunca atravessa campos.
    /// </summary>
    private const char SeparadorCampos = '\n';

    /// <summary>
    /// Minúsculas, sem acentos e com os espaços reduzidos a um só. A decomposição Unicode (FormD)
    /// trata do mesmo jeito letras acentuadas prontas ("ç") e combinadas ("c" + cedilha).
    /// </summary>
    public static string Normalizar(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return string.Empty;
        }

        var resultado = new StringBuilder(texto.Length);
        bool espacoPendente = false;
        foreach (char caractere in texto.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caractere) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsWhiteSpace(caractere))
            {
                espacoPendente = resultado.Length > 0;
                continue;
            }

            if (espacoPendente)
            {
                resultado.Append(' ');
                espacoPendente = false;
            }

            resultado.Append(char.ToLowerInvariant(caractere));
        }

        return resultado.ToString().Normalize(NormalizationForm.FormC);
    }

    public static string DeParada(Parada parada) => Juntar(parada.Nome, parada.CodigoExterno);

    public static string DeLinha(Linha linha) => Juntar(linha.Codigo, linha.Letreiro, linha.TerminalPrincipal, linha.TerminalSecundario);

    public static string DeDestino(Destino destino) => Juntar(destino.Nome);

    /// <summary>Padrão <c>LIKE</c> de "contém", com <c>%</c>, <c>_</c> e <c>\</c> do termo escapados.</summary>
    public static string PadraoContem(string termoNormalizado) =>
        "%" + termoNormalizado.Replace(@"\", @"\\", StringComparison.Ordinal)
            .Replace("%", @"\%", StringComparison.Ordinal)
            .Replace("_", @"\_", StringComparison.Ordinal) + "%";

    public const string CaractereEscape = @"\";

    private static string Juntar(params string[] campos) =>
        string.Join(SeparadorCampos, campos.Select(Normalizar));
}

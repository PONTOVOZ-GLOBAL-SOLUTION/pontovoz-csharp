using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace PontoVoz.Api.Application.Dtos;

/// <summary>
/// Lê um <c>date-time</c> que precisa trazer o fuso (<c>Z</c> ou <c>±hh:mm</c>), como exige o
/// contrato para <c>observadaEm</c> (seção 5.6). O conversor padrão aceitaria
/// <c>2026-11-10T14:24:00</c> e completaria o fuso com o do servidor, o que mudaria o instante
/// sem aviso. Um valor sem fuso vira erro de leitura, e a API responde 400.
/// </summary>
public sealed partial class DataHoraComFusoConverter : JsonConverter<DateTimeOffset>
{
    [GeneratedRegex(@"(Z|z|[+-]\d{2}:\d{2})\z")]
    private static partial Regex TerminaComFuso();

    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? texto = reader.TokenType == JsonTokenType.String ? reader.GetString() : null;
        if (texto is null
            || !TerminaComFuso().IsMatch(texto)
            || !DateTimeOffset.TryParse(texto, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset valor))
        {
            throw new JsonException("Data e hora sem fuso ou em formato inválido.");
        }

        return valor;
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(value);
    }
}

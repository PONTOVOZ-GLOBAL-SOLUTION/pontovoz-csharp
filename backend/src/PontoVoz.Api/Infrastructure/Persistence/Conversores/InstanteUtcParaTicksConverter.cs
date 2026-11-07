using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace PontoVoz.Api.Infrastructure.Persistence.Conversores;

/// <summary>
/// Grava um <see cref="DateTimeOffset"/> como o número de ticks UTC (<c>INTEGER</c> no SQLite).
/// O provedor SQLite do EF Core não traduz comparações nem <c>ORDER BY</c> sobre
/// <see cref="DateTimeOffset"/>. Com um inteiro, a ordenação e filtros como
/// <c>ObservadaEmUtc &lt;= @referencia</c> rodam no banco, com a precisão de 100 ns do domínio.
/// A leitura devolve sempre deslocamento zero, como o domínio espera.
/// </summary>
public sealed class InstanteUtcParaTicksConverter() : ValueConverter<DateTimeOffset, long>(
    instante => instante.UtcTicks,
    ticks => new DateTimeOffset(ticks, TimeSpan.Zero));

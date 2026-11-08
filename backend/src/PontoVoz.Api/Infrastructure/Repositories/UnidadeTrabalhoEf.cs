using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using PontoVoz.Api.Application.Erros;
using PontoVoz.Api.Application.Interfaces;
using PontoVoz.Api.Infrastructure.Persistence;

namespace PontoVoz.Api.Infrastructure.Repositories;

/// <summary>
/// Grava as mudanças do <see cref="PontoVozDbContext"/> da requisição. Só a violação de chave
/// única ou primária do SQLite vira <see cref="ViolacaoUnicidadeException"/>; outras falhas
/// de gravação (chave estrangeira, disco, banco bloqueado) continuam como erro inesperado (500).
/// </summary>
public sealed class UnidadeTrabalhoEf(PontoVozDbContext contexto) : IUnidadeTrabalho
{
    // Códigos estendidos do SQLite (https://www.sqlite.org/rescode.html).
    private const int SqliteConstraintUnique = 2067;
    private const int SqliteConstraintPrimaryKey = 1555;

    public async Task SalvarAsync(CancellationToken cancellationToken)
    {
        try
        {
            await contexto.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException erro) when (ViolouUnicidade(erro))
        {
            throw new ViolacaoUnicidadeException(erro);
        }
    }

    /// <summary>Se a falha de gravação foi uma violação de chave única ou primária do SQLite.</summary>
    public static bool ViolouUnicidade(DbUpdateException erro) =>
        erro.InnerException is SqliteException { SqliteExtendedErrorCode: SqliteConstraintUnique or SqliteConstraintPrimaryKey };
}

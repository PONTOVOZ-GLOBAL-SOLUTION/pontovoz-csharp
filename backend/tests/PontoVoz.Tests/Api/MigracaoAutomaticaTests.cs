using System.Net;
using System.Text;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using PontoVoz.Api.Infrastructure.Persistence;
using PontoVoz.Tests.Infrastructure;

namespace PontoVoz.Tests.Api;

/// <summary>
/// Migração automática na inicialização (<c>Banco:AplicarMigracoesAoIniciar</c>): ligada em
/// Development pelo <c>appsettings.Development.json</c>, desligada e ignorada nos outros ambientes.
/// Cada teste usa um arquivo SQLite temporário; o banco de desenvolvimento não é usado.
/// </summary>
public sealed class MigracaoAutomaticaTests : IDisposable
{
    private static readonly Dictionary<string, long> ContagensDoSeed = new()
    {
        ["Paradas"] = 2,
        ["Linhas"] = 4,
        ["ParadasLinhas"] = 5,
        ["Destinos"] = 3,
        ["PercursosEstimados"] = 5,
        ["RegistrosPassagem"] = 2,
    };

    private readonly string _arquivo = Path.Combine(Path.GetTempPath(), $"pontovoz-migracao-{Guid.NewGuid():N}.db");

    private static CancellationToken Ct => BancoSqliteTeste.Ct;

    private string ConnectionString => $"Data Source={_arquivo}";

    /// <summary>API real no <paramref name="ambiente"/>; sem <paramref name="aplicarMigracoes"/>, vale o appsettings do ambiente.</summary>
    private WebApplicationFactory<Program> Api(string ambiente, string? aplicarMigracoes = null) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(ambiente);
            builder.UseSetting("ConnectionStrings:PontoVoz", ConnectionString);
            if (aplicarMigracoes is not null)
            {
                builder.UseSetting("Banco:AplicarMigracoesAoIniciar", aplicarMigracoes);
            }
        });

    private static async Task<HttpStatusCode> IniciarEConsultarHealthAsync(WebApplicationFactory<Program> api)
    {
        using HttpClient cliente = api.CreateClient();
        using HttpResponseMessage resposta = await cliente.GetAsync(new Uri("/health", UriKind.Relative), Ct);
        return resposta.StatusCode;
    }

    [Fact]
    public async Task Development_BancoInexistente_ECriadoComAsMigrationsEOSeed()
    {
        Assert.False(File.Exists(_arquivo));

        await using (WebApplicationFactory<Program> api = Api("Development"))
        {
            Assert.Equal(HttpStatusCode.OK, await IniciarEConsultarHealthAsync(api));
            using HttpClient cliente = api.CreateClient();
            using HttpResponseMessage parada = await cliente.GetAsync(new Uri("/api/v1/paradas?codigoExterno=SIM-P001", UriKind.Relative), Ct);
            Assert.Equal(HttpStatusCode.OK, parada.StatusCode);
            Assert.Contains("\"SIM-P001\"", await parada.Content.ReadAsStringAsync(Ct), StringComparison.Ordinal);
        }

        Assert.True(File.Exists(_arquivo));
        Assert.Equal(MigrationsDoProjeto(), await MigrationsAplicadasAsync());
        Assert.Equal(ContagensDoSeed, await ContagensAsync());
    }

    [Fact]
    public async Task Reinicio_NaoDuplicaNemAlteraOsDados()
    {
        await using (WebApplicationFactory<Program> primeira = Api("Development"))
        {
            Assert.Equal(HttpStatusCode.OK, await IniciarEConsultarHealthAsync(primeira));
        }

        string depoisDaPrimeira = await ConteudoAsync();

        await using (WebApplicationFactory<Program> segunda = Api("Development"))
        {
            Assert.Equal(HttpStatusCode.OK, await IniciarEConsultarHealthAsync(segunda));
        }

        Assert.Equal(depoisDaPrimeira, await ConteudoAsync());
        Assert.Equal(ContagensDoSeed, await ContagensAsync());
    }

    [Fact]
    public async Task BancoExistenteEAtualizado_NaoEAlterado()
    {
        // Um banco já criado e provisionado, com um dado mudado depois (como faria o operador).
        await using (BancoSqliteTeste banco = await BancoSqliteTeste.CriarAsync(ConnectionString, provisionarDemonstracao: true))
        {
            await using PontoVozDbContext contexto = banco.NovoContexto();
            await contexto.Database.ExecuteSqlRawAsync(
                "UPDATE \"Paradas\" SET \"Nome\" = 'Parada Renomeada Pelo Operador' WHERE \"CodigoExterno\" = 'SIM-P001'", Ct);
        }

        string antes = await ConteudoAsync();

        await using (WebApplicationFactory<Program> api = Api("Development"))
        {
            Assert.Equal(HttpStatusCode.OK, await IniciarEConsultarHealthAsync(api));
        }

        Assert.Equal(antes, await ConteudoAsync());
    }

    [Theory]
    [InlineData("Production", null)]
    [InlineData("Production", "true")]
    [InlineData("Development", "false")]
    public async Task ForaDeDevelopmentOuDesligada_NaoCriaOBanco(string ambiente, string? aplicarMigracoes)
    {
        await using (WebApplicationFactory<Program> api = Api(ambiente, aplicarMigracoes))
        {
            Assert.Equal(HttpStatusCode.ServiceUnavailable, await IniciarEConsultarHealthAsync(api));
        }

        Assert.False(File.Exists(_arquivo), $"Em {ambiente} (opção: {aplicarMigracoes ?? "padrão"}) o banco não pode ser criado.");
    }

    private static List<string> MigrationsDoProjeto()
    {
        using var contexto = new PontoVozDbContext(
            (DbContextOptions<PontoVozDbContext>)new DbContextOptionsBuilder<PontoVozDbContext>()
                .UsarSqlitePontoVoz("Data Source=:memory:", provisionamento: null).Options);
        return [.. contexto.Database.GetMigrations()];
    }

    private async Task<List<string>> MigrationsAplicadasAsync()
    {
        var aplicadas = new List<string>();
        await using SqliteConnection conexao = await AbrirSomenteLeituraAsync();
        await using SqliteCommand comando = conexao.CreateCommand();
        comando.CommandText = "SELECT \"MigrationId\" FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\"";
        await using SqliteDataReader leitor = await comando.ExecuteReaderAsync(Ct);
        while (await leitor.ReadAsync(Ct))
        {
            aplicadas.Add(leitor.GetString(0));
        }

        return aplicadas;
    }

    private async Task<Dictionary<string, long>> ContagensAsync()
    {
        var contagens = new Dictionary<string, long>();
        await using SqliteConnection conexao = await AbrirSomenteLeituraAsync();
        foreach (string tabela in ContagensDoSeed.Keys)
        {
            await using SqliteCommand comando = conexao.CreateCommand();
            comando.CommandText = $"SELECT COUNT(*) FROM \"{tabela}\"";
            contagens[tabela] = (long)(await comando.ExecuteScalarAsync(Ct))!;
        }

        return contagens;
    }

    /// <summary>Todas as linhas de todas as tabelas, em ordem, para comparar o banco antes e depois.</summary>
    private async Task<string> ConteudoAsync()
    {
        var conteudo = new StringBuilder();
        await using SqliteConnection conexao = await AbrirSomenteLeituraAsync();
        var tabelas = new List<string>();
        await using (SqliteCommand lista = conexao.CreateCommand())
        {
            lista.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%' ORDER BY name";
            await using SqliteDataReader leitor = await lista.ExecuteReaderAsync(Ct);
            while (await leitor.ReadAsync(Ct))
            {
                tabelas.Add(leitor.GetString(0));
            }
        }

        foreach (string tabela in tabelas)
        {
            conteudo.Append("# ").AppendLine(tabela);
            await using SqliteCommand comando = conexao.CreateCommand();
            comando.CommandText = $"SELECT * FROM \"{tabela}\" ORDER BY rowid";
            await using SqliteDataReader leitor = await comando.ExecuteReaderAsync(Ct);
            while (await leitor.ReadAsync(Ct))
            {
                for (int i = 0; i < leitor.FieldCount; i++)
                {
                    conteudo.Append(leitor.IsDBNull(i) ? "NULL" : Convert.ToString(leitor.GetValue(i), System.Globalization.CultureInfo.InvariantCulture)).Append('|');
                }

                conteudo.AppendLine();
            }
        }

        return conteudo.ToString();
    }

    private async Task<SqliteConnection> AbrirSomenteLeituraAsync()
    {
        var conexao = new SqliteConnection($"Data Source={_arquivo};Mode=ReadOnly");
        await conexao.OpenAsync(Ct);
        return conexao;
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        foreach (string arquivo in new[] { _arquivo, _arquivo + "-wal", _arquivo + "-shm" })
        {
            File.Delete(arquivo);
        }
    }
}

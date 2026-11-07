using System.Net;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

using PontoVoz.Tests.Infrastructure;

namespace PontoVoz.Tests.Api;

/// <summary>
/// Chama o <c>GET /health</c> pela API real, em memória, apontando para um arquivo SQLite
/// temporário e isolado em cada teste. O banco de desenvolvimento não é usado.
/// </summary>
public sealed class HealthEndpointTests : IDisposable
{
    private readonly string _arquivo = Path.Combine(Path.GetTempPath(), $"pontovoz-health-{Guid.NewGuid():N}.db");

    private static CancellationToken Ct => BancoSqliteTeste.Ct;

    private string ConnectionString => $"Data Source={_arquivo}";

    private WebApplicationFactory<Program> Api() =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:PontoVoz", ConnectionString));

    private async Task<(HttpStatusCode Status, string Corpo, JsonElement Json)> ObterHealthAsync()
    {
        await using WebApplicationFactory<Program> api = Api();
        using HttpClient cliente = api.CreateClient();
        using HttpResponseMessage resposta = await cliente.GetAsync(new Uri("/health", UriKind.Relative), Ct);
        string corpo = await resposta.Content.ReadAsStringAsync(Ct);
        return (resposta.StatusCode, corpo, JsonDocument.Parse(corpo).RootElement.Clone());
    }

    private static void VerificarBancoNaResposta(JsonElement json, string statusGeral, string statusBanco)
    {
        Assert.Equal(statusGeral, json.GetProperty("status").GetString());
        Assert.Equal("processo-e-dependencias", json.GetProperty("escopo").GetString());
        JsonElement verificacao = Assert.Single(json.GetProperty("verificacoes").EnumerateArray());
        Assert.Equal("banco-de-dados", verificacao.GetProperty("nome").GetString());
        Assert.Equal(statusBanco, verificacao.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Health_ComBancoMigrado_Responde200EIndicaOBancoSaudavel()
    {
        await using (await BancoSqliteTeste.CriarAsync(ConnectionString, provisionarDemonstracao: false))
        {
        }

        var (status, _, json) = await ObterHealthAsync();

        Assert.Equal(HttpStatusCode.OK, status);
        VerificarBancoNaResposta(json, "Healthy", "Healthy");
    }

    [Fact]
    public async Task Health_SemArquivoDeBanco_Responde503ENaoCriaOBanco()
    {
        var (status, corpo, json) = await ObterHealthAsync();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, status);
        VerificarBancoNaResposta(json, "Unhealthy", "Unhealthy");
        Assert.False(File.Exists(_arquivo), "O health check não pode criar o arquivo do banco.");
        Assert.DoesNotContain(Path.GetFileName(_arquivo), corpo);
        Assert.DoesNotContain("Data Source", corpo);
    }

    [Fact]
    public async Task Health_ComBancoSemMigrations_Responde503ENaoAplicaAsMigrations()
    {
        await using (var conexao = new SqliteConnection(ConnectionString))
        {
            await conexao.OpenAsync(Ct);
        }

        var (status, _, json) = await ObterHealthAsync();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, status);
        VerificarBancoNaResposta(json, "Unhealthy", "Unhealthy");
        await using var leitura = new SqliteConnection(ConnectionString);
        await leitura.OpenAsync(Ct);
        await using SqliteCommand comando = leitura.CreateCommand();
        comando.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'Paradas'";
        Assert.Equal(0L, await comando.ExecuteScalarAsync(Ct));
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

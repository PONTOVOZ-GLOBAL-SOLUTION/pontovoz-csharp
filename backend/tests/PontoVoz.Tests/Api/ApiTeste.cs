using System.Net.Http.Headers;
using System.Text.Json.Nodes;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Infrastructure.Persistence;
using PontoVoz.Tests.Infrastructure;

namespace PontoVoz.Tests.Api;

/// <summary>
/// A API real, em memória (<see cref="WebApplicationFactory{TEntryPoint}"/>), sobre um arquivo
/// SQLite temporário e isolado, com as migrations, o seed de demonstração e o registro manual
/// dos exemplos do contrato. O relógio da API fica parado no instante das consultas dos
/// exemplos (10/11/2026, 14h32 em São Paulo). O banco de desenvolvimento não é usado.
/// </summary>
public sealed class ApiTeste : IAsyncDisposable
{
    /// <summary>Instante das consultas dos exemplos do contrato.</summary>
    public static readonly DateTimeOffset ConsultaDosExemplos = new(2026, 11, 10, 14, 32, 0, TimeSpan.FromHours(-3));

    private readonly string _arquivo = Path.Combine(Path.GetTempPath(), $"pontovoz-http-{Guid.NewGuid():N}.db");

    private ApiTeste()
    {
    }

    public WebApplicationFactory<Program> Fabrica { get; private set; } = null!;

    public FakeTimeProvider Relogio { get; } = new(ConsultaDosExemplos);

    public static CancellationToken Ct => BancoSqliteTeste.Ct;

    /// <summary>Cria o banco do teste e a API. <paramref name="configurar"/> ajusta o host (configuração, serviços substitutos).</summary>
    public static async Task<ApiTeste> CriarAsync(Action<IWebHostBuilder>? configurar = null)
    {
        var api = new ApiTeste();
        string connectionString = $"Data Source={api._arquivo}";
        await using (BancoSqliteTeste banco = await BancoSqliteTeste.CriarAsync(connectionString, provisionarDemonstracao: true))
        {
            await RegistrarManualDoExemploAsync(banco);
        }

        api.Fabrica = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:PontoVoz", connectionString);
            builder.ConfigureTestServices(services => services.AddSingleton<TimeProvider>(api.Relogio));
            configurar?.Invoke(builder);
        });
        return api;
    }

    public HttpClient Cliente() => Fabrica.CreateClient();

    public async Task<Resposta> GetAsync(string caminho, string? origem = null)
    {
        using HttpClient cliente = Cliente();
        using var requisicao = new HttpRequestMessage(HttpMethod.Get, new Uri(caminho, UriKind.Relative));
        if (origem is not null)
        {
            requisicao.Headers.Add("Origin", origem);
        }

        using HttpResponseMessage resposta = await cliente.SendAsync(requisicao, Ct);
        string corpo = await resposta.Content.ReadAsStringAsync(Ct);
        return new Resposta(resposta.StatusCode, resposta.Content.Headers.ContentType, resposta.Headers, corpo);
    }

    /// <summary>O registro manual dos exemplos: SIM-L004 vista às 14h24 e registrada às 14h25min10s na SIM-P001.</summary>
    private static async Task RegistrarManualDoExemploAsync(BancoSqliteTeste banco)
    {
        await using PontoVozDbContext contexto = banco.NovoContexto();
        Parada centro = await contexto.Paradas.Include(p => p.LinhasAssociadas).ThenInclude(a => a.Linha).SingleAsync(p => p.CodigoExterno == "SIM-P001", Ct);
        Linha l004 = centro.LinhasAssociadas.Single(a => a.Linha.Codigo == "SIM-L004").Linha;
        DateTimeOffset observadaEm = new(2026, 11, 10, 14, 24, 0, TimeSpan.FromHours(-3));
        contexto.RegistrosPassagem.Add(PassagemRegistroManual.Registrar(centro, l004, observadaEm, null, "operador-local", observadaEm.AddSeconds(70)));
        await contexto.SaveChangesAsync(Ct);
    }

    public async ValueTask DisposeAsync()
    {
        if (Fabrica is not null)
        {
            await Fabrica.DisposeAsync();
        }

        SqliteConnection.ClearAllPools();
        foreach (string arquivo in new[] { _arquivo, _arquivo + "-wal", _arquivo + "-shm" })
        {
            File.Delete(arquivo);
        }
    }

    /// <summary>Resposta HTTP já lida: status, Content-Type, cabeçalhos e corpo.</summary>
    public sealed record Resposta(System.Net.HttpStatusCode Status, MediaTypeHeaderValue? ContentType, HttpResponseHeaders Cabecalhos, string Corpo)
    {
        public JsonNode Json => JsonNode.Parse(Corpo)!;

        public string? TipoMidia => ContentType?.MediaType;
    }
}

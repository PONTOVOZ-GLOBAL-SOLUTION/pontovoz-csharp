using System.Net.Http.Headers;
using System.Text.Json.Nodes;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

    private ApiTeste(DateTimeOffset inicioRelogio)
    {
        Relogio = new FakeTimeProvider(inicioRelogio);
    }

    public WebApplicationFactory<Program> Fabrica { get; private set; } = null!;

    /// <summary>Relógio da API. Por padrão começa em <see cref="ConsultaDosExemplos"/>; só anda para a frente.</summary>
    public FakeTimeProvider Relogio { get; }

    public static CancellationToken Ct => BancoSqliteTeste.Ct;

    /// <summary>
    /// Cria o banco do teste e a API. <paramref name="configurar"/> ajusta o host (configuração,
    /// serviços substitutos). <paramref name="chaveAdministrativa"/> é o <c>Admin:Chave</c> da API:
    /// sem valor, a chave fica vazia de propósito, para que um user-secret da máquina do
    /// desenvolvedor não mude o resultado dos testes. <paramref name="ambiente"/> é o ambiente do host.
    /// Com <paramref name="registrarManualDoExemplo"/> falso, o banco fica só com o seed, para o
    /// teste criar o registro manual pela própria rota administrativa.
    /// </summary>
    public static async Task<ApiTeste> CriarAsync(
        Action<IWebHostBuilder>? configurar = null,
        string? chaveAdministrativa = null,
        string ambiente = "Development",
        bool registrarManualDoExemplo = true,
        DateTimeOffset? inicioRelogio = null)
    {
        var api = new ApiTeste(inicioRelogio ?? ConsultaDosExemplos);
        string connectionString = $"Data Source={api._arquivo}";
        await using (BancoSqliteTeste banco = await BancoSqliteTeste.CriarAsync(connectionString, provisionarDemonstracao: true))
        {
            if (registrarManualDoExemplo)
            {
                await RegistrarManualDoExemploAsync(banco);
            }
        }

        api.Fabrica = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(ambiente);
            builder.UseSetting("ConnectionStrings:PontoVoz", connectionString);
            // O banco do teste já foi criado acima: a API não aplica migrations ao iniciar.
            builder.ConfigureAppConfiguration(configuracao => configuracao.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Admin:Chave"] = chaveAdministrativa ?? string.Empty,
                    ["Banco:AplicarMigracoesAoIniciar"] = "false",
                }));
            builder.ConfigureTestServices(services => services.AddSingleton<TimeProvider>(api.Relogio));
            configurar?.Invoke(builder);
        });
        return api;
    }

    public HttpClient Cliente() => Fabrica.CreateClient();

    /// <summary>Contexto novo sobre o arquivo do teste, para conferir o que a API gravou no SQLite.</summary>
    public PontoVozDbContext NovoContexto() =>
        new((DbContextOptions<PontoVozDbContext>)new DbContextOptionsBuilder<PontoVozDbContext>()
            .UsarSqlitePontoVoz($"Data Source={_arquivo}", provisionamento: null).Options);

    /// <summary>Envia uma requisição com corpo JSON (texto cru, para testar também JSON inválido) e cabeçalhos.</summary>
    public async Task<Resposta> EnviarAsync(HttpMethod metodo, string caminho, string? json = null, IReadOnlyDictionary<string, string>? cabecalhos = null, string tipoConteudo = "application/json")
    {
        using HttpClient cliente = Cliente();
        using var requisicao = new HttpRequestMessage(metodo, new Uri(caminho, UriKind.Relative));
        if (json is not null)
        {
            requisicao.Content = new StringContent(json, System.Text.Encoding.UTF8);
            requisicao.Content.Headers.ContentType = new MediaTypeHeaderValue(tipoConteudo);
        }

        foreach (var (nome, valor) in cabecalhos ?? new Dictionary<string, string>())
        {
            requisicao.Headers.Add(nome, valor);
        }

        using HttpResponseMessage resposta = await cliente.SendAsync(requisicao, Ct);
        string corpo = await resposta.Content.ReadAsStringAsync(Ct);
        return new Resposta(resposta.StatusCode, resposta.Content.Headers.ContentType, resposta.Headers, corpo);
    }

    /// <summary>Id de uma parada (ativa) pelo código, lido da própria API.</summary>
    public async Task<int> IdDaParadaAsync(string codigoExterno) =>
        (await GetAsync($"/api/v1/paradas?codigoExterno={codigoExterno}")).Json["itens"]!.AsArray().Single()!["id"]!.GetValue<int>();

    /// <summary>Id de uma linha (ativa) pelo código, lido da própria API.</summary>
    public async Task<int> IdDaLinhaAsync(string codigo) =>
        (await GetAsync("/api/v1/linhas?tamanhoPagina=50")).Json["itens"]!.AsArray().Single(l => l!["codigo"]!.GetValue<string>() == codigo)!["id"]!.GetValue<int>();

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

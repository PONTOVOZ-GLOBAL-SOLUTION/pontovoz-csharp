using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Infrastructure.Persistence;
using PontoVoz.Api.Infrastructure.Persistence.Seed;
using PontoVoz.Tests.Domain;

namespace PontoVoz.Tests.Infrastructure;

public class ProvisionamentoDemonstracaoTests
{
    private static CancellationToken Ct => BancoSqliteTeste.Ct;

    private sealed record Contagem(int Paradas, int Linhas, int Associacoes, int Destinos, int Percursos, int Passagens);

    private static async Task<Contagem> ContarAsync(PontoVozDbContext contexto) => new(
        await contexto.Paradas.CountAsync(Ct),
        await contexto.Linhas.CountAsync(Ct),
        await contexto.ParadasLinhas.CountAsync(Ct),
        await contexto.Destinos.CountAsync(Ct),
        await contexto.PercursosEstimados.CountAsync(Ct),
        await contexto.RegistrosPassagem.CountAsync(Ct));

    private static readonly Contagem Esperado = new(Paradas: 2, Linhas: 4, Associacoes: 5, Destinos: 3, Percursos: 5, Passagens: 2);

    [Fact]
    public async Task PrimeiroProvisionamento_CriaOsDadosDeDemonstracaoCoerentesComOContrato()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync(provisionarDemonstracao: true);
        await using PontoVozDbContext contexto = banco.NovoContexto();

        Assert.Equal(Esperado, await ContarAsync(contexto));
        Assert.Equal(21, DadosDemonstracao.Criar(BancoSqliteTeste.InicioRelogio).TotalRegistros);

        Parada centro = await contexto.Paradas
            .Include(p => p.LinhasAssociadas).ThenInclude(a => a.Linha)
            .SingleAsync(p => p.CodigoExterno == "SIM-P001", Ct);
        Assert.Equal(new[] { "SIM-L001", "SIM-L002", "SIM-L004" }, centro.LinhasAssociadas.Select(a => a.Linha.Codigo).Order());

        Destino norte = await contexto.Destinos.SingleAsync(d => d.Id == 3, Ct);
        Assert.Equal("Terminal Demonstração Norte", norte.Nome);

        List<int> percursosCentroNorte = await contexto.PercursosEstimados
            .Where(p => p.ParadaId == centro.Id && p.DestinoId == norte.Id)
            .OrderBy(p => p.MinutosEstimados)
            .Select(p => p.MinutosEstimados)
            .ToListAsync(Ct);
        Assert.Equal(new[] { 20, 30, 40 }, percursosCentroNorte);
    }

    [Fact]
    public async Task PrimeiroProvisionamento_SoCriaDadosFicticiosIdentificados()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync(provisionarDemonstracao: true);
        await using PontoVozDbContext contexto = banco.NovoContexto();

        Assert.All(await contexto.Paradas.ToListAsync(Ct), p => Assert.True(p.DadosSimulados && p.CodigoExterno.StartsWith("SIM-", StringComparison.Ordinal) && p.Endereco is null && p.Coordenada is null));
        Assert.All(await contexto.Linhas.ToListAsync(Ct), l => Assert.True(l.DadosSimulados && l.Codigo.StartsWith("SIM-", StringComparison.Ordinal) && l.Letreiro.StartsWith("SIM-", StringComparison.Ordinal)));
        Assert.All(await contexto.Destinos.ToListAsync(Ct), d => Assert.True(d.DadosSimulados));
        Assert.All(await contexto.PercursosEstimados.ToListAsync(Ct), p => Assert.True(p.DadosSimulados));
        Assert.All(await contexto.RegistrosPassagem.ToListAsync(Ct), p =>
        {
            Assert.IsType<PassagemSimulada>(p);
            Assert.False(p.Verificada);
            Assert.False(p.OficialSptrans);
            Assert.StartsWith("SIM-", p.PrefixoVeiculo);
        });
    }

    [Fact]
    public async Task PrimeiroProvisionamento_CalculaOsHorariosEmRelacaoAoInstanteDoProvisionamento()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync(provisionarDemonstracao: true);
        await using PontoVozDbContext contexto = banco.NovoContexto();
        DateTimeOffset instante = BancoSqliteTeste.InicioRelogio;

        List<PassagemSimulada> passagens = await contexto.RegistrosPassagem.OfType<PassagemSimulada>().OrderBy(p => p.ChaveSeed).ToListAsync(Ct);

        Assert.Equal(new[] { "SIM-OBS-001", "SIM-OBS-002" }, passagens.Select(p => p.ChaveSeed));
        Assert.Equal(new[] { instante.AddMinutes(-15), instante.AddMinutes(-25) }, passagens.Select(p => p.ObservadaEmUtc));
        Assert.All(passagens, p => Assert.Equal(instante, p.RegistradaEmUtc));
    }

    [Fact]
    public async Task SegundoProvisionamento_NaoDuplicaNada()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync(provisionarDemonstracao: true);

        await using (PontoVozDbContext contexto = banco.NovoContexto())
        {
            await contexto.Database.MigrateAsync(Ct);
        }

        await using PontoVozDbContext leitura = banco.NovoContexto();
        Assert.Equal(Esperado, await ContarAsync(leitura));
    }

    [Fact]
    public async Task Reinicio_DiasDepois_NaoCriaNemRenovaPassagens()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync(provisionarDemonstracao: true);
        List<(string Chave, DateTimeOffset Observada)> antes;
        await using (PontoVozDbContext contexto = banco.NovoContexto())
        {
            antes = await contexto.RegistrosPassagem.OfType<PassagemSimulada>().OrderBy(p => p.Id)
                .Select(p => new ValueTuple<string, DateTimeOffset>(p.ChaveSeed, p.ObservadaEmUtc)).ToListAsync(Ct);
        }

        banco.Relogio.Advance(TimeSpan.FromDays(2));
        await using (PontoVozDbContext reinicio = banco.NovoContexto())
        {
            await reinicio.Database.MigrateAsync(Ct);
        }

        await using PontoVozDbContext leitura = banco.NovoContexto();
        List<PassagemSimulada> depois = await leitura.RegistrosPassagem.OfType<PassagemSimulada>().OrderBy(p => p.Id).ToListAsync(Ct);
        DateTimeOffset agora = banco.Relogio.GetUtcNow();

        Assert.Equal(antes, depois.Select(p => (p.ChaveSeed, p.ObservadaEmUtc)));
        Assert.All(depois, p => Assert.True(p.EstaDesatualizada(agora)));
        Assert.Equal(Esperado, await ContarAsync(leitura));
    }

    [Fact]
    public async Task Reprovisionamento_PreservaRegistrosManuais()
    {
        await using var banco = await BancoSqliteTeste.CriarAsync(provisionarDemonstracao: true);
        DateTimeOffset registroManual = banco.Relogio.GetUtcNow().AddMinutes(30);
        int manualId;
        await using (PontoVozDbContext contexto = banco.NovoContexto())
        {
            Parada centro = await contexto.Paradas.Include(p => p.LinhasAssociadas).ThenInclude(a => a.Linha).SingleAsync(p => p.CodigoExterno == "SIM-P001", Ct);
            Linha l004 = centro.LinhasAssociadas.Single(a => a.Linha.Codigo == "SIM-L004").Linha;
            var manual = PassagemRegistroManual.Registrar(centro, l004, registroManual.AddMinutes(-2), "SIM-V4001", "operador-local", registroManual);
            contexto.RegistrosPassagem.Add(manual);
            await contexto.SaveChangesAsync(Ct);
            manualId = manual.Id;
        }

        banco.Relogio.Advance(TimeSpan.FromHours(3));
        await using (PontoVozDbContext reinicio = banco.NovoContexto())
        {
            await reinicio.Database.MigrateAsync(Ct);
        }

        await using PontoVozDbContext leitura = banco.NovoContexto();
        var preservado = Assert.IsType<PassagemRegistroManual>(await leitura.RegistrosPassagem.SingleAsync(p => p.Id == manualId, Ct));
        Assert.Equal(registroManual.AddMinutes(-2), preservado.ObservadaEmUtc);
        Assert.Equal("operador-local", preservado.OrigemRegistro);
        Assert.Equal(3, await leitura.RegistrosPassagem.CountAsync(Ct));
    }

    [Fact]
    public async Task FluxoSincrono_DeMigracaoTambemProvisionaUmaVez()
    {
        await using var conexao = new SqliteConnection("Data Source=:memory:");
        await conexao.OpenAsync(Ct);
        var provisionamento = new ProvisionamentoDemonstracao(new Microsoft.Extensions.Time.Testing.FakeTimeProvider(BancoSqliteTeste.InicioRelogio));
        PontoVozDbContext NovoContexto()
        {
            var options = new DbContextOptionsBuilder<PontoVozDbContext>();
            options.UsarSqlitePontoVoz(conexao, provisionamento);
            return new PontoVozDbContext(options.Options);
        }

        using (PontoVozDbContext primeiro = NovoContexto())
        {
            primeiro.Database.Migrate();
        }

        using (PontoVozDbContext segundo = NovoContexto())
        {
            segundo.Database.Migrate();
        }

        await using PontoVozDbContext leitura = NovoContexto();
        Assert.Equal(Esperado, await ContarAsync(leitura));
    }

    [Fact]
    public async Task BancoEmArquivo_ReaplicarAMigrationNaoPerdeDados()
    {
        string arquivo = Path.Combine(Path.GetTempPath(), $"pontovoz-teste-{Guid.NewGuid():N}.db");
        string connectionString = $"Data Source={arquivo}";
        try
        {
            await using (var banco = await BancoSqliteTeste.CriarAsync(connectionString, provisionarDemonstracao: true))
            await using (PontoVozDbContext contexto = banco.NovoContexto())
            {
                Parada centro = await contexto.Paradas.Include(p => p.LinhasAssociadas).ThenInclude(a => a.Linha).SingleAsync(p => p.CodigoExterno == "SIM-P001", Ct);
                Linha l001 = centro.LinhasAssociadas.Single(a => a.Linha.Codigo == "SIM-L001").Linha;
                DateTimeOffset agora = banco.Relogio.GetUtcNow();
                contexto.RegistrosPassagem.Add(PassagemRegistroManual.Registrar(centro, l001, agora, null, "operador-local", agora));
                await contexto.SaveChangesAsync(Ct);
            }

            SqliteConnection.ClearAllPools();

            await using var reaberto = await BancoSqliteTeste.CriarAsync(connectionString, provisionarDemonstracao: true);
            await using PontoVozDbContext leitura = reaberto.NovoContexto();

            Assert.Equal(Esperado with { Passagens = 3 }, await ContarAsync(leitura));
            Assert.Single(await leitura.Database.GetAppliedMigrationsAsync(Ct));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(arquivo);
        }
    }
}

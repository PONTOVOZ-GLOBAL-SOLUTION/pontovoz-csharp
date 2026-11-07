using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

using PontoVoz.Api.Application.Consultas;
using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Infrastructure.Persistence;
using PontoVoz.Api.Infrastructure.Persistence.Busca;
using PontoVoz.Api.Infrastructure.Repositories;
using PontoVoz.Tests.Domain;

namespace PontoVoz.Tests.Infrastructure;

/// <summary>
/// Migration <c>AddSearchColumns</c> sobre um banco que já tinha dados no esquema do B3, e a
/// manutenção da coluna de busca nas gravações seguintes.
/// </summary>
public sealed class MigracaoBuscaTests : IAsyncDisposable
{
    private const string InitialCreate = "20261107211456_InitialCreate";
    private const string AddSearchColumns = "20261107222533_AddSearchColumns";

    private readonly SqliteConnection _conexao = new("Data Source=:memory:");

    private static CancellationToken Ct => BancoSqliteTeste.Ct;

    private PontoVozDbContext NovoContexto()
    {
        var options = new DbContextOptionsBuilder<PontoVozDbContext>();
        options.UsarSqlitePontoVoz(_conexao, provisionamento: null);
        return new PontoVozDbContext(options.Options);
    }

    /// <summary>
    /// Cria o esquema só até a <c>InitialCreate</c>, pelo script SQL dela, e grava registros como o
    /// B3 gravava (sem <c>TextoBusca</c>).
    /// </summary>
    private async Task CriarBancoNoEsquemaDoB3Async()
    {
        await _conexao.OpenAsync(Ct);
        await using PontoVozDbContext contexto = NovoContexto();
        string script = contexto.GetService<IMigrator>().GenerateScript(fromMigration: null, toMigration: InitialCreate);
        await contexto.Database.ExecuteSqlRawAsync(script, Ct);
        await contexto.Database.ExecuteSqlRawAsync("""
            INSERT INTO Paradas (CodigoExterno, Nome, Endereco, Ativa, Origem, DadosSimulados, Latitude, Longitude, CriadoEmUtc)
            VALUES ('LEG-P001', 'Parada Conceição', 'Rua Antiga, 10', 1, 'CadastroAdministrativo', 0, -23.5, -46.6, 638000000000000000);
            INSERT INTO Linhas (Codigo, Letreiro, TerminalPrincipal, TerminalSecundario, Sentido, Ativa, Origem, DadosSimulados, CriadoEmUtc)
            VALUES ('LEG-L001', '875A-10', 'Terminal Ação', 'Estação Integração', 'SecundarioParaPrincipal', 1, 'CadastroAdministrativo', 0, 638000000000000000);
            INSERT INTO Destinos (Nome, Tipo, Ativo, Origem, DadosSimulados, CriadoEmUtc)
            VALUES ('Estação Integração', 'Estacao', 1, 'Simulado', 1, 638000000000000000);
            INSERT INTO ParadasLinhas (ParadaId, LinhaId, AssociadaEmUtc) VALUES (1, 1, 638000000000000000);
            INSERT INTO RegistrosPassagem (ParadaId, LinhaId, ObservadaEmUtc, RegistradaEmUtc, PrefixoVeiculo, TipoPassagem, DadosSimulados, OrigemRegistro, ChaveSeed, CriadoEmUtc)
            VALUES (1, 1, 638000000000000000, 638000000000000000, 'V-1', 'RegistroManual', 0, 'operador-local', NULL, 638000000000000000);
            """, Ct);
    }

    private async Task<List<string>> TextosBuscaAsync(PontoVozDbContext contexto) =>
        await contexto.Database.SqlQuery<string>(
            $"SELECT TextoBusca AS Value FROM Paradas UNION ALL SELECT TextoBusca FROM Linhas UNION ALL SELECT TextoBusca FROM Destinos").ToListAsync(Ct);

    [Fact]
    public async Task Migration_EmBancoComDadosAntigos_PreservaTudoEPreencheABusca()
    {
        await CriarBancoNoEsquemaDoB3Async();
        await using (PontoVozDbContext antes = NovoContexto())
        {
            Assert.Equal(new[] { AddSearchColumns }, await antes.Database.GetPendingMigrationsAsync(Ct));
        }

        await using (PontoVozDbContext migracao = NovoContexto())
        {
            await migracao.Database.MigrateAsync(Ct);
        }

        await using PontoVozDbContext leitura = NovoContexto();
        Parada parada = await leitura.Paradas.AsNoTracking().SingleAsync(Ct);
        Linha linha = await leitura.Linhas.AsNoTracking().SingleAsync(Ct);
        RegistroPassagem passagem = await leitura.RegistrosPassagem.AsNoTracking().SingleAsync(Ct);

        Assert.Empty(await leitura.Database.GetPendingMigrationsAsync(Ct));
        Assert.Equal(("LEG-P001", "Parada Conceição", "Rua Antiga, 10", -23.5), (parada.CodigoExterno, parada.Nome, parada.Endereco, parada.Coordenada!.Latitude));
        Assert.Equal(("Terminal Ação", "Estação Integração"), (linha.TerminalPrincipal, linha.TerminalSecundario));
        Assert.Equal(("operador-local", 638000000000000000L), (passagem.OrigemRegistro, passagem.ObservadaEmUtc.UtcTicks));
        Assert.Equal(1, await leitura.ParadasLinhas.CountAsync(Ct));
        Assert.Equal(
            new[] { "parada conceicao\nleg-p001", "leg-l001\n875a-10\nterminal acao\nestacao integracao", "estacao integracao" },
            await TextosBuscaAsync(leitura));
        Assert.Empty(await leitura.Database.SqlQuery<string>($"SELECT 'violação' AS Value FROM pragma_foreign_key_check").ToListAsync(Ct));

        Assert.Equal(1, (await new ParadaRepository(leitura).BuscarAtivasAsync("CONCEICAO", null, new Paginacao(), Ct)).TotalItens);
        Assert.Equal(1, (await new LinhaRepository(leitura).BuscarAtivasAsync("acao", new Paginacao(), Ct)).TotalItens);
        Assert.Equal(1, (await new DestinoRepository(leitura).BuscarAtivosAsync("integracao", new Paginacao(), Ct)).TotalItens);
    }

    [Fact]
    public async Task Migration_ReaplicadaNaoAlteraNada()
    {
        await CriarBancoNoEsquemaDoB3Async();
        await using (PontoVozDbContext primeira = NovoContexto())
        {
            await primeira.Database.MigrateAsync(Ct);
        }

        List<string> depoisDaPrimeira;
        await using (PontoVozDbContext leitura = NovoContexto())
        {
            depoisDaPrimeira = await TextosBuscaAsync(leitura);
        }

        await using (PontoVozDbContext segunda = NovoContexto())
        {
            await segunda.Database.MigrateAsync(Ct);
            Assert.Equal(0, await PreenchimentoTextoBusca.ExecutarAsync(segunda, Ct));
        }

        await using PontoVozDbContext final = NovoContexto();
        Assert.Equal(depoisDaPrimeira, await TextosBuscaAsync(final));
        Assert.Equal(new[] { InitialCreate, AddSearchColumns }, await final.Database.GetAppliedMigrationsAsync(Ct));
        Assert.Equal(1, await final.RegistrosPassagem.CountAsync(Ct));
    }

    [Fact]
    public async Task NovoCadastro_EAlteracaoDeNome_AtualizamABusca_SemMudarONomeExibido()
    {
        await _conexao.OpenAsync(Ct);
        await using (PontoVozDbContext criacao = NovoContexto())
        {
            await criacao.Database.MigrateAsync(Ct);
            criacao.Paradas.Add(Parada.CadastrarAdministrativa("PARADA-0001", "Parada São João", null, null, false, Cenario.Agora));
            await criacao.SaveChangesAsync(Ct);
        }

        await using (PontoVozDbContext alteracao = NovoContexto())
        {
            Parada parada = await alteracao.Paradas.SingleAsync(Ct);
            parada.AtualizarDados("Parada Praça da Sé", null, null);
            await alteracao.SaveChangesAsync(Ct);
        }

        await using PontoVozDbContext leitura = NovoContexto();
        var repositorio = new ParadaRepository(leitura);

        Assert.Equal(0, (await repositorio.BuscarAtivasAsync("sao joao", null, new Paginacao(), Ct)).TotalItens);
        Parada encontrada = Assert.Single((await repositorio.BuscarAtivasAsync("PRACA DA SE", null, new Paginacao(), Ct)).Itens);
        Assert.Equal("Parada Praça da Sé", encontrada.Nome);
        Assert.Equal(new[] { "parada praca da se\nparada-0001" }, await leitura.Database.SqlQuery<string>($"SELECT TextoBusca AS Value FROM Paradas").ToListAsync(Ct));
    }

    [Fact]
    public async Task Busca_TrataCuringasDoTermoComoTextoLiteral()
    {
        await _conexao.OpenAsync(Ct);
        await using (PontoVozDbContext criacao = NovoContexto())
        {
            await criacao.Database.MigrateAsync(Ct);
            criacao.Destinos.AddRange(
                Destino.CriarSimulado("Ponto 100% Demonstração", TipoDestino.PontoReferencia, Cenario.Agora),
                Destino.CriarSimulado("Ponto 1000 Demonstração", TipoDestino.PontoReferencia, Cenario.Agora));
            await criacao.SaveChangesAsync(Ct);
        }

        await using PontoVozDbContext leitura = NovoContexto();
        var repositorio = new DestinoRepository(leitura);

        Assert.Equal(new[] { "Ponto 100% Demonstração" }, (await repositorio.BuscarAtivosAsync("100%", new Paginacao(), Ct)).Itens.Select(d => d.Nome));
        Assert.Equal(2, (await repositorio.BuscarAtivosAsync("100", new Paginacao(), Ct)).TotalItens);
        Assert.Empty((await repositorio.BuscarAtivosAsync("10_0", new Paginacao(), Ct)).Itens);
    }

    public async ValueTask DisposeAsync() => await _conexao.DisposeAsync();
}

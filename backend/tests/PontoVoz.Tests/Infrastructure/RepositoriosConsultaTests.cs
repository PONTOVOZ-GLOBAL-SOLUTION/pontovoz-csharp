using Microsoft.EntityFrameworkCore;

using PontoVoz.Api.Application.Consultas;
using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Infrastructure.Persistence;
using PontoVoz.Api.Infrastructure.Repositories;
using PontoVoz.Tests.Domain;

namespace PontoVoz.Tests.Infrastructure;

/// <summary>
/// Repositórios de paradas, linhas e destinos sobre o SQLite real, com os dados do seed
/// (2 paradas, 4 linhas, 5 associações, 3 destinos e 5 percursos).
/// </summary>
public class RepositoriosConsultaTests
{
    private static CancellationToken Ct => BancoSqliteTeste.Ct;

    private static readonly Paginacao Primeira = new();

    private static Task<BancoSqliteTeste> BancoComSeedAsync() => BancoSqliteTeste.CriarAsync(provisionarDemonstracao: true);

    private static async Task ExecutarAsync(BancoSqliteTeste banco, FormattableString sql)
    {
        await using PontoVozDbContext contexto = banco.NovoContexto();
        await contexto.Database.ExecuteSqlInterpolatedAsync(sql, Ct);
    }

    private static async Task<int> IdDaParadaAsync(BancoSqliteTeste banco, string codigo)
    {
        await using PontoVozDbContext contexto = banco.NovoContexto();
        return await contexto.Paradas.Where(p => p.CodigoExterno == codigo).Select(p => p.Id).SingleAsync(Ct);
    }

    private static async Task<int> IdDaLinhaAsync(BancoSqliteTeste banco, string codigo)
    {
        await using PontoVozDbContext contexto = banco.NovoContexto();
        return await contexto.Linhas.Where(l => l.Codigo == codigo).Select(l => l.Id).SingleAsync(Ct);
    }

    private static async Task<int> IdDoDestinoAsync(BancoSqliteTeste banco, string nome)
    {
        await using PontoVozDbContext contexto = banco.NovoContexto();
        return await contexto.Destinos.Where(d => d.Nome == nome).Select(d => d.Id).SingleAsync(Ct);
    }

    private static async Task DesativarParadaAsync(BancoSqliteTeste banco, string codigo)
    {
        await using PontoVozDbContext contexto = banco.NovoContexto();
        Parada parada = await contexto.Paradas.SingleAsync(p => p.CodigoExterno == codigo, Ct);
        parada.Desativar();
        await contexto.SaveChangesAsync(Ct);
    }

    private static async Task DesativarLinhaAsync(BancoSqliteTeste banco, string codigo)
    {
        await using PontoVozDbContext contexto = banco.NovoContexto();
        Linha linha = await contexto.Linhas.SingleAsync(l => l.Codigo == codigo, Ct);
        linha.Desativar();
        await contexto.SaveChangesAsync(Ct);
    }

    // --- Paradas -----------------------------------------------------------------------------

    [Fact]
    public async Task Paradas_SemFiltro_ListaAsAtivasPorNomeEId_ComDadosFicticiosIdentificados()
    {
        await using var banco = await BancoComSeedAsync();
        await using PontoVozDbContext contexto = banco.NovoContexto();

        Pagina<Parada> pagina = await new ParadaRepository(contexto).BuscarAtivasAsync(null, null, Primeira, Ct);

        Assert.Equal(new[] { "Parada Demonstração Avenida", "Parada Demonstração Centro" }, pagina.Itens.Select(p => p.Nome));
        Assert.All(pagina.Itens, p => Assert.True(p.DadosSimulados && p.Origem == OrigemDado.Simulado));
        Assert.Equal((1, 20, 2, 1), (pagina.NumeroPagina, pagina.TamanhoPagina, pagina.TotalItens, pagina.TotalPaginas));
    }

    [Fact]
    public async Task Paradas_Inativas_FicamForaDaBuscaEDaConsultaPorId()
    {
        await using var banco = await BancoComSeedAsync();
        int idAvenida = await IdDaParadaAsync(banco, "SIM-P002");
        await DesativarParadaAsync(banco, "SIM-P002");
        await using PontoVozDbContext contexto = banco.NovoContexto();
        var repositorio = new ParadaRepository(contexto);

        Pagina<Parada> pagina = await repositorio.BuscarAtivasAsync(null, null, Primeira, Ct);

        Assert.Equal(new[] { "SIM-P001" }, pagina.Itens.Select(p => p.CodigoExterno));
        Assert.Null(await repositorio.ObterAtivaAsync(idAvenida, Ct));
        Assert.Equal(0, (await repositorio.BuscarAtivasAsync(null, "SIM-P002", Primeira, Ct)).TotalItens);
    }

    [Theory]
    [InlineData("SIM-P001")]
    [InlineData("sim-p001")]
    [InlineData("  Sim-P001 ")]
    public async Task Paradas_PorCodigoExterno_SaoExatasSemDiferenciarMaiusculas(string codigo)
    {
        await using var banco = await BancoComSeedAsync();
        await using PontoVozDbContext contexto = banco.NovoContexto();

        Pagina<Parada> pagina = await new ParadaRepository(contexto).BuscarAtivasAsync(null, codigo, Primeira, Ct);

        Parada parada = Assert.Single(pagina.Itens);
        Assert.Equal("Parada Demonstração Centro", parada.Nome);
    }

    [Fact]
    public async Task Paradas_PorCodigoExterno_ParcialNaoEncontra()
    {
        await using var banco = await BancoComSeedAsync();
        await using PontoVozDbContext contexto = banco.NovoContexto();

        Pagina<Parada> pagina = await new ParadaRepository(contexto).BuscarAtivasAsync(null, "SIM-P00", Primeira, Ct);

        Assert.Empty(pagina.Itens);
        Assert.Equal((0, 0), (pagina.TotalItens, pagina.TotalPaginas));
    }

    [Theory]
    [InlineData("demonstracao", 2)]
    [InlineData("Demonstração", 2)]
    [InlineData("DEMONSTRAÇÃO", 2)]
    [InlineData("  demonstração   centro ", 1)]
    [InlineData("Demonstração Avenida", 1)]
    [InlineData("p002", 1)]
    [InlineData("terminal", 0)]
    [InlineData("inexistente", 0)]
    public async Task Paradas_PorTermo_IgnoramAcentosMaiusculasEEspacosExtras(string termo, int encontradas)
    {
        await using var banco = await BancoComSeedAsync();
        await using PontoVozDbContext contexto = banco.NovoContexto();

        Pagina<Parada> pagina = await new ParadaRepository(contexto).BuscarAtivasAsync(termo, null, Primeira, Ct);

        Assert.Equal(encontradas, pagina.TotalItens);
        Assert.Equal(encontradas, pagina.Itens.Count);
    }

    [Fact]
    public async Task Paradas_TermoECodigo_SeSomam()
    {
        await using var banco = await BancoComSeedAsync();
        await using PontoVozDbContext contexto = banco.NovoContexto();
        var repositorio = new ParadaRepository(contexto);

        Assert.Equal(1, (await repositorio.BuscarAtivasAsync("centro", "SIM-P001", Primeira, Ct)).TotalItens);
        Assert.Equal(0, (await repositorio.BuscarAtivasAsync("avenida", "SIM-P001", Primeira, Ct)).TotalItens);
    }

    [Fact]
    public async Task Paradas_Paginacao_RodaNoBancoComOrdemEstavel()
    {
        await using var banco = await BancoComSeedAsync();
        await using (PontoVozDbContext escrita = banco.NovoContexto())
        {
            // Nomes iguais: o desempate pelo id precisa manter a ordem entre as páginas.
            for (int i = 1; i <= 5; i++)
            {
                escrita.Paradas.Add(Parada.CadastrarAdministrativa($"TESTE-P10{i}", "Parada Repetida", null, null, true, Cenario.Agora));
            }

            await escrita.SaveChangesAsync(Ct);
        }

        await using PontoVozDbContext contexto = banco.NovoContexto();
        var repositorio = new ParadaRepository(contexto);
        banco.LimparComandosSql();

        Pagina<Parada> segunda = await repositorio.BuscarAtivasAsync("repetida", null, new Paginacao(2, 2), Ct);
        Pagina<Parada> alem = await repositorio.BuscarAtivasAsync("repetida", null, new Paginacao(4, 2), Ct);
        List<Parada> todas = [.. (await repositorio.BuscarAtivasAsync("repetida", null, new Paginacao(1, 50), Ct)).Itens];

        Assert.Equal(todas.Skip(2).Take(2).Select(p => p.Id), segunda.Itens.Select(p => p.Id));
        Assert.Equal(todas.OrderBy(p => p.Id).Select(p => p.Id), todas.Select(p => p.Id));
        Assert.Equal((2, 2, 5, 3), (segunda.NumeroPagina, segunda.Itens.Count, segunda.TotalItens, segunda.TotalPaginas));
        Assert.Empty(alem.Itens);
        Assert.Equal(5, alem.TotalItens);

        string consultaDaPagina = banco.ComandosSql.First(sql => sql.Contains("LIMIT", StringComparison.Ordinal) && sql.Contains("OFFSET", StringComparison.Ordinal));
        Assert.Contains("\"p\".\"Ativa\"", consultaDaPagina);
        Assert.Contains("LIKE", consultaDaPagina);
        Assert.Contains("ORDER BY \"p\".\"Nome\", \"p\".\"Id\"", consultaDaPagina);
        Assert.Contains(banco.ComandosSql, sql => sql.Contains("SELECT COUNT(*)", StringComparison.Ordinal) && sql.Contains("LIKE", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Paradas_ConsultaPorId_InexistenteDevolveNulo()
    {
        await using var banco = await BancoComSeedAsync();
        await using PontoVozDbContext contexto = banco.NovoContexto();
        var repositorio = new ParadaRepository(contexto);
        int id = await IdDaParadaAsync(banco, "SIM-P001");

        Assert.Equal("SIM-P001", (await repositorio.ObterAtivaAsync(id, Ct))?.CodigoExterno);
        Assert.Null(await repositorio.ObterAtivaAsync(9999, Ct));
    }

    // --- Linhas e associações ----------------------------------------------------------------

    [Theory]
    [InlineData("104", new[] { "SIM-L004" })]
    [InlineData("sim-l00", new[] { "SIM-L001", "SIM-L002", "SIM-L003", "SIM-L004" })]
    [InlineData("norte", new[] { "SIM-L001", "SIM-L002", "SIM-L004" })]
    [InlineData("DEMONSTRAÇÃO SUL", new[] { "SIM-L003" })]
    [InlineData("inexistente", new string[0])]
    public async Task Linhas_PorTermo_ProcuramEmCodigoLetreiroETerminais(string termo, string[] codigos)
    {
        await using var banco = await BancoComSeedAsync();
        await using PontoVozDbContext contexto = banco.NovoContexto();

        Pagina<Linha> pagina = await new LinhaRepository(contexto).BuscarAtivasAsync(termo, Primeira, Ct);

        Assert.Equal(codigos, pagina.Itens.Select(l => l.Codigo));
    }

    [Fact]
    public async Task Linhas_ComTermoQueAtravessaDoisCampos_NaoEncontra()
    {
        await using var banco = await BancoComSeedAsync();
        await using PontoVozDbContext contexto = banco.NovoContexto();

        // "sim-104 terminal" junta o fim do letreiro com o início de um terminal.
        Pagina<Linha> pagina = await new LinhaRepository(contexto).BuscarAtivasAsync("sim-104 terminal", Primeira, Ct);

        Assert.Empty(pagina.Itens);
    }

    [Fact]
    public async Task Linhas_DaParada_SaoAsAssociadasPersistidasEAtivas_ComSentido()
    {
        await using var banco = await BancoComSeedAsync();
        int centro = await IdDaParadaAsync(banco, "SIM-P001");
        int avenida = await IdDaParadaAsync(banco, "SIM-P002");
        await using PontoVozDbContext contexto = banco.NovoContexto();
        var repositorio = new LinhaRepository(contexto);

        IReadOnlyList<Linha> doCentro = await repositorio.ListarAtivasDaParadaAsync(centro, Ct);
        IReadOnlyList<Linha> daAvenida = await repositorio.ListarAtivasDaParadaAsync(avenida, Ct);

        Assert.Equal(new[] { "SIM-L001", "SIM-L002", "SIM-L004" }, doCentro.Select(l => l.Codigo));
        Assert.Equal(new[] { "SIM-L002", "SIM-L003" }, daAvenida.Select(l => l.Codigo));
        Assert.All(doCentro, l => Assert.Equal(SentidoLinha.PrincipalParaSecundario, l.Sentido));
        Assert.Empty(await repositorio.ListarAtivasDaParadaAsync(9999, Ct));
    }

    [Fact]
    public async Task Linhas_Inativas_SaemDaBuscaDaParadaEDaConsultaPorId()
    {
        await using var banco = await BancoComSeedAsync();
        int centro = await IdDaParadaAsync(banco, "SIM-P001");
        int l002 = await IdDaLinhaAsync(banco, "SIM-L002");
        await DesativarLinhaAsync(banco, "SIM-L002");
        await using PontoVozDbContext contexto = banco.NovoContexto();
        var repositorio = new LinhaRepository(contexto);

        Assert.Equal(new[] { "SIM-L001", "SIM-L004" }, (await repositorio.ListarAtivasDaParadaAsync(centro, Ct)).Select(l => l.Codigo));
        Assert.DoesNotContain("SIM-L002", (await repositorio.BuscarAtivasAsync(null, Primeira, Ct)).Itens.Select(l => l.Codigo));
        Assert.Null(await repositorio.ObterAtivaAsync(l002, Ct));
    }

    [Fact]
    public async Task AtendeParada_ExigeAAssociacaoPersistida()
    {
        await using var banco = await BancoComSeedAsync();
        int centro = await IdDaParadaAsync(banco, "SIM-P001");
        int l004 = await IdDaLinhaAsync(banco, "SIM-L004");
        int l003 = await IdDaLinhaAsync(banco, "SIM-L003");
        await using PontoVozDbContext contexto = banco.NovoContexto();
        var repositorio = new LinhaRepository(contexto);

        Assert.True(await repositorio.AtendeParadaAsync(centro, l004, Ct));
        Assert.False(await repositorio.AtendeParadaAsync(centro, l003, Ct));
    }

    // --- Destinos e percursos ----------------------------------------------------------------

    [Theory]
    [InlineData("norte", new[] { "Terminal Demonstração Norte" })]
    [InlineData("TERMINAL DEMONSTRAÇÃO", new[] { "Terminal Demonstração Centro", "Terminal Demonstração Norte", "Terminal Demonstração Sul" })]
    [InlineData("estacao", new string[0])]
    public async Task Destinos_PorTermo_OrdenadosPorNome(string termo, string[] nomes)
    {
        await using var banco = await BancoComSeedAsync();
        await using PontoVozDbContext contexto = banco.NovoContexto();

        Pagina<Destino> pagina = await new DestinoRepository(contexto).BuscarAtivosAsync(termo, Primeira, Ct);

        Assert.Equal(nomes, pagina.Itens.Select(d => d.Nome));
    }

    [Fact]
    public async Task Destinos_InexistenteOuInativo_NaoSaoEncontrados()
    {
        await using var banco = await BancoComSeedAsync();
        int norte = await IdDoDestinoAsync(banco, "Terminal Demonstração Norte");
        await using (PontoVozDbContext contexto = banco.NovoContexto())
        {
            Assert.Equal("Terminal Demonstração Norte", (await new DestinoRepository(contexto).ObterAtivoAsync(norte, Ct))?.Nome);
            Assert.Null(await new DestinoRepository(contexto).ObterAtivoAsync(9999, Ct));
        }

        // O contrato não tem rota para desativar destinos; o SQL simula um cadastro inativo.
        await ExecutarAsync(banco, $"UPDATE Destinos SET Ativo = 0 WHERE Id = {norte}");

        await using PontoVozDbContext leitura = banco.NovoContexto();
        var repositorio = new DestinoRepository(leitura);
        Assert.Null(await repositorio.ObterAtivoAsync(norte, Ct));
        Assert.Empty((await repositorio.BuscarAtivosAsync("norte", Primeira, Ct)).Itens);
    }

    [Fact]
    public async Task Percursos_Diretos_TrazemAsLinhasEOsMinutos_PorTempoECodigo()
    {
        await using var banco = await BancoComSeedAsync();
        int centro = await IdDaParadaAsync(banco, "SIM-P001");
        int norte = await IdDoDestinoAsync(banco, "Terminal Demonstração Norte");
        await using PontoVozDbContext contexto = banco.NovoContexto();

        IReadOnlyList<PercursoEstimado> percursos = await new DestinoRepository(contexto).ListarPercursosDiretosAsync(centro, norte, Ct);

        Assert.Equal(new[] { ("SIM-L004", 20), ("SIM-L001", 30), ("SIM-L002", 40) }, percursos.Select(p => (p.Linha.Codigo, p.MinutosEstimados)));
        Assert.All(percursos, p => Assert.Equal(OrigemDado.Simulado, p.Origem));
    }

    [Fact]
    public async Task Percursos_SemCadastro_DevolveListaVazia()
    {
        await using var banco = await BancoComSeedAsync();
        int centro = await IdDaParadaAsync(banco, "SIM-P001");
        int sul = await IdDoDestinoAsync(banco, "Terminal Demonstração Sul");
        await using PontoVozDbContext contexto = banco.NovoContexto();

        Assert.Empty(await new DestinoRepository(contexto).ListarPercursosDiretosAsync(centro, sul, Ct));
    }

    [Fact]
    public async Task Percursos_DeLinhaOuParadaInativa_SaemDaLista()
    {
        await using var banco = await BancoComSeedAsync();
        int centro = await IdDaParadaAsync(banco, "SIM-P001");
        int norte = await IdDoDestinoAsync(banco, "Terminal Demonstração Norte");
        await DesativarLinhaAsync(banco, "SIM-L004");

        await using (PontoVozDbContext contexto = banco.NovoContexto())
        {
            IReadOnlyList<PercursoEstimado> percursos = await new DestinoRepository(contexto).ListarPercursosDiretosAsync(centro, norte, Ct);
            Assert.Equal(new[] { "SIM-L001", "SIM-L002" }, percursos.Select(p => p.Linha.Codigo));
        }

        await DesativarParadaAsync(banco, "SIM-P001");
        await using PontoVozDbContext leitura = banco.NovoContexto();
        Assert.Empty(await new DestinoRepository(leitura).ListarPercursosDiretosAsync(centro, norte, Ct));
    }

    // --- Cancelamento ------------------------------------------------------------------------

    [Fact]
    public async Task Consultas_ComTokenCancelado_SaoInterrompidas()
    {
        await using var banco = await BancoComSeedAsync();
        await using PontoVozDbContext contexto = banco.NovoContexto();
        using var cancelamento = new CancellationTokenSource();
        await cancelamento.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new ParadaRepository(contexto).BuscarAtivasAsync("demonstracao", null, Primeira, cancelamento.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new RegistroPassagemRepository(contexto).ListarUltimasPorLinhaAsync(1, BancoSqliteTeste.InicioRelogio, null, cancelamento.Token));
    }
}

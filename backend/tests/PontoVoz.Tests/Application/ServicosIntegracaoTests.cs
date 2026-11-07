using Microsoft.EntityFrameworkCore;

using PontoVoz.Api.Application.Consultas;
using PontoVoz.Api.Application.Dtos;
using PontoVoz.Api.Application.Services;
using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Infrastructure.Persistence;
using PontoVoz.Api.Infrastructure.Repositories;
using PontoVoz.Tests.Infrastructure;

namespace PontoVoz.Tests.Application;

/// <summary>
/// Services com os repositórios reais sobre SQLite, com o seed de demonstração. O seed é
/// provisionado em 08/11/2026 às 09h00 de São Paulo, como nos exemplos do contrato, e as
/// respostas são comparadas campo a campo com <c>contracts/exemplos/</c> (sem os ids internos).
/// </summary>
public class ServicosIntegracaoTests
{
    /// <summary>Instante das consultas dos exemplos: 10/11/2026 às 14h32 em São Paulo.</summary>
    private static readonly DateTimeOffset ConsultaDosExemplos = new(2026, 11, 10, 14, 32, 0, TimeSpan.FromHours(-3));

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static async Task<(BancoSqliteTeste Banco, PontoVozDbContext Contexto)> BancoAsync()
    {
        var banco = await BancoSqliteTeste.CriarAsync(provisionarDemonstracao: true);
        return (banco, banco.NovoContexto());
    }

    private static UltimasPassagensService Passagens(PontoVozDbContext contexto, TimeProvider relogio) =>
        new(new ParadaRepository(contexto), new LinhaRepository(contexto), new RegistroPassagemRepository(contexto), relogio);

    private static async Task<int> IdDaParadaAsync(PontoVozDbContext contexto, string codigo) =>
        await contexto.Paradas.Where(p => p.CodigoExterno == codigo).Select(p => p.Id).SingleAsync(Ct);

    /// <summary>O registro manual dos exemplos: SIM-L004 vista às 14h24 e registrada às 14h25min10s.</summary>
    private static async Task RegistrarManualDoExemploAsync(BancoSqliteTeste banco)
    {
        await using PontoVozDbContext contexto = banco.NovoContexto();
        Parada centro = await contexto.Paradas.Include(p => p.LinhasAssociadas).ThenInclude(a => a.Linha).SingleAsync(p => p.CodigoExterno == "SIM-P001", Ct);
        Linha l004 = centro.LinhasAssociadas.Single(a => a.Linha.Codigo == "SIM-L004").Linha;
        DateTimeOffset observadaEm = new(2026, 11, 10, 14, 24, 0, TimeSpan.FromHours(-3));
        contexto.RegistrosPassagem.Add(PassagemRegistroManual.Registrar(centro, l004, observadaEm, null, "operador-local", observadaEm.AddSeconds(70)));
        await contexto.SaveChangesAsync(Ct);
    }

    [Fact]
    public async Task UltimasPassagens_DoBancoReal_SaoIguaisAoExemploDoContrato()
    {
        var (banco, contexto) = await BancoAsync();
        await using var _ = banco;
        await using var __ = contexto;
        await RegistrarManualDoExemploAsync(banco);
        banco.Relogio.SetUtcNow(ConsultaDosExemplos);

        UltimasPassagensParadaDto resposta = await Passagens(contexto, banco.Relogio).ObterAsync(await IdDaParadaAsync(contexto, "SIM-P001"), null, Ct);

        ExemplosContrato.AssertIgualAoExemplo(ExemplosContrato.Exemplo("ultimas-passagens.json"), resposta);
    }

    [Fact]
    public async Task Paradas_Linhas_EDestinos_DoBancoReal_SaoIguaisAosExemplosDoContrato()
    {
        var (banco, contexto) = await BancoAsync();
        await using var _ = banco;
        await using var __ = contexto;
        var paradas = new ParadaService(new ParadaRepository(contexto), new LinhaRepository(contexto));

        ExemplosContrato.AssertIgualAoExemplo(ExemplosContrato.Exemplo("paradas-listagem.json"),
            await paradas.BuscarAsync(new ConsultaParadas(Termo: "demonstracao"), Ct));
        ExemplosContrato.AssertIgualAoExemplo(ExemplosContrato.Exemplo("paradas-busca-codigo.json"),
            await paradas.BuscarAsync(new ConsultaParadas(CodigoExterno: "SIM-P001"), Ct));
        ExemplosContrato.AssertIgualAoExemplo(ExemplosContrato.Exemplo("parada-detalhe.json"),
            await paradas.ObterDetalheAsync(await IdDaParadaAsync(contexto, "SIM-P001"), Ct));
        ExemplosContrato.AssertIgualAoExemplo(ExemplosContrato.Exemplo("destinos-busca.json"),
            await new DestinoService(new DestinoRepository(contexto)).BuscarAsync(new ConsultaPaginada(Termo: "norte"), Ct));
        ExemplosContrato.AssertIgualAoExemplo(ExemplosContrato.BlocoDoDocumento("Exemplo de `GET /api/v1/linhas?termo=104`"),
            await new LinhaService(new LinhaRepository(contexto)).BuscarAsync(new ConsultaPaginada(Termo: "104"), Ct));
    }

    [Theory]
    [InlineData("demonstracao", 2)]
    [InlineData("DEMONSTRAÇÃO", 2)]
    [InlineData("  Demonstração   Centro ", 1)]
    [InlineData("Demonstração Avenida", 1)]
    [InlineData("inexistente", 0)]
    public async Task Paradas_BuscaSemAcentos_PeloService(string termo, int encontradas)
    {
        var (banco, contexto) = await BancoAsync();
        await using var _ = banco;
        await using var __ = contexto;

        PaginaDto<ParadaDto> pagina = await new ParadaService(new ParadaRepository(contexto), new LinhaRepository(contexto)).BuscarAsync(new ConsultaParadas(Termo: termo), Ct);

        Assert.Equal(encontradas, pagina.TotalItens);
        Assert.All(pagina.Itens, p => Assert.Contains("Demonstração", p.Nome));
    }

    [Fact]
    public async Task UltimasPassagens_RegistroAindaNoFuturo_FicaDeFora()
    {
        var (banco, contexto) = await BancoAsync();
        await using var _ = banco;
        await using var __ = contexto;
        int centro = await IdDaParadaAsync(contexto, "SIM-P001");
        await using (PontoVozDbContext escrita = banco.NovoContexto())
        {
            Parada parada = await escrita.Paradas.Include(p => p.LinhasAssociadas).ThenInclude(a => a.Linha).SingleAsync(p => p.Id == centro, Ct);
            Linha l001 = parada.LinhasAssociadas.Single(a => a.Linha.Codigo == "SIM-L001").Linha;
            DateTimeOffset agora = banco.Relogio.GetUtcNow();
            escrita.RegistrosPassagem.Add(PassagemRegistroManual.Registrar(parada, l001, agora.AddSeconds(90), null, "operador-local", agora));
            await escrita.SaveChangesAsync(Ct);
        }

        int l001Id = await contexto.Linhas.Where(l => l.Codigo == "SIM-L001").Select(l => l.Id).SingleAsync(Ct);
        UltimasPassagensParadaDto agoraMesmo = await Passagens(contexto, banco.Relogio).ObterAsync(centro, l001Id, Ct);
        banco.Relogio.Advance(TimeSpan.FromSeconds(90));
        UltimasPassagensParadaDto depois = await Passagens(contexto, banco.Relogio).ObterAsync(centro, l001Id, Ct);

        Assert.Empty(agoraMesmo.Passagens);
        Assert.Equal(new[] { "SIM-L001" }, agoraMesmo.LinhasSemRegistro.Select(l => l.CodigoLinha));
        PassagemDto passagem = Assert.Single(depois.Passagens);
        Assert.Equal((0, false), (passagem.MinutosDesdeObservacao, passagem.Desatualizada));
    }

    [Fact]
    public async Task Services_ComTokenCancelado_SaoInterrompidos()
    {
        var (banco, contexto) = await BancoAsync();
        await using var _ = banco;
        await using var __ = contexto;
        using var cancelamento = new CancellationTokenSource();
        await cancelamento.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new ParadaService(new ParadaRepository(contexto), new LinhaRepository(contexto)).BuscarAsync(new ConsultaParadas(), cancelamento.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            Passagens(contexto, banco.Relogio).ObterAsync(1, null, cancelamento.Token));
    }
}

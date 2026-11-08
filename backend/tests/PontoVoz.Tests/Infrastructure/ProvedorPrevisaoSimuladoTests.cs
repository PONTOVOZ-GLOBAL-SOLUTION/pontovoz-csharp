using System.Globalization;

using Microsoft.Extensions.Options;

using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.ValueObjects;
using PontoVoz.Api.Infrastructure.Providers;
using PontoVoz.Tests.Domain;

namespace PontoVoz.Tests.Infrastructure;

/// <summary>Configuração fixa para testes: o <see cref="IOptionsMonitor{TOptions}"/> sempre devolve o mesmo valor.</summary>
internal sealed class MonitorFixo<T>(T valor) : IOptionsMonitor<T>
{
    public T CurrentValue => valor;

    public T Get(string? name) => valor;

    public IDisposable? OnChange(Action<T, string?> listener) => null;
}

/// <summary>
/// Provedor simulado (requisitos R1 e R2): grade de chegadas calculada no instante da consulta,
/// sem data fixa, sem previsões vencidas e sem gravar nada. O relógio é sempre um instante
/// informado pelo teste, nunca o relógio real.
/// </summary>
public sealed class ProvedorPrevisaoSimuladoTests
{
    private static readonly TimeSpan SaoPaulo = TimeSpan.FromHours(-3);
    private static readonly DateTimeOffset ConsultaDoExemplo = new(2026, 11, 10, 14, 32, 0, SaoPaulo);

    private readonly Parada _centro = Cenario.ParadaSimulada().ComId(1);
    private readonly Linha _l001 = Cenario.LinhaSimulada("SIM-L001", "SIM-101").ComId(1);
    private readonly Linha _l002 = Cenario.LinhaSimulada("SIM-L002", "SIM-102").ComId(2);
    private readonly Linha _l003 = Cenario.LinhaSimulada("SIM-L003", "SIM-203").ComId(3);
    private readonly Linha _l004 = Cenario.LinhaSimulada("SIM-L004", "SIM-104").ComId(4);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static ProvedorPrevisaoSimulado Provedor(bool indisponivel = false) =>
        new(new MonitorFixo<OpcoesPrevisoes>(new OpcoesPrevisoes { SimularIndisponibilidade = indisponivel }));

    private static DateTimeOffset Local(int ano, int mes, int dia, int hora, int minuto, int segundo = 0) =>
        new(ano, mes, dia, hora, minuto, segundo, SaoPaulo);

    private Task<ResultadoProvedor> ConsultarAsync(DateTimeOffset referencia, params Linha[] linhas) =>
        Provedor().ConsultarAsync(_centro, linhas.Length > 0 ? linhas : [_l001, _l002, _l004], referencia, Ct);

    private static string Horario(PrevisaoChegada previsao) =>
        previsao.PrevistaParaUtc.ToOffset(SaoPaulo).ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

    [Fact]
    public async Task NoInstanteDoExemplo_ReproduzAsChegadasDoContrato()
    {
        ResultadoProvedor resultado = await ConsultarAsync(ConsultaDoExemplo);

        Assert.Equal(StatusPrevisao.Disponivel, resultado.Status);
        Assert.Equal(OrigemDado.Simulado, resultado.Origem);
        Assert.Equal(
            [
                ("SIM-L002", "10/11/2026 14:33:00", "SIM-V2001", 1),
                ("SIM-L002", "10/11/2026 14:51:00", "SIM-V2002", 19),
                ("SIM-L004", "10/11/2026 14:40:00", "SIM-V4001", 8),
            ],
            resultado.Previsoes.Select(p => (p.Linha.CodigoLinha, Horario(p), p.PrefixoVeiculo!, p.EsperaMinutos(ConsultaDoExemplo))));
        Assert.All(resultado.Previsoes, p => Assert.Equal(NivelConfirmacao.Previsao, p.NivelConfirmacao));
        Assert.DoesNotContain(resultado.Previsoes, p => p.Linha.CodigoLinha == "SIM-L001");
    }

    /// <summary>A mesma regra vale em qualquer data: horários futuros, espaçamento fixo e veículos em rodízio.</summary>
    [Theory]
    [InlineData("2026-11-10T14:32:00-03:00")]
    [InlineData("2026-11-11T09:00:00-03:00")]
    [InlineData("2027-03-15T06:07:59-03:00")]
    [InlineData("2028-02-29T12:00:01-03:00")]
    [InlineData("2030-07-01T00:00:00-03:00")]
    [InlineData("2026-12-31T23:59:59-03:00")]
    public void Grade_EmQualquerData_TemChegadasFuturasEspacadasPeloIntervalo(string instante)
    {
        DateTimeOffset referencia = DateTimeOffset.Parse(instante, CultureInfo.InvariantCulture);

        foreach (LinhaSimulada configuracao in ProvedorPrevisaoSimulado.LinhasDemonstracao.Values)
        {
            List<PrevisaoChegada> chegadas = [.. ProvedorPrevisaoSimulado.ProximasChegadas(configuracao, new ReferenciaLinha(9, configuracao.CodigoLinha, "X"), referencia)];

            Assert.Equal(configuracao.QuantidadeChegadas, chegadas.Count);
            Assert.All(chegadas, c => Assert.False(c.VencidaEm(referencia)));
            Assert.True(chegadas[0].PrevistaParaUtc - referencia < TimeSpan.FromMinutes(configuracao.IntervaloMinutos), "a primeira chegada é a próxima da grade");
            Assert.All(chegadas, c => Assert.Equal(0, c.PrevistaParaUtc.Second));
            for (int i = 1; i < chegadas.Count; i++)
            {
                Assert.Equal(TimeSpan.FromMinutes(configuracao.IntervaloMinutos), chegadas[i].PrevistaParaUtc - chegadas[i - 1].PrevistaParaUtc);
                Assert.NotEqual(configuracao.Prefixos.Count > 1 ? chegadas[i - 1].PrefixoVeiculo : null, chegadas[i].PrefixoVeiculo);
            }
        }
    }

    [Fact]
    public async Task ChegadaExatamenteNoInstante_EntraComEsperaZero()
    {
        DateTimeOffset chegada = Local(2026, 11, 10, 14, 33);

        ResultadoProvedor resultado = await ConsultarAsync(chegada, _l002);

        PrevisaoChegada primeira = resultado.Previsoes[0];
        Assert.Equal(chegada, primeira.PrevistaParaUtc);
        Assert.Equal(0, primeira.EsperaMinutos(chegada));
    }

    [Fact]
    public async Task UmSegundoDepoisDaChegada_ElaNaoVoltaComoVencida()
    {
        DateTimeOffset referencia = Local(2026, 11, 10, 14, 33, 1);

        ResultadoProvedor resultado = await ConsultarAsync(referencia, _l002);

        Assert.Equal(["10/11/2026 14:51:00", "10/11/2026 15:09:00"], resultado.Previsoes.Select(Horario));
        Assert.All(resultado.Previsoes, p => Assert.False(p.VencidaEm(referencia)));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(30, 0)]
    [InlineData(59, 0)]
    public async Task EsperaEmMinutos_ArredondaParaBaixoSemFicarNegativa(int segundosDepoisDas1432, int espera)
    {
        DateTimeOffset referencia = ConsultaDoExemplo.AddSeconds(segundosDepoisDas1432);

        ResultadoProvedor resultado = await ConsultarAsync(referencia, _l002);

        Assert.Equal(espera, resultado.Previsoes[0].EsperaMinutos(referencia));
    }

    [Theory]
    [InlineData(2026, 11, 30, "01/12/2026 00:09:00", "01/12/2026 00:10:00")]
    [InlineData(2026, 12, 31, "01/01/2027 00:09:00", "01/01/2027 00:10:00")]
    public async Task PertoDaMeiaNoite_AsChegadasViramODiaOMesEOAno(int ano, int mes, int dia, string l002, string l004)
    {
        DateTimeOffset referencia = Local(ano, mes, dia, 23, 59);

        ResultadoProvedor resultado = await ConsultarAsync(referencia, _l002, _l004);

        Assert.Equal(l002, Horario(resultado.Previsoes.First(p => p.Linha.CodigoLinha == "SIM-L002")));
        Assert.Equal(l004, Horario(resultado.Previsoes.First(p => p.Linha.CodigoLinha == "SIM-L004")));
    }

    [Fact]
    public async Task MesmoInstante_MesmoResultado_MesmoDepoisDeRecriarOProvedor()
    {
        DateTimeOffset referencia = Local(2027, 5, 20, 18, 41, 17);

        ResultadoProvedor primeiro = await Provedor().ConsultarAsync(_centro, [_l002, _l003, _l004], referencia, Ct);
        ResultadoProvedor depoisDeReiniciar = await Provedor().ConsultarAsync(_centro, [_l002, _l003, _l004], referencia, Ct);

        Assert.Equal(primeiro.Previsoes, depoisDeReiniciar.Previsoes);
    }

    [Fact]
    public async Task UmDiaDepois_AGradeContinuaAMesmaSemDataFixa()
    {
        // A grade de 18 min da SIM-L002 se repete a cada 3 dias (1440 × 3 é múltiplo de 54); um dia
        // depois, os horários mudam, mas continuam alinhados à mesma grade.
        ResultadoProvedor hoje = await ConsultarAsync(ConsultaDoExemplo, _l002);
        ResultadoProvedor tresDiasDepois = await ConsultarAsync(ConsultaDoExemplo.AddDays(3), _l002);
        ResultadoProvedor umDiaDepois = await ConsultarAsync(ConsultaDoExemplo.AddDays(1), _l002);

        Assert.Equal(hoje.Previsoes.Select(p => (p.PrevistaParaUtc.AddDays(3), p.PrefixoVeiculo)), tresDiasDepois.Previsoes.Select(p => (p.PrevistaParaUtc, p.PrefixoVeiculo)));
        Assert.All(umDiaDepois.Previsoes, p => Assert.Equal(0, (p.PrevistaParaUtc - hoje.Previsoes[0].PrevistaParaUtc).TotalMinutes % 18));
    }

    [Fact]
    public async Task LinhaSemConfiguracao_FicaSemPrevisao_NadaEInventado()
    {
        Linha administrativa = Cenario.LinhaAdministrativa(dadosSimulados: false, codigo: "LT-0100").ComId(30);

        ResultadoProvedor resultado = await ConsultarAsync(ConsultaDoExemplo, _l001, administrativa);

        Assert.Equal(StatusPrevisao.SemPrevisoes, resultado.Status);
        Assert.Empty(resultado.Previsoes);
    }

    [Fact]
    public async Task VariasChegadasDaMesmaLinha_ComVeiculosDiferentes()
    {
        ResultadoProvedor resultado = await ConsultarAsync(ConsultaDoExemplo, _l003);

        Assert.Equal(2, resultado.Previsoes.Count);
        Assert.All(resultado.Previsoes, p => Assert.Equal("SIM-V3001", p.PrefixoVeiculo));
        Assert.Equal(TimeSpan.FromMinutes(20), resultado.Previsoes[1].PrevistaParaUtc - resultado.Previsoes[0].PrevistaParaUtc);
    }

    [Fact]
    public async Task ComIndisponibilidadeSimulada_RespondeIndisponivelSemPrevisoes()
    {
        ResultadoProvedor resultado = await Provedor(indisponivel: true).ConsultarAsync(_centro, [_l002, _l004], ConsultaDoExemplo, Ct);

        Assert.False(resultado.ProvedorDisponivel);
        Assert.Equal(StatusPrevisao.ProvedorIndisponivel, resultado.Status);
        Assert.Empty(resultado.Previsoes);
    }

    [Fact]
    public async Task ComTokenCancelado_PropagaOCancelamento()
    {
        using var cancelamento = new CancellationTokenSource();
        await cancelamento.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Provedor().ConsultarAsync(_centro, [_l002], ConsultaDoExemplo, cancelamento.Token));
    }

    [Fact]
    public async Task Consulta_NaoGravaNadaNoBanco()
    {
        await using BancoSqliteTeste banco = await BancoSqliteTeste.CriarAsync(provisionarDemonstracao: true);
        await using var contexto = banco.NovoContexto();
        Parada centro = contexto.Paradas.Single(p => p.CodigoExterno == "SIM-P001");
        List<Linha> linhas = [.. contexto.Linhas];
        banco.LimparComandosSql();

        ResultadoProvedor resultado = await Provedor().ConsultarAsync(centro, linhas, ConsultaDoExemplo, Ct);

        Assert.NotEmpty(resultado.Previsoes);
        Assert.Empty(banco.ComandosSql);
        Assert.False(contexto.ChangeTracker.HasChanges());
    }
}

using PontoVoz.Api.Application.Dtos;
using PontoVoz.Api.Application.Erros;
using PontoVoz.Api.Application.Services;
using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Tests.Domain;

namespace PontoVoz.Tests.Application;

/// <summary>Regras do service de últimas passagens, com repositórios em memória e relógio controlado.</summary>
public class UltimasPassagensServiceTests
{
    private static readonly DateTimeOffset Agora = new(2026, 11, 10, 17, 32, 0, TimeSpan.Zero);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private readonly DublesRepositorio _dados = new();
    private readonly RelogioContador _relogio = new(Agora);
    private readonly Parada _parada = Cenario.ParadaSimulada().ComId(1);
    private readonly Linha _l001 = Cenario.LinhaSimulada("SIM-L001", "SIM-101").ComId(1);
    private readonly Linha _l002 = Cenario.LinhaSimulada("SIM-L002", "SIM-102").ComId(2);
    private readonly Linha _l004 = Cenario.LinhaSimulada("SIM-L004", "SIM-104").ComId(4);
    private readonly Linha _l003 = Cenario.LinhaSimulada("SIM-L003", "SIM-203").ComId(3);

    public UltimasPassagensServiceTests()
    {
        foreach (Linha linha in new[] { _l001, _l002, _l004 })
        {
            _parada.AssociarLinha(linha, Cenario.Agora);
        }

        _dados.Paradas.Add(_parada);
        _dados.Linhas.AddRange([_l001, _l002, _l004, _l003]);
    }

    private UltimasPassagensService Service => new(_dados, _dados, _dados, _relogio);

    private PassagemRegistroManual Manual(Linha linha, TimeSpan antesDeAgora, string origem = "operador-local")
    {
        DateTimeOffset observadaEm = Agora - antesDeAgora;
        var passagem = PassagemRegistroManual.Registrar(_parada, linha, observadaEm, null, origem, observadaEm).ComId(_dados.Passagens.Count + 10);
        _dados.Passagens.Add(passagem);
        return passagem;
    }

    private PassagemSimulada Simulada(Linha linha, TimeSpan antesDeAgora)
    {
        // Provisionado no passado, 15 minutos depois da observação, como faz o seed.
        TimeSpan antecedencia = TimeSpan.FromMinutes(15);
        var passagem = PassagemSimulada.CriarNoProvisionamento(_parada.ObterAssociacao(linha), "SIM-OBS-001", antecedencia, Agora - antesDeAgora + antecedencia, "SIM-V2003").ComId(_dados.Passagens.Count + 10);
        _dados.Passagens.Add(passagem);
        return passagem;
    }

    private static async Task<ErroAplicacaoException> ErroAsync(Func<Task> acao, TipoErroAplicacao tipo, string codigo)
    {
        var erro = await Assert.ThrowsAsync<ErroAplicacaoException>(acao);
        Assert.Equal((tipo, codigo), (erro.Tipo, erro.Codigo));
        return erro;
    }

    [Fact]
    public async Task ParadaInexistente_E404_SemConsultarPassagens()
    {
        await ErroAsync(() => Service.ObterAsync(99, null, Ct), TipoErroAplicacao.NaoEncontrado, "ParadaNaoEncontrada");

        Assert.DoesNotContain("Passagens.Ultimas", _dados.Chamadas);
    }

    [Fact]
    public async Task ParadaInativa_E404()
    {
        _parada.Desativar();

        await ErroAsync(() => Service.ObterAsync(1, null, Ct), TipoErroAplicacao.NaoEncontrado, "ParadaNaoEncontrada");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public async Task LinhaIdMenorQue1_E400_AntesDeProcurarAParada(int linhaId)
    {
        var erro = await ErroAsync(() => Service.ObterAsync(99, linhaId, Ct), TipoErroAplicacao.Validacao, "RequisicaoInvalida");

        Assert.Equal(new[] { "linhaId" }, erro.Erros.Keys);
        Assert.Empty(_dados.Chamadas);
    }

    [Fact]
    public async Task LinhaInexistenteOuInativa_E404_DepoisDaParada()
    {
        await ErroAsync(() => Service.ObterAsync(1, 77, Ct), TipoErroAplicacao.NaoEncontrado, "LinhaNaoEncontrada");
        _l002.Desativar();
        await ErroAsync(() => Service.ObterAsync(1, 2, Ct), TipoErroAplicacao.NaoEncontrado, "LinhaNaoEncontrada");

        await ErroAsync(() => Service.ObterAsync(99, 77, Ct), TipoErroAplicacao.NaoEncontrado, "ParadaNaoEncontrada");
    }

    [Fact]
    public async Task LinhaQueNaoAtendeAParada_E422()
    {
        await ErroAsync(() => Service.ObterAsync(1, 3, Ct), TipoErroAplicacao.RegraNegocio, "LinhaNaoAtendeParada");

        Assert.DoesNotContain("Passagens.Ultimas", _dados.Chamadas);
    }

    [Fact]
    public async Task VariasLinhas_TrazemAsPassagensELinhasSemRegistro_ComAvisos()
    {
        Manual(_l004, TimeSpan.FromMinutes(8));
        Simulada(_l002, TimeSpan.FromMinutes(3227));

        UltimasPassagensParadaDto resposta = await Service.ObterAsync(1, null, Ct);

        Assert.Equal(new[] { "SIM-L004", "SIM-L002" }, resposta.Passagens.Select(p => p.CodigoLinha));
        Assert.Equal(new[] { new ReferenciaLinhaDto(1, "SIM-L001", "SIM-101") }, resposta.LinhasSemRegistro);
        Assert.Equal(new[] { "DadosSimulados", "PassagemDesatualizada", "PassagemNaoRegistrada" }, resposta.Avisos.Select(a => a.Codigo));
        Assert.Equal((1, "SIM-P001", "Parada Demonstração Centro", true, 60), (resposta.ParadaId, resposta.CodigoExternoParada, resposta.NomeParada, resposta.DadosSimulados, resposta.LimiteDesatualizacaoMinutos));
    }

    [Fact]
    public async Task PassagemSimuladaEManual_TemNivelAvisoEOrigemDoTipoConcreto_ENenhumaEVerificada()
    {
        Manual(_l004, TimeSpan.FromMinutes(8), "totem-01");
        Simulada(_l002, TimeSpan.FromMinutes(15));

        UltimasPassagensParadaDto resposta = await Service.ObterAsync(1, null, Ct);
        PassagemDto manual = resposta.Passagens.Single(p => p.CodigoLinha == "SIM-L004");
        PassagemDto simulada = resposta.Passagens.Single(p => p.CodigoLinha == "SIM-L002");

        Assert.Equal((NivelConfirmacao.RegistroManual, "totem-01", "RegistroManualNaoVerificado"), (manual.NivelConfirmacao, manual.OrigemRegistro, manual.Aviso.Codigo));
        Assert.Equal((NivelConfirmacao.ObservacaoSimulada, (string?)null, "ObservacaoSimulada", "SIM-V2003"), (simulada.NivelConfirmacao, simulada.OrigemRegistro, simulada.Aviso.Codigo, simulada.PrefixoVeiculo));
        Assert.All(resposta.Passagens, p =>
        {
            Assert.False(p.Verificada);
            Assert.False(p.OficialSptrans);
            Assert.True(p.DadosSimulados);
            Assert.NotEqual(NivelConfirmacao.Previsao, p.NivelConfirmacao);
        });
    }

    [Theory]
    [InlineData(0, 0, false)]
    [InlineData(59 * 60 + 59, 59, false)]
    [InlineData(3600, 60, false)]
    [InlineData(3601, 60, true)]
    [InlineData(3660, 61, true)]
    public async Task Desatualizacao_UsaOIntervaloExatoDe60Minutos(int segundos, int minutos, bool desatualizada)
    {
        Manual(_l004, TimeSpan.FromSeconds(segundos));

        UltimasPassagensParadaDto resposta = await Service.ObterAsync(1, 4, Ct);

        PassagemDto passagem = Assert.Single(resposta.Passagens);
        Assert.Equal((minutos, desatualizada), (passagem.MinutosDesdeObservacao, passagem.Desatualizada));
        Assert.Equal(desatualizada, resposta.Avisos.Any(a => a.Codigo == "PassagemDesatualizada"));
    }

    [Fact]
    public async Task OInstanteELidoUmaVez_EUsadoNoFiltroENoConsultadoEm()
    {
        Manual(_l004, TimeSpan.FromMinutes(5));

        UltimasPassagensParadaDto resposta = await Service.ObterAsync(1, null, Ct);

        Assert.Equal(1, _relogio.Leituras);
        Assert.Equal(Agora, _dados.UltimaReferenciaPassagens);
        Assert.Equal(Agora, resposta.ConsultadoEm);
        Assert.Equal(TimeSpan.FromHours(-3), resposta.ConsultadoEm.Offset);
        Assert.Equal(TimeSpan.FromHours(-3), resposta.Passagens[0].ObservadaEm.Offset);
    }

    [Fact]
    public async Task FiltroPorLinhaSemRegistro_ColocaALinhaEmLinhasSemRegistro()
    {
        Manual(_l004, TimeSpan.FromMinutes(5));

        UltimasPassagensParadaDto resposta = await Service.ObterAsync(1, 2, Ct);

        Assert.Empty(resposta.Passagens);
        Assert.Equal(new[] { "SIM-L002" }, resposta.LinhasSemRegistro.Select(l => l.CodigoLinha));
        Assert.Contains(resposta.Avisos, a => a.Codigo == "PassagemNaoRegistrada");
    }

    [Fact]
    public async Task ParadaSemLinhas_DevolveListasVaziasComAvisoParadaSemLinhas()
    {
        _dados.Paradas.Add(Cenario.ParadaSimulada("SIM-P009", "Parada Sem Linhas").ComId(9));

        UltimasPassagensParadaDto resposta = await Service.ObterAsync(9, null, Ct);

        Assert.Empty(resposta.Passagens);
        Assert.Empty(resposta.LinhasSemRegistro);
        Assert.Equal(new[] { "DadosSimulados", "ParadaSemLinhas" }, resposta.Avisos.Select(a => a.Codigo));
    }

    [Fact]
    public async Task CadastroAdministrativoReal_NaoEMarcadoComoSimulado_MasTemCadastroNaoVerificado()
    {
        Parada parada = Parada.CadastrarAdministrativa("PARADA-0001", "Parada Real", null, null, false, Cenario.Agora).ComId(20);
        Linha linha = Linha.CadastrarAdministrativa("LINHA-0001", "8000-10", "Centro", "Sul", SentidoLinha.PrincipalParaSecundario, false, Cenario.Agora).ComId(20);
        parada.AssociarLinha(linha, Cenario.Agora);
        _dados.Paradas.Add(parada);
        _dados.Linhas.Add(linha);
        var passagem = PassagemRegistroManual.Registrar(parada, linha, Agora.AddMinutes(-2), "V-12", "operador-local", Agora).ComId(30);
        _dados.Passagens.Add(passagem);

        UltimasPassagensParadaDto resposta = await Service.ObterAsync(20, null, Ct);

        Assert.False(resposta.DadosSimulados);
        Assert.False(Assert.Single(resposta.Passagens).DadosSimulados);
        Assert.Equal(new[] { "CadastroNaoVerificado" }, resposta.Avisos.Select(a => a.Codigo));
    }
}

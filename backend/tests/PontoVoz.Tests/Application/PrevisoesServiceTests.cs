using PontoVoz.Api.Application.Dtos;
using PontoVoz.Api.Application.Erros;
using PontoVoz.Api.Application.Interfaces;
using PontoVoz.Api.Application.Services;
using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.Exceptions;
using PontoVoz.Api.Domain.ValueObjects;
using PontoVoz.Tests.Domain;

namespace PontoVoz.Tests.Application;

/// <summary>
/// Regras do service de previsões (seção 4.5) com repositórios em memória e um provedor falso
/// que registra as chamadas: ordem 400 → 404 → 422 → provedor, estados da resposta, avisos e 503.
/// </summary>
public sealed class PrevisoesServiceTests
{
    private static readonly DateTimeOffset Agora = new(2026, 11, 10, 17, 32, 0, TimeSpan.Zero);

    private readonly DublesRepositorio _dados = new();
    private readonly RelogioContador _relogio = new(Agora);
    private readonly ProvedorFalso _provedor = new();
    private readonly Parada _parada = Cenario.ParadaSimulada().ComId(1);
    private readonly Linha _l001 = Cenario.LinhaSimulada("SIM-L001", "SIM-101").ComId(1);
    private readonly Linha _l002 = Cenario.LinhaSimulada("SIM-L002", "SIM-102").ComId(2);
    private readonly Linha _l003 = Cenario.LinhaSimulada("SIM-L003", "SIM-203").ComId(3);
    private readonly Linha _l004 = Cenario.LinhaSimulada("SIM-L004", "SIM-104").ComId(4);

    public PrevisoesServiceTests()
    {
        foreach (Linha linha in new[] { _l004, _l001, _l002 })
        {
            _parada.AssociarLinha(linha, Cenario.Agora);
        }

        _dados.Paradas.Add(_parada);
        _dados.Linhas.AddRange([_l001, _l002, _l003, _l004]);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private PrevisoesService Service => new(_dados, _dados, _provedor, _relogio);

    private static PrevisaoChegada Previsao(Linha linha, int minutos, string? prefixo = null) =>
        new(ReferenciaLinha.De(linha), Agora.AddMinutes(minutos), prefixo);

    private void Responder(params PrevisaoChegada[] previsoes) =>
        _provedor.Resultado = ResultadoProvedor.Respondido(OrigemDado.Simulado, Agora, previsoes);

    private static string[] Avisos(PrevisoesParadaDto dto) => [.. dto.Avisos.Select(a => a.Codigo)];

    [Fact]
    public async Task ParadaValida_ComPrevisaoParcial_MontaARespostaDoContrato()
    {
        Responder(Previsao(_l002, 19, "SIM-V2002"), Previsao(_l004, 8, "SIM-V4001"), Previsao(_l002, 1, "SIM-V2001"));

        PrevisoesParadaDto dto = await Service.ObterAsync(1, null, Ct);

        Assert.Equal((StatusPrevisao.Disponivel, OrigemDado.Simulado, true), (dto.Status, dto.Origem, dto.DadosSimulados));
        Assert.Equal([("SIM-L002", 1), ("SIM-L004", 8), ("SIM-L002", 19)], dto.Previsoes.Select(p => (p.CodigoLinha, p.EsperaMinutos)));
        Assert.All(dto.Previsoes, p => Assert.Equal(NivelConfirmacao.Previsao, p.NivelConfirmacao));
        Assert.Equal(["SIM-L001"], dto.LinhasSemPrevisao.Select(l => l.CodigoLinha));
        Assert.Equal(["DadosSimulados", "PrevisaoParcial"], Avisos(dto));
        Assert.Equal(TimeSpan.FromHours(-3), dto.ConsultadoEm.Offset);
        Assert.Equal(Agora, dto.ConsultadoEm);
    }

    [Fact]
    public async Task UmInstanteSo_ParaProvedorEsperasEConsultadoEm()
    {
        Responder(Previsao(_l002, 1));

        PrevisoesParadaDto dto = await Service.ObterAsync(1, null, Ct);

        Assert.Equal(1, _relogio.Leituras);
        Assert.Equal(Agora, _provedor.Referencias.Single());
        Assert.Equal(Agora, dto.ConsultadoEm);
    }

    [Fact]
    public async Task PrevisaoParaTodasAsLinhas_SemLinhasSemPrevisaoNemAvisoParcial()
    {
        Responder(Previsao(_l001, 3), Previsao(_l002, 1), Previsao(_l004, 8));

        PrevisoesParadaDto dto = await Service.ObterAsync(1, null, Ct);

        Assert.Equal(StatusPrevisao.Disponivel, dto.Status);
        Assert.Empty(dto.LinhasSemPrevisao);
        Assert.Equal(["DadosSimulados"], Avisos(dto));
    }

    [Fact]
    public async Task ProvedorSemPrevisoes_StatusSemPrevisoesComTodasAsLinhas()
    {
        Responder();

        PrevisoesParadaDto dto = await Service.ObterAsync(1, null, Ct);

        Assert.Equal(StatusPrevisao.SemPrevisoes, dto.Status);
        Assert.Empty(dto.Previsoes);
        Assert.Equal(["SIM-L001", "SIM-L002", "SIM-L004"], dto.LinhasSemPrevisao.Select(l => l.CodigoLinha));
        Assert.Equal(["DadosSimulados", "SemPrevisoes"], Avisos(dto));
    }

    [Fact]
    public async Task ProvedorIndisponivel_LancaAExcecaoDo503()
    {
        _provedor.Resultado = ResultadoProvedor.Indisponivel(OrigemDado.Simulado, Agora);

        var erro = await Assert.ThrowsAsync<ProvedorPrevisoesIndisponivelException>(() => Service.ObterAsync(1, null, Ct));

        Assert.Equal("ProvedorPrevisoesIndisponivel", erro.Codigo);
    }

    [Fact]
    public async Task ParadaSemLinhas_Responde200SemConsultarOProvedor()
    {
        Parada vazia = Cenario.ParadaSimulada("SIM-P009", "Parada Sem Linhas").ComId(9);
        _dados.Paradas.Add(vazia);
        _provedor.Resultado = ResultadoProvedor.Indisponivel(OrigemDado.Simulado, Agora);

        PrevisoesParadaDto dto = await Service.ObterAsync(9, null, Ct);

        Assert.Empty(_provedor.Referencias);
        Assert.Equal((StatusPrevisao.SemPrevisoes, OrigemDado.Simulado), (dto.Status, dto.Origem));
        Assert.Empty(dto.Previsoes);
        Assert.Empty(dto.LinhasSemPrevisao);
        Assert.Equal(["DadosSimulados", "ParadaSemLinhas"], Avisos(dto));
    }

    [Fact]
    public async Task ComLinhaId_ConsultaSoEssaLinha()
    {
        Responder(Previsao(_l004, 8), Previsao(_l002, 1));

        PrevisoesParadaDto dto = await Service.ObterAsync(1, 4, Ct);

        Assert.Equal([4], _provedor.Linhas.Single().Select(l => l.Id));
        Assert.Equal(["SIM-L004"], dto.Previsoes.Select(p => p.CodigoLinha));
        Assert.Empty(dto.LinhasSemPrevisao);
    }

    [Fact]
    public async Task OrdemDeterministica_PorHorarioCodigoEPrefixo()
    {
        Responder(Previsao(_l004, 5, "SIM-V4002"), Previsao(_l002, 5, "SIM-V2002"), Previsao(_l002, 5, "SIM-V2001"), Previsao(_l001, 2));

        PrevisoesParadaDto dto = await Service.ObterAsync(1, null, Ct);

        Assert.Equal([("SIM-L001", null), ("SIM-L002", "SIM-V2001"), ("SIM-L002", "SIM-V2002"), ("SIM-L004", "SIM-V4002")],
            dto.Previsoes.Select(p => (p.CodigoLinha, p.PrefixoVeiculo)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-4)]
    public async Task LinhaIdInvalido_Responde400SemConsultarNada(int linhaId)
    {
        var erro = await Assert.ThrowsAsync<ErroAplicacaoException>(() => Service.ObterAsync(1, linhaId, Ct));

        Assert.Equal((TipoErroAplicacao.Validacao, "linhaId"), (erro.Tipo, erro.Erros.Keys.Single()));
        Assert.Empty(_dados.Chamadas);
        Assert.Empty(_provedor.Referencias);
    }

    [Theory]
    [InlineData(99)]
    [InlineData(0)]
    public async Task ParadaInexistenteOuIdInvalido_Responde404SemConsultarOProvedor(int paradaId)
    {
        var erro = await Assert.ThrowsAsync<ErroAplicacaoException>(() => Service.ObterAsync(paradaId, null, Ct));

        Assert.Equal("ParadaNaoEncontrada", erro.Codigo);
        Assert.Empty(_provedor.Referencias);
    }

    [Fact]
    public async Task ParadaInativa_Responde404()
    {
        _parada.Desativar();

        var erro = await Assert.ThrowsAsync<ErroAplicacaoException>(() => Service.ObterAsync(1, null, Ct));

        Assert.Equal("ParadaNaoEncontrada", erro.Codigo);
    }

    [Fact]
    public async Task LinhaInexistenteOuInativa_Responde404()
    {
        _l002.Desativar();

        var inexistente = await Assert.ThrowsAsync<ErroAplicacaoException>(() => Service.ObterAsync(1, 77, Ct));
        var inativa = await Assert.ThrowsAsync<ErroAplicacaoException>(() => Service.ObterAsync(1, 2, Ct));

        Assert.Equal("LinhaNaoEncontrada", inexistente.Codigo);
        Assert.Equal("LinhaNaoEncontrada", inativa.Codigo);
        Assert.Empty(_provedor.Referencias);
    }

    [Fact]
    public async Task LinhaQueNaoAtendeAParada_Responde422SemConsultarOProvedor()
    {
        var erro = await Assert.ThrowsAsync<ErroAplicacaoException>(() => Service.ObterAsync(1, 3, Ct));

        Assert.Equal((TipoErroAplicacao.RegraNegocio, "LinhaNaoAtendeParada"), (erro.Tipo, erro.Codigo));
        Assert.Empty(_provedor.Referencias);
    }

    [Fact]
    public async Task CadastroAdministrativoNaoFicticio_TemCadastroNaoVerificadoESemDadosSimulados()
    {
        Parada real = Cenario.ParadaAdministrativa(dadosSimulados: false, codigo: "PT-0001").ComId(5);
        Linha linhaReal = Cenario.LinhaAdministrativa(dadosSimulados: false, codigo: "LT-0100").ComId(6);
        real.AssociarLinha(linhaReal, Cenario.Agora);
        _dados.Paradas.Add(real);
        _dados.Linhas.Add(linhaReal);
        Responder();

        PrevisoesParadaDto dto = await Service.ObterAsync(5, null, Ct);

        Assert.False(dto.DadosSimulados);
        Assert.Equal(["CadastroNaoVerificado", "SemPrevisoes"], Avisos(dto));
    }

    [Fact]
    public async Task CadastroAdministrativoFicticio_TemOsDoisAvisosNaOrdem()
    {
        Parada ficticia = Cenario.ParadaAdministrativa(dadosSimulados: true).ComId(7);
        ficticia.AssociarLinha(_l002, Cenario.Agora);
        _dados.Paradas.Add(ficticia);
        Responder(Previsao(_l002, 4));

        PrevisoesParadaDto dto = await Service.ObterAsync(7, null, Ct);

        Assert.Equal(["DadosSimulados", "CadastroNaoVerificado"], Avisos(dto));
    }

    [Fact]
    public async Task CancellationToken_ChegaAoProvedor()
    {
        Responder(Previsao(_l002, 1));

        await Service.ObterAsync(1, null, Ct);

        Assert.Equal(Ct, _provedor.Tokens.Single());
    }

    [Fact]
    public async Task CancelamentoNoProvedor_EPropagado()
    {
        _provedor.Falha = new OperationCanceledException();

        await Assert.ThrowsAsync<OperationCanceledException>(() => Service.ObterAsync(1, null, Ct));
    }

    /// <summary>Provedor falso: devolve <see cref="Resultado"/> e guarda as referências, linhas e tokens recebidos.</summary>
    private sealed class ProvedorFalso : IProvedorPrevisaoChegada
    {
        public ResultadoProvedor Resultado { get; set; } = ResultadoProvedor.Respondido(OrigemDado.Simulado, Agora, []);

        public Exception? Falha { get; set; }

        public List<DateTimeOffset> Referencias { get; } = [];

        public List<IReadOnlyList<Linha>> Linhas { get; } = [];

        public List<CancellationToken> Tokens { get; } = [];

        public OrigemDado OrigemPara(Parada parada) => OrigemDado.Simulado;

        public Task<ResultadoProvedor> ConsultarAsync(Parada parada, IReadOnlyList<Linha> linhas, DateTimeOffset referencia, CancellationToken cancellationToken)
        {
            Referencias.Add(referencia);
            Linhas.Add(linhas);
            Tokens.Add(cancellationToken);
            return Falha is null ? Task.FromResult(Resultado) : throw Falha;
        }
    }
}

using PontoVoz.Api.Application.Consultas;
using PontoVoz.Api.Application.Dtos;
using PontoVoz.Api.Application.Erros;
using PontoVoz.Api.Application.Interfaces;
using PontoVoz.Api.Application.Services;
using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.ValueObjects;
using PontoVoz.Tests.Domain;

namespace PontoVoz.Tests.Application;

/// <summary>
/// Regras do resumo (seção 4.8): instante único, situação completa ou parcial, partes iguais às
/// rotas individuais, parada sem linhas, erros, cancelamento e o paralelismo seguro (provedor junto
/// com a consulta de passagens, sem duas operações de banco ao mesmo tempo).
/// </summary>
public sealed class ResumoParadaServiceTests
{
    private static readonly DateTimeOffset Agora = new(2026, 11, 10, 17, 32, 0, TimeSpan.Zero);

    private readonly DublesRepositorio _dados = new();
    private readonly RelogioContador _relogio = new(Agora);
    private readonly ProvedorFalso _provedor = new();
    private readonly Parada _parada = Cenario.ParadaSimulada().ComId(1);
    private readonly Linha _l001 = Cenario.LinhaSimulada("SIM-L001", "SIM-101").ComId(1);
    private readonly Linha _l002 = Cenario.LinhaSimulada("SIM-L002", "SIM-102").ComId(2);
    private readonly Linha _l004 = Cenario.LinhaSimulada("SIM-L004", "SIM-104").ComId(4);

    public ResumoParadaServiceTests()
    {
        foreach (Linha linha in new[] { _l001, _l002, _l004 })
        {
            _parada.AssociarLinha(linha, Cenario.Agora);
        }

        _dados.Paradas.Add(_parada);
        _dados.Linhas.AddRange([_l001, _l002, _l004]);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private ResumoParadaService Service => new(_dados, _dados, _dados, _provedor, _relogio);

    private static PrevisaoChegada Previsao(Linha linha, int minutos) =>
        new(ReferenciaLinha.De(linha), Agora.AddMinutes(minutos), null);

    private void Responder(params PrevisaoChegada[] previsoes) =>
        _provedor.Resultado = ResultadoProvedor.Respondido(OrigemDado.Simulado, Agora, previsoes);

    private PassagemRegistroManual Manual(Linha linha, TimeSpan antes) =>
        PassagemRegistroManual.Registrar(_parada, linha, Agora - antes, null, "operador-local", Agora - antes + TimeSpan.FromSeconds(30)).ComId(50 + linha.Id);

    private static string[] Avisos(IEnumerable<AvisoDto> avisos) => [.. avisos.Select(a => a.Codigo)];

    [Fact]
    public async Task Completo_UmInstanteParaTodasAsPartes()
    {
        Responder(Previsao(_l002, 1), Previsao(_l004, 8));
        _dados.Passagens.Add(Manual(_l004, TimeSpan.FromMinutes(8)));

        ResumoParadaDto dto = await Service.ObterAsync(1, Ct);

        Assert.Equal(1, _relogio.Leituras);
        Assert.Equal(Agora, _provedor.Referencias.Single());
        Assert.Equal(Agora, _dados.UltimaReferenciaPassagens);
        Assert.All(new[] { dto.ConsultadoEm, dto.Previsoes.ConsultadoEm, dto.UltimasPassagens.ConsultadoEm }, c =>
        {
            Assert.Equal(Agora, c);
            Assert.Equal(TimeSpan.FromHours(-3), c.Offset);
        });
        Assert.Equal((SituacaoResumo.Completo, true), (dto.Situacao, dto.DadosSimulados));
        Assert.Empty(dto.PartesIndisponiveis);
        Assert.Empty(dto.Avisos);
        Assert.Equal(8, dto.UltimasPassagens.Passagens.Single().MinutosDesdeObservacao);
        Assert.Equal([1, 8], dto.Previsoes.Previsoes.Select(p => p.EsperaMinutos));
    }

    [Fact]
    public async Task PartesTemOMesmoFormatoDasRotasIndividuais()
    {
        Responder(Previsao(_l002, 1), Previsao(_l004, 8));
        _dados.Passagens.Add(Manual(_l002, TimeSpan.FromMinutes(90)));

        ResumoParadaDto resumo = await Service.ObterAsync(1, Ct);
        ParadaDetalheDto detalhe = await new ParadaService(_dados, _dados).ObterDetalheAsync(1, Ct);
        PrevisoesParadaDto previsoes = await new PrevisoesService(_dados, _dados, _provedor, new RelogioContador(Agora)).ObterAsync(1, null, Ct);
        UltimasPassagensParadaDto ultimas = await new UltimasPassagensService(_dados, _dados, _dados, new RelogioContador(Agora)).ObterAsync(1, null, Ct);

        Assert.Equal(ExemplosContrato.Serializar(detalhe).ToJsonString(), ExemplosContrato.Serializar(resumo.Parada).ToJsonString());
        Assert.Equal(ExemplosContrato.Serializar(previsoes).ToJsonString(), ExemplosContrato.Serializar(resumo.Previsoes).ToJsonString());
        Assert.Equal(ExemplosContrato.Serializar(ultimas).ToJsonString(), ExemplosContrato.Serializar(resumo.UltimasPassagens).ToJsonString());
        Assert.True(resumo.UltimasPassagens.Passagens.Single().Desatualizada);
    }

    [Fact]
    public async Task ProvedorIndisponivel_ParcialComAsOutrasPartesIntactas()
    {
        _provedor.Resultado = ResultadoProvedor.Indisponivel(OrigemDado.Simulado, Agora);
        _dados.Passagens.Add(Manual(_l004, TimeSpan.FromMinutes(8)));

        ResumoParadaDto dto = await Service.ObterAsync(1, Ct);

        Assert.Equal(SituacaoResumo.Parcial, dto.Situacao);
        Assert.Equal(["previsoes"], dto.PartesIndisponiveis);
        Assert.Equal(["ResumoParcial"], Avisos(dto.Avisos));
        Assert.Equal((StatusPrevisao.ProvedorIndisponivel, OrigemDado.Simulado, true), (dto.Previsoes.Status, dto.Previsoes.Origem, dto.Previsoes.DadosSimulados));
        Assert.Empty(dto.Previsoes.Previsoes);
        Assert.Equal(["SIM-L001", "SIM-L002", "SIM-L004"], dto.Previsoes.LinhasSemPrevisao.Select(l => l.CodigoLinha));
        Assert.Equal(["ProvedorIndisponivel", "DadosSimulados"], Avisos(dto.Previsoes.Avisos));
        Assert.Equal(3, dto.Parada.Linhas.Count);
        Assert.Single(dto.UltimasPassagens.Passagens);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SemPrevisoesOuPrevisaoParcial_ComProvedorDisponivel_ContinuaCompleto(bool parcial)
    {
        if (parcial)
        {
            Responder(Previsao(_l002, 1));
        }
        else
        {
            Responder();
        }

        ResumoParadaDto dto = await Service.ObterAsync(1, Ct);

        Assert.Equal(SituacaoResumo.Completo, dto.Situacao);
        Assert.Empty(dto.PartesIndisponiveis);
        Assert.Empty(dto.Avisos);
        Assert.Equal(parcial ? StatusPrevisao.Disponivel : StatusPrevisao.SemPrevisoes, dto.Previsoes.Status);
        Assert.Contains(parcial ? "PrevisaoParcial" : "SemPrevisoes", Avisos(dto.Previsoes.Avisos));
    }

    [Fact]
    public async Task ParadaSemLinhas_SemConsultarOProvedor()
    {
        Parada vazia = Cenario.ParadaSimulada("SIM-P009", "Parada Sem Linhas").ComId(9);
        _dados.Paradas.Add(vazia);
        _provedor.Resultado = ResultadoProvedor.Indisponivel(OrigemDado.Simulado, Agora);

        ResumoParadaDto dto = await Service.ObterAsync(9, Ct);

        Assert.Empty(_provedor.Referencias);
        Assert.Equal(SituacaoResumo.Completo, dto.Situacao);
        Assert.Empty(dto.Parada.Linhas);
        Assert.Contains("ParadaSemLinhas", Avisos(dto.Previsoes.Avisos));
        Assert.Contains("ParadaSemLinhas", Avisos(dto.UltimasPassagens.Avisos));
    }

    [Fact]
    public async Task ParadaInexistenteOuInativa_Responde404SemLerRelogioNemProvedor()
    {
        var inexistente = await Assert.ThrowsAsync<ErroAplicacaoException>(() => Service.ObterAsync(99, Ct));
        _parada.Desativar();
        var inativa = await Assert.ThrowsAsync<ErroAplicacaoException>(() => Service.ObterAsync(1, Ct));

        Assert.Equal("ParadaNaoEncontrada", inexistente.Codigo);
        Assert.Equal("ParadaNaoEncontrada", inativa.Codigo);
        Assert.Equal(0, _relogio.Leituras);
        Assert.Empty(_provedor.Referencias);
    }

    [Fact]
    public async Task CadastroAdministrativo_AvisoFicaDentroDasPartes()
    {
        Parada real = Cenario.ParadaAdministrativa(dadosSimulados: false, codigo: "PT-0001").ComId(5);
        Linha linhaReal = Cenario.LinhaAdministrativa(dadosSimulados: false, codigo: "LT-0100").ComId(6);
        real.AssociarLinha(linhaReal, Cenario.Agora);
        _dados.Paradas.Add(real);
        _dados.Linhas.Add(linhaReal);
        Responder();

        ResumoParadaDto dto = await Service.ObterAsync(5, Ct);

        Assert.False(dto.DadosSimulados);
        Assert.Empty(dto.Avisos);
        Assert.Contains("CadastroNaoVerificado", Avisos(dto.Previsoes.Avisos));
        Assert.Contains("CadastroNaoVerificado", Avisos(dto.UltimasPassagens.Avisos));
    }

    [Fact]
    public async Task FalhaDoBanco_NaoViraResumoParcial()
    {
        Responder(Previsao(_l002, 1));
        var falhando = new RepositoriosInstrumentados(_dados) { FalhaNasPassagens = new InvalidOperationException("falha do banco") };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new ResumoParadaService(falhando, falhando, falhando, _provedor, _relogio).ObterAsync(1, Ct));
    }

    [Fact]
    public async Task ProvedorEPassagensEmParalelo_SemDuasOperacoesDeBancoAoMesmoTempo()
    {
        var repositorios = new RepositoriosInstrumentados(_dados);
        var provedor = new ProvedorLento(repositorios, ResultadoProvedor.Respondido(OrigemDado.Simulado, Agora, [Previsao(_l002, 1)]));

        ResumoParadaDto dto = await new ResumoParadaService(repositorios, repositorios, repositorios, provedor, _relogio).ObterAsync(1, Ct);

        Assert.Equal(1, repositorios.MaximoSimultaneo);
        Assert.True(provedor.ViuConsultaDePassagensEmAndamento, "o provedor deveria rodar junto com a consulta de passagens (Task.WhenAll)");
        Assert.Equal(1, provedor.Chamadas);
        Assert.Equal(SituacaoResumo.Completo, dto.Situacao);
    }

    [Fact]
    public async Task CancellationToken_ChegaAoProvedorEAosRepositorios()
    {
        Responder(Previsao(_l002, 1));

        await Service.ObterAsync(1, Ct);

        Assert.Equal(Ct, _provedor.Tokens.Single());
    }

    [Fact]
    public async Task CancelamentoNoProvedor_EPropagado()
    {
        _provedor.Falha = new OperationCanceledException();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Service.ObterAsync(1, Ct));
    }

    /// <summary>Provedor falso: devolve <see cref="Resultado"/> e guarda referências e tokens.</summary>
    private sealed class ProvedorFalso : IProvedorPrevisaoChegada
    {
        public ResultadoProvedor Resultado { get; set; } = ResultadoProvedor.Respondido(OrigemDado.Simulado, Agora, []);

        public Exception? Falha { get; set; }

        public List<DateTimeOffset> Referencias { get; } = [];

        public List<CancellationToken> Tokens { get; } = [];

        public OrigemDado OrigemPara(Parada parada) => OrigemDado.Simulado;

        public Task<ResultadoProvedor> ConsultarAsync(Parada parada, IReadOnlyList<Linha> linhas, DateTimeOffset referencia, CancellationToken cancellationToken)
        {
            Referencias.Add(referencia);
            Tokens.Add(cancellationToken);
            return Falha is null ? Task.FromResult(Resultado) : throw Falha;
        }
    }

    /// <summary>Provedor assíncrono de verdade: espera um pouco e registra se a consulta de passagens estava em andamento.</summary>
    private sealed class ProvedorLento(RepositoriosInstrumentados repositorios, ResultadoProvedor resultado) : IProvedorPrevisaoChegada
    {
        public bool ViuConsultaDePassagensEmAndamento { get; private set; }

        public int Chamadas { get; private set; }

        public OrigemDado OrigemPara(Parada parada) => OrigemDado.Simulado;

        public async Task<ResultadoProvedor> ConsultarAsync(Parada parada, IReadOnlyList<Linha> linhas, DateTimeOffset referencia, CancellationToken cancellationToken)
        {
            Chamadas++;
            for (int i = 0; i < 20 && !ViuConsultaDePassagensEmAndamento; i++)
            {
                await Task.Delay(5, cancellationToken);
                ViuConsultaDePassagensEmAndamento |= repositorios.PassagensEmAndamento;
            }

            return resultado;
        }
    }

    /// <summary>
    /// Repositórios que demoram de verdade (como o banco) e contam quantas operações estão em
    /// andamento ao mesmo tempo, como o <c>DbContext</c>, que não aceita duas.
    /// </summary>
    private sealed class RepositoriosInstrumentados(DublesRepositorio dados) : IParadaRepository, ILinhaRepository, IRegistroPassagemRepository
    {
        private int _emAndamento;

        public int MaximoSimultaneo { get; private set; }

        public bool PassagensEmAndamento { get; private set; }

        public Exception? FalhaNasPassagens { get; init; }

        public Task<Pagina<Parada>> BuscarAtivasAsync(string? termo, string? codigoExterno, Paginacao paginacao, CancellationToken cancellationToken) =>
            ExecutarAsync(() => ((IParadaRepository)dados).BuscarAtivasAsync(termo, codigoExterno, paginacao, cancellationToken), cancellationToken);

        public Task<Parada?> ObterAtivaAsync(int id, CancellationToken cancellationToken) =>
            ExecutarAsync(() => ((IParadaRepository)dados).ObterAtivaAsync(id, cancellationToken), cancellationToken);

        public Task<Pagina<Linha>> BuscarAtivasAsync(string? termo, Paginacao paginacao, CancellationToken cancellationToken) =>
            ExecutarAsync(() => ((ILinhaRepository)dados).BuscarAtivasAsync(termo, paginacao, cancellationToken), cancellationToken);

        Task<Linha?> ILinhaRepository.ObterAtivaAsync(int id, CancellationToken cancellationToken) =>
            ExecutarAsync(() => ((ILinhaRepository)dados).ObterAtivaAsync(id, cancellationToken), cancellationToken);

        public Task<IReadOnlyList<Linha>> ListarAtivasDaParadaAsync(int paradaId, CancellationToken cancellationToken) =>
            ExecutarAsync(() => ((ILinhaRepository)dados).ListarAtivasDaParadaAsync(paradaId, cancellationToken), cancellationToken);

        public Task<bool> AtendeParadaAsync(int paradaId, int linhaId, CancellationToken cancellationToken) =>
            ExecutarAsync(() => ((ILinhaRepository)dados).AtendeParadaAsync(paradaId, linhaId, cancellationToken), cancellationToken);

        public async Task<IReadOnlyList<RegistroPassagem>> ListarUltimasPorLinhaAsync(int paradaId, DateTimeOffset referencia, int? linhaId, CancellationToken cancellationToken)
        {
            PassagensEmAndamento = true;
            try
            {
                return await ExecutarAsync(async () =>
                {
                    if (FalhaNasPassagens is not null)
                    {
                        throw FalhaNasPassagens;
                    }

                    return await ((IRegistroPassagemRepository)dados).ListarUltimasPorLinhaAsync(paradaId, referencia, linhaId, cancellationToken);
                }, cancellationToken);
            }
            finally
            {
                PassagensEmAndamento = false;
            }
        }

        private async Task<T> ExecutarAsync<T>(Func<Task<T>> operacao, CancellationToken cancellationToken)
        {
            int simultaneas = Interlocked.Increment(ref _emAndamento);
            MaximoSimultaneo = Math.Max(MaximoSimultaneo, simultaneas);
            try
            {
                await Task.Delay(30, cancellationToken);
                return await operacao();
            }
            finally
            {
                Interlocked.Decrement(ref _emAndamento);
            }
        }
    }
}

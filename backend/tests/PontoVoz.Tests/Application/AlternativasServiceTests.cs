using System.Reflection;

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
/// Regras do service de alternativas (seção 4.7) com repositórios em memória, um provedor falso e
/// relógio controlado: validação, situações, ranking, desempates, linhas sem previsão e avisos.
/// </summary>
public sealed class AlternativasServiceTests
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
    private readonly Destino _norte = Destino.CriarSimulado("Terminal Demonstração Norte", TipoDestino.Terminal, Cenario.Agora).ComId(3);
    private readonly Destino _sul = Destino.CriarSimulado("Terminal Demonstração Sul", TipoDestino.Terminal, Cenario.Agora).ComId(2);

    public AlternativasServiceTests()
    {
        _dados.Paradas.Add(_parada);
        _dados.Linhas.AddRange([_l001, _l002, _l003, _l004]);
        _dados.Destinos.AddRange([_norte, _sul]);
        Percurso(_l001, 30);
        Percurso(_l002, 40);
        Percurso(_l004, 20);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private AlternativasService Service => new(_dados, _dados, _dados, _provedor, _relogio);

    private void Percurso(Linha linha, int minutos, Parada? parada = null, Destino? destino = null)
    {
        Parada alvo = parada ?? _parada;
        ParadaLinha associacao = alvo.AssociarLinha(linha, Cenario.Agora);
        _dados.Percursos.Add(PercursoEstimado.CriarSimulado(associacao, destino ?? _norte, minutos));
    }

    private static PrevisaoChegada Previsao(Linha linha, int minutos, int segundos = 0) =>
        new(ReferenciaLinha.De(linha), Agora.AddMinutes(minutos).AddSeconds(segundos), null);

    private void Responder(params PrevisaoChegada[] previsoes) =>
        _provedor.Resultado = ResultadoProvedor.Respondido(OrigemDado.Simulado, Agora, previsoes);

    private static string[] Avisos(AlternativasDeslocamentoDto dto) => [.. dto.Avisos.Select(a => a.Codigo)];

    private static (int? Posicao, string Codigo, int? Total) Item(AlternativaDto a) => (a.Posicao, a.CodigoLinha, a.TempoTotalEstimadoMinutos);

    private static void Desativar(Destino destino) =>
        typeof(Destino).GetProperty(nameof(Destino.Ativo), BindingFlags.Instance | BindingFlags.Public)!.SetValue(destino, false);

    // ----- Validação e 404 -----

    [Fact]
    public async Task DestinoIdAusente_Responde400SemConsultarNada()
    {
        var erro = await Assert.ThrowsAsync<ErroAplicacaoException>(() => Service.ObterAsync(1, null, Ct));

        Assert.Equal(TipoErroAplicacao.Validacao, erro.Tipo);
        Assert.Equal("O campo destinoId é obrigatório.", Assert.Single(erro.Erros["destinoId"]));
        Assert.Empty(_dados.Chamadas);
        Assert.Empty(_provedor.Linhas);
        Assert.Equal(0, _relogio.Leituras);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public async Task DestinoIdMenorQueUm_Responde400(int destinoId)
    {
        var erro = await Assert.ThrowsAsync<ErroAplicacaoException>(() => Service.ObterAsync(1, destinoId, Ct));

        Assert.Equal("destinoId", Assert.Single(erro.Erros).Key);
        Assert.Empty(_dados.Chamadas);
    }

    [Fact]
    public async Task ParadaInexistenteOuInativa_Responde404ParadaNaoEncontrada()
    {
        var inexistente = await Assert.ThrowsAsync<ErroAplicacaoException>(() => Service.ObterAsync(99, 3, Ct));
        _parada.Desativar();
        var inativa = await Assert.ThrowsAsync<ErroAplicacaoException>(() => Service.ObterAsync(1, 3, Ct));

        Assert.Equal("ParadaNaoEncontrada", inexistente.Codigo);
        Assert.Equal("ParadaNaoEncontrada", inativa.Codigo);
        Assert.Empty(_provedor.Linhas);
    }

    [Fact]
    public async Task DestinoInexistenteOuInativo_Responde404DestinoNaoEncontrado()
    {
        var inexistente = await Assert.ThrowsAsync<ErroAplicacaoException>(() => Service.ObterAsync(1, 77, Ct));
        Desativar(_norte);
        var inativo = await Assert.ThrowsAsync<ErroAplicacaoException>(() => Service.ObterAsync(1, 3, Ct));

        Assert.Equal(("DestinoNaoEncontrado", "Não existe destino ativo com o id 77."), (inexistente.Codigo, inexistente.Message));
        Assert.Equal("DestinoNaoEncontrado", inativo.Codigo);
        Assert.Empty(_provedor.Linhas);
    }

    // ----- Ranking (exemplo do contrato) -----

    [Fact]
    public async Task Exemplo_TempoTotalVenceQuemChegaPrimeiro_ESemPrevisaoVaiPorUltimo()
    {
        Responder(Previsao(_l002, 1), Previsao(_l004, 8), Previsao(_l002, 19));

        AlternativasDeslocamentoDto dto = await Service.ObterAsync(1, 3, Ct);

        Assert.Equal([(1, "SIM-L004", 28), (2, "SIM-L002", 41), (null, "SIM-L001", null)], dto.Alternativas.Select(Item));
        AlternativaDto primeira = dto.Alternativas[0];
        Assert.Equal((8, 20), (primeira.EsperaMinutos, primeira.PercursoEstimadoMinutos));
        Assert.Equal(Agora.AddMinutes(8), primeira.PrevistaPara);
        Assert.Equal(Agora.AddMinutes(28), primeira.ChegadaEstimadaAoDestino);
        Assert.Equal(TimeSpan.FromHours(-3), primeira.ChegadaEstimadaAoDestino!.Value.Offset);
        Assert.Equal((SituacaoAlternativas.Comparadas, false, StatusPrevisao.Disponivel, OrigemDado.Simulado),
            (dto.Situacao, dto.ComparacaoCompleta, dto.StatusPrevisao, dto.OrigemPrevisao));
        Assert.Equal(CriterioAlternativas.MenorTempoTotalEstimado, dto.Criterio);
        Assert.Equal(["DadosSimulados", "LimitacaoLinhasDiretas", "PercursoEstimado", "ComparacaoIncompleta"], Avisos(dto));
    }

    [Fact]
    public async Task SoAProximaPrevisaoValidaDeCadaLinhaEntraNoCalculo()
    {
        // A SIM-L002 tem chegadas em 1 e 19 minutos: só a de 1 minuto conta.
        Responder(Previsao(_l002, 19), Previsao(_l002, 1), Previsao(_l004, 8), Previsao(_l001, 50));

        AlternativasDeslocamentoDto dto = await Service.ObterAsync(1, 3, Ct);

        AlternativaDto l002 = dto.Alternativas.Single(a => a.CodigoLinha == "SIM-L002");
        Assert.Equal((1, 41), (l002.EsperaMinutos, l002.TempoTotalEstimadoMinutos));
        Assert.Equal(3, dto.Alternativas.Count);
    }

    [Fact]
    public async Task TodasComparaveis_ComparacaoCompletaSemAvisoDeIncompleta()
    {
        Responder(Previsao(_l001, 2), Previsao(_l002, 1), Previsao(_l004, 8));

        AlternativasDeslocamentoDto dto = await Service.ObterAsync(1, 3, Ct);

        Assert.True(dto.ComparacaoCompleta);
        Assert.Equal([(1, "SIM-L004", 28), (2, "SIM-L001", 32), (3, "SIM-L002", 41)], dto.Alternativas.Select(Item));
        Assert.Equal(["DadosSimulados", "LimitacaoLinhasDiretas", "PercursoEstimado"], Avisos(dto));
    }

    [Fact]
    public async Task UmaLinhaComparavel_RecebePosicaoUm()
    {
        _dados.Percursos.RemoveAll(p => p.LinhaId != 4);
        Responder(Previsao(_l004, 3));

        AlternativasDeslocamentoDto dto = await Service.ObterAsync(1, 3, Ct);

        Assert.Equal([(1, "SIM-L004", 23)], dto.Alternativas.Select(Item));
        Assert.True(dto.ComparacaoCompleta);
    }

    [Fact]
    public async Task EmpateNoTempoTotal_GanhaAMenorEspera()
    {
        // SIM-L004: 10 + 20 = 30; SIM-L001: 0 + 30 = 30. Mesma soma; a menor espera vem antes.
        Responder(Previsao(_l004, 10), Previsao(_l001, 0, 30), Previsao(_l002, 5));

        AlternativasDeslocamentoDto dto = await Service.ObterAsync(1, 3, Ct);

        Assert.Equal([(1, "SIM-L001", 30), (2, "SIM-L004", 30), (3, "SIM-L002", 45)], dto.Alternativas.Select(Item));
    }

    [Fact]
    public async Task EmpateNoTempoTotalENaEspera_GanhaOMenorCodigo()
    {
        Linha l005 = Cenario.LinhaSimulada("SIM-L005", "SIM-105").ComId(5);
        _dados.Linhas.Add(l005);
        Percurso(l005, 20);
        Responder(Previsao(l005, 8), Previsao(_l004, 8));

        AlternativasDeslocamentoDto dto = await Service.ObterAsync(1, 3, Ct);

        Assert.Equal(["SIM-L004", "SIM-L005", "SIM-L001", "SIM-L002"], dto.Alternativas.Select(a => a.CodigoLinha));
        Assert.Equal([1, 2, null, null], dto.Alternativas.Select(a => a.Posicao));
    }

    [Fact]
    public async Task LinhasSemPrevisao_PorPercursoECodigo_SemPosicaoECamposNulos()
    {
        Linha l006 = Cenario.LinhaSimulada("SIM-L006", "SIM-106").ComId(6);
        _dados.Linhas.Add(l006);
        Percurso(l006, 30);
        Responder(Previsao(_l004, 8));

        AlternativasDeslocamentoDto dto = await Service.ObterAsync(1, 3, Ct);

        Assert.Equal(["SIM-L004", "SIM-L001", "SIM-L006", "SIM-L002"], dto.Alternativas.Select(a => a.CodigoLinha));
        Assert.All(dto.Alternativas.Skip(1), a =>
        {
            Assert.Equal(SituacaoAlternativa.SemPrevisao, a.Situacao);
            Assert.Null(a.Posicao);
            Assert.Null(a.PrevistaPara);
            Assert.Null(a.EsperaMinutos);
            Assert.Null(a.TempoTotalEstimadoMinutos);
            Assert.Null(a.ChegadaEstimadaAoDestino);
            Assert.True(a.PercursoEstimadoMinutos > 0);
            Assert.Equal((0, OrigemDado.Simulado), (a.Baldeacoes, a.OrigemPercurso));
        });
    }

    [Fact]
    public async Task PosicoesSaoConsecutivasSoNasComparaveis()
    {
        Responder(Previsao(_l002, 1), Previsao(_l004, 8));

        AlternativasDeslocamentoDto dto = await Service.ObterAsync(1, 3, Ct);

        Assert.Equal([1, 2], dto.Alternativas.Where(a => a.Situacao == SituacaoAlternativa.Comparavel).Select(a => a.Posicao!.Value));
        Assert.DoesNotContain(dto.Alternativas, a => a.Situacao == SituacaoAlternativa.SemPrevisao && a.Posicao is not null);
    }

    // ----- SemComparacao e SemLinhaDireta -----

    [Fact]
    public async Task ProvedorSemPrevisoes_SemComparacaoComSemPrevisoes()
    {
        Responder();

        AlternativasDeslocamentoDto dto = await Service.ObterAsync(1, 3, Ct);

        Assert.Equal((SituacaoAlternativas.SemComparacao, false, StatusPrevisao.SemPrevisoes), (dto.Situacao, dto.ComparacaoCompleta, dto.StatusPrevisao));
        Assert.All(dto.Alternativas, a => Assert.Null(a.Posicao));
        Assert.Equal(["SIM-L004", "SIM-L001", "SIM-L002"], dto.Alternativas.Select(a => a.CodigoLinha));
        Assert.Equal(["SemPrevisoes", "SemComparacao", "DadosSimulados", "LimitacaoLinhasDiretas", "PercursoEstimado"], Avisos(dto));
    }

    [Fact]
    public async Task ProvedorIndisponivel_SemComparacao200ComAvisoENaoExcecao()
    {
        _provedor.Resultado = ResultadoProvedor.Indisponivel(OrigemDado.Simulado, Agora);

        AlternativasDeslocamentoDto dto = await Service.ObterAsync(1, 3, Ct);

        Assert.Equal((SituacaoAlternativas.SemComparacao, StatusPrevisao.ProvedorIndisponivel, OrigemDado.Simulado), (dto.Situacao, dto.StatusPrevisao, dto.OrigemPrevisao));
        Assert.Equal(["ProvedorIndisponivel", "SemComparacao", "DadosSimulados", "LimitacaoLinhasDiretas", "PercursoEstimado"], Avisos(dto));
        Assert.All(dto.Alternativas, a => Assert.Equal(SituacaoAlternativa.SemPrevisao, a.Situacao));
    }

    [Fact]
    public async Task SemPercursoAteODestino_SemLinhaDiretaSemConsultarOProvedor()
    {
        AlternativasDeslocamentoDto dto = await Service.ObterAsync(1, 2, Ct);

        Assert.Empty(_provedor.Linhas);
        Assert.Equal((SituacaoAlternativas.SemLinhaDireta, false), (dto.Situacao, dto.ComparacaoCompleta));
        Assert.Null(dto.StatusPrevisao);
        Assert.Null(dto.OrigemPrevisao);
        Assert.Empty(dto.Alternativas);
        Assert.Equal(["SemLinhaDireta", "DadosSimulados", "LimitacaoLinhasDiretas"], Avisos(dto));
        Assert.Equal(new DestinoConsultadoDto(2, "Terminal Demonstração Sul", TipoDestino.Terminal), dto.Destino);
    }

    [Fact]
    public async Task ParadaSemLinhas_SemLinhaDiretaComParadaSemLinhas()
    {
        Parada vazia = Cenario.ParadaSimulada("SIM-P009", "Parada Sem Linhas").ComId(9);
        _dados.Paradas.Add(vazia);

        AlternativasDeslocamentoDto dto = await Service.ObterAsync(9, 3, Ct);

        Assert.Empty(_provedor.Linhas);
        Assert.Equal(["ParadaSemLinhas", "SemLinhaDireta", "DadosSimulados", "LimitacaoLinhasDiretas"], Avisos(dto));
    }

    [Fact]
    public async Task ProvedorRecebeSoAsLinhasDiretasEUmInstante()
    {
        Responder(Previsao(_l004, 8));

        await Service.ObterAsync(1, 3, Ct);

        Assert.Equal(1, _relogio.Leituras);
        Assert.Equal(["SIM-L004", "SIM-L001", "SIM-L002"], _provedor.Linhas.Single().Select(l => l.Codigo));
        Assert.Equal(Agora, _provedor.Referencias.Single());
        Assert.Equal(Ct, _provedor.Tokens.Single());
    }

    // ----- Natureza do dado e cadastro -----

    [Fact]
    public async Task CadastroAdministrativoNaoFicticio_TemCadastroNaoVerificadoUmaVez()
    {
        Parada real = Cenario.ParadaAdministrativa(dadosSimulados: false, codigo: "PT-0001").ComId(5);
        Linha linhaReal = Cenario.LinhaAdministrativa(dadosSimulados: false, codigo: "LT-0100").ComId(6);
        _dados.Paradas.Add(real);
        _dados.Linhas.Add(linhaReal);
        Percurso(linhaReal, 15, real);
        Responder(Previsao(linhaReal, 4));

        AlternativasDeslocamentoDto dto = await Service.ObterAsync(5, 3, Ct);

        // O percurso e o destino do seed são simulados, então dadosSimulados continua true; o cadastro não é verificado.
        Assert.True(dto.DadosSimulados);
        Assert.Equal(["DadosSimulados", "CadastroNaoVerificado", "LimitacaoLinhasDiretas", "PercursoEstimado"], Avisos(dto));
        Assert.Single(dto.Avisos, a => a.Codigo == "CadastroNaoVerificado");
    }

    [Fact]
    public async Task CancelamentoNoProvedor_EPropagado()
    {
        _provedor.Falha = new OperationCanceledException();

        await Assert.ThrowsAsync<OperationCanceledException>(() => Service.ObterAsync(1, 3, Ct));
    }

    /// <summary>Provedor falso: devolve <see cref="Resultado"/> e guarda as linhas, referências e tokens recebidos.</summary>
    private sealed class ProvedorFalso : IProvedorPrevisaoChegada
    {
        public ResultadoProvedor Resultado { get; set; } = ResultadoProvedor.Respondido(OrigemDado.Simulado, Agora, []);

        public Exception? Falha { get; set; }

        public List<IReadOnlyList<Linha>> Linhas { get; } = [];

        public List<DateTimeOffset> Referencias { get; } = [];

        public List<CancellationToken> Tokens { get; } = [];

        public OrigemDado OrigemPara(Parada parada) => OrigemDado.Simulado;

        public Task<ResultadoProvedor> ConsultarAsync(Parada parada, IReadOnlyList<Linha> linhas, DateTimeOffset referencia, CancellationToken cancellationToken)
        {
            Linhas.Add(linhas);
            Referencias.Add(referencia);
            Tokens.Add(cancellationToken);
            return Falha is null ? Task.FromResult(Resultado) : throw Falha;
        }
    }
}

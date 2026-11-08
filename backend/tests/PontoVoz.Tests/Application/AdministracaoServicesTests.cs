using PontoVoz.Api.Application.Dtos;
using PontoVoz.Api.Application.Erros;
using PontoVoz.Api.Application.Interfaces;
using PontoVoz.Api.Application.Services;
using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.Exceptions;
using PontoVoz.Tests.Domain;

namespace PontoVoz.Tests.Application;

/// <summary>
/// Regras dos services administrativos (B7) sem banco: validação antes de qualquer leitura ou
/// gravação, erros esperados, repasse do <see cref="CancellationToken"/>, leitura única do
/// <see cref="TimeProvider"/> e o tratamento de uma violação de unicidade (corrida entre
/// requisições). A gravação real no SQLite é testada em UnidadeTrabalhoEfTests e nos testes HTTP.
/// </summary>
public sealed class AdministracaoServicesTests
{
    private static readonly DateTimeOffset Agora = Cenario.Agora;

    private readonly DublesCadastro _dados = new();
    private readonly RelogioContador _relogio = new(Agora);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private AdministracaoParadasService Paradas => new(_dados, _dados, _dados, _relogio);

    private AdministracaoLinhasService Linhas => new(_dados, _dados, _relogio);

    private ObservacoesPassagemService Observacoes => new(_dados, _dados, _dados, _dados, _relogio);

    private static CadastroParadaDto ParadaValida(string codigo = "TESTE-P003") =>
        new(codigo, "Parada Cadastro de Teste", null, null, true);

    private static CadastroLinhaDto LinhaValida(string codigo = "TESTE-L005") =>
        new(codigo, "TESTE-105", "Terminal A", "Terminal B", SentidoLinha.PrincipalParaSecundario, true);

    private static async Task<ErroAplicacaoException> ValidacaoAsync(Func<Task> acao, params string[] campos)
    {
        var erro = await Assert.ThrowsAsync<ErroAplicacaoException>(acao);
        Assert.Equal(TipoErroAplicacao.Validacao, erro.Tipo);
        Assert.Equal(campos.Order(StringComparer.Ordinal), erro.Erros.Keys.Order(StringComparer.Ordinal));
        return erro;
    }

    /// <summary>Parada e linha simuladas associadas, com ids, como viriam do banco.</summary>
    private (Parada Parada, Linha Linha) CentroComL004()
    {
        Parada parada = Cenario.ParadaSimulada().ComId(1);
        Linha linha = Cenario.LinhaSimulada().ComId(4);
        parada.AssociarLinha(linha, Agora);
        _dados.Paradas.Add(parada);
        _dados.Linhas.Add(linha);
        return (parada, linha);
    }

    // ----- Cadastro de parada -----

    [Fact]
    public async Task CadastroDeParada_Valido_AdicionaGravaUmaVezERepassaOToken()
    {
        ParadaAdministrativaDto dto = await Paradas.CadastrarAsync(ParadaValida(), Ct);

        Assert.Equal(["CodigoParada", "AdicionarParada", "Salvar"], _dados.Chamadas);
        Assert.All(_dados.Tokens, token => Assert.Equal(Ct, token));
        Parada parada = Assert.Single(_dados.Paradas);
        Assert.Equal((OrigemDado.CadastroAdministrativo, true, true), (parada.Origem, parada.DadosSimulados, parada.Ativa));
        Assert.Equal(Agora, dto.CriadoEm);
        Assert.Equal(TimeSpan.FromHours(-3), dto.CriadoEm.Offset);
        Assert.Equal(1, _relogio.Leituras);
    }

    [Fact]
    public async Task CadastroDeParada_Invalido_NaoConsultaNemGrava()
    {
        await ValidacaoAsync(() => Paradas.CadastrarAsync(new CadastroParadaDto("SIM-X", null, null, null, null), Ct),
            "codigoExterno", "nome", "dadosSimulados");

        Assert.Empty(_dados.Chamadas);
        Assert.Equal(0, _relogio.Leituras);
    }

    [Fact]
    public async Task CadastroDeParada_SemCorpo_ErroEmCorpo()
    {
        ErroAplicacaoException erro = await ValidacaoAsync(() => Paradas.CadastrarAsync(null!, Ct), "corpo");

        Assert.Equal("O corpo da requisição é obrigatório.", Assert.Single(erro.Erros["corpo"]));
        Assert.Empty(_dados.Chamadas);
    }

    [Fact]
    public async Task CadastroDeParada_FicticiaComEndereco_ERecusadaPeloDominio()
    {
        var erro = await Assert.ThrowsAsync<ValidacaoDominioException>(() =>
            Paradas.CadastrarAsync(new CadastroParadaDto("TESTE-P004", "Parada Fictícia", "Rua Real, 1", null, true), Ct));

        Assert.Equal("endereco", erro.Campo);
        Assert.DoesNotContain("Salvar", _dados.Chamadas);
    }

    [Fact]
    public async Task CadastroDeParada_ComCodigoExistente_Responde409SemGravar()
    {
        _dados.Paradas.Add(Cenario.ParadaAdministrativa(dadosSimulados: true, codigo: "TESTE-P003").ComId(9));

        var erro = await Assert.ThrowsAsync<ErroAplicacaoException>(() => Paradas.CadastrarAsync(ParadaValida(), Ct));

        Assert.Equal((TipoErroAplicacao.Conflito, "CodigoJaCadastrado"), (erro.Tipo, erro.Codigo));
        Assert.Equal(["CodigoParada"], _dados.Chamadas);
    }

    [Fact]
    public async Task CadastroDeParada_QuandoOutraRequisicaoGravaOMesmoCodigo_Responde409()
    {
        // A verificação prévia passou, mas o índice único recusou na gravação (corrida).
        _dados.FalhaAoSalvar = new ViolacaoUnicidadeException(new InvalidOperationException("detalhe interno"));

        var erro = await Assert.ThrowsAsync<ErroAplicacaoException>(() => Paradas.CadastrarAsync(ParadaValida(), Ct));

        Assert.Equal((TipoErroAplicacao.Conflito, "CodigoJaCadastrado"), (erro.Tipo, erro.Codigo));
        Assert.DoesNotContain("detalhe interno", erro.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CadastroDeParada_OutraFalhaDeGravacao_NaoViraConflito()
    {
        _dados.FalhaAoSalvar = new InvalidOperationException("Falha inesperada do banco.");

        await Assert.ThrowsAsync<InvalidOperationException>(() => Paradas.CadastrarAsync(ParadaValida(), Ct));
    }

    // ----- Atualização de parada -----

    [Fact]
    public async Task AtualizacaoDeParada_Invalida_NaoProcuraAParada()
    {
        await ValidacaoAsync(() => Paradas.AtualizarAsync(1, new AtualizacaoParadaDto("ab", null, null, null), Ct), "nome", "ativa");

        Assert.Empty(_dados.Chamadas);
    }

    [Fact]
    public async Task AtualizacaoDeParada_Inexistente_Responde404DepoisDaValidacao()
    {
        var erro = await Assert.ThrowsAsync<ErroAplicacaoException>(() =>
            Paradas.AtualizarAsync(77, new AtualizacaoParadaDto("Nome Novo", null, null, true), Ct));

        Assert.Equal((TipoErroAplicacao.NaoEncontrado, "ParadaNaoEncontrada"), (erro.Tipo, erro.Codigo));
        Assert.Equal("Não existe parada com o id 77.", erro.Message);
        Assert.Equal(["ObterParada"], _dados.Chamadas);
    }

    [Fact]
    public async Task AtualizacaoDeParada_DesativaEPreservaCodigoOrigemENatureza()
    {
        var (parada, _) = CentroComL004();

        ParadaAdministrativaDto dto = await Paradas.AtualizarAsync(parada.Id, new AtualizacaoParadaDto("Centro Renomeado", null, null, false), Ct);

        Assert.Equal(("Centro Renomeado", false, "SIM-P001", OrigemDado.Simulado, true), (dto.Nome, dto.Ativa, dto.CodigoExterno, dto.Origem, dto.DadosSimulados));
        Assert.Single(parada.LinhasAssociadas);
        Assert.Equal(["ObterParada", "Salvar"], _dados.Chamadas);
    }

    // ----- Associação -----

    [Fact]
    public async Task Associacao_Repetida_NaoCriaOutraAssociacao()
    {
        var (parada, linha) = CentroComL004();

        await Paradas.AssociarLinhaAsync(parada.Id, linha.Id, Ct);
        await Paradas.AssociarLinhaAsync(parada.Id, linha.Id, Ct);

        Assert.Single(parada.LinhasAssociadas);
    }

    [Fact]
    public async Task Associacao_QuandoOutraRequisicaoGravaAMesma_ContinuaIdempotente()
    {
        Parada parada = Cenario.ParadaAdministrativa(dadosSimulados: true).ComId(3);
        Linha linha = Cenario.LinhaAdministrativa(dadosSimulados: true).ComId(5);
        _dados.Paradas.Add(parada);
        _dados.Linhas.Add(linha);
        _dados.FalhaAoSalvar = new ViolacaoUnicidadeException(new InvalidOperationException("PRIMARY KEY"));

        await Paradas.AssociarLinhaAsync(parada.Id, linha.Id, Ct);

        Assert.Equal(["ObterParada", "ObterLinha", "Salvar"], _dados.Chamadas);
    }

    [Fact]
    public async Task Associacao_ComParadaInativa_RecusadaPeloDominioSemGravar()
    {
        Parada parada = Cenario.ParadaAdministrativa(dadosSimulados: true).ComId(3);
        parada.Desativar();
        _dados.Paradas.Add(parada);
        _dados.Linhas.Add(Cenario.LinhaAdministrativa(dadosSimulados: true).ComId(5));

        var erro = await Assert.ThrowsAsync<RegraNegocioException>(() => Paradas.AssociarLinhaAsync(3, 5, Ct));

        Assert.Equal(RegraNegocioException.ParadaInativa, erro.Codigo);
        Assert.DoesNotContain("Salvar", _dados.Chamadas);
    }

    [Fact]
    public async Task Associacao_ComParadaOuLinhaInexistente_Responde404NaOrdem()
    {
        _dados.Linhas.Add(Cenario.LinhaAdministrativa(dadosSimulados: true).ComId(5));

        var semParada = await Assert.ThrowsAsync<ErroAplicacaoException>(() => Paradas.AssociarLinhaAsync(3, 99, Ct));
        _dados.Paradas.Add(Cenario.ParadaAdministrativa(dadosSimulados: true).ComId(3));
        var semLinha = await Assert.ThrowsAsync<ErroAplicacaoException>(() => Paradas.AssociarLinhaAsync(3, 99, Ct));

        Assert.Equal("ParadaNaoEncontrada", semParada.Codigo);
        Assert.Equal("LinhaNaoEncontrada", semLinha.Codigo);
    }

    // ----- Cadastro de linha -----

    [Fact]
    public async Task CadastroDeLinha_Valido_NasceAtivaEAdministrativa()
    {
        LinhaAdministrativaDto dto = await Linhas.CadastrarAsync(LinhaValida(), Ct);

        Assert.Equal((OrigemDado.CadastroAdministrativo, true, true), (dto.Origem, dto.DadosSimulados, dto.Ativa));
        Assert.Equal(["CodigoLinha", "AdicionarLinha", "Salvar"], _dados.Chamadas);
    }

    [Fact]
    public async Task CadastroDeLinha_SemNaturezaEComSentidoInvalido_DevolveOsDoisErros()
    {
        await ValidacaoAsync(() => Linhas.CadastrarAsync(LinhaValida() with { Sentido = (SentidoLinha)99, DadosSimulados = null }, Ct),
            "sentido", "dadosSimulados");

        Assert.Empty(_dados.Chamadas);
    }

    [Fact]
    public async Task CadastroDeLinha_QuandoOutraRequisicaoGravaOMesmoCodigo_Responde409()
    {
        _dados.FalhaAoSalvar = new ViolacaoUnicidadeException(new InvalidOperationException("UNIQUE"));

        var erro = await Assert.ThrowsAsync<ErroAplicacaoException>(() => Linhas.CadastrarAsync(LinhaValida(), Ct));

        Assert.Equal("CodigoJaCadastrado", erro.Codigo);
    }

    // ----- Observação de passagem -----

    [Fact]
    public async Task Observacao_Valida_UsaUmInstanteParaJanelaRegistroEMinutos()
    {
        var (parada, linha) = CentroComL004();

        PassagemRegistradaDto dto = await Observacoes.RegistrarAsync(
            new ObservacaoPassagemDto(parada.Id, linha.Id, Agora.AddMinutes(-7), "SIM-V1", "operador-local"), Ct);

        Assert.Equal(1, _relogio.Leituras);
        Assert.Equal((7, false, NivelConfirmacao.RegistroManual, false, false), (dto.MinutosDesdeObservacao, dto.Desatualizada, dto.NivelConfirmacao, dto.Verificada, dto.OficialSptrans));
        Assert.Equal(Agora, dto.RegistradaEm);
        Assert.Equal(parada.Id, dto.ParadaId);
        PassagemRegistroManual gravada = Assert.IsType<PassagemRegistroManual>(Assert.Single(_dados.Passagens));
        Assert.Equal("operador-local", gravada.OrigemRegistro);
        Assert.Equal(["ObterParada", "ObterLinha", "AdicionarPassagem", "Salvar"], _dados.Chamadas);
        Assert.All(_dados.Tokens, token => Assert.Equal(Ct, token));
    }

    [Fact]
    public async Task Observacao_ForaDaJanela_Responde400SemConsultarOBanco()
    {
        CentroComL004();

        ErroAplicacaoException erro = await ValidacaoAsync(() => Observacoes.RegistrarAsync(
            new ObservacaoPassagemDto(1, 4, Agora.AddHours(-24).AddSeconds(-1), null, "operador-local"), Ct), "observadaEm");

        Assert.Equal(PassagemRegistroManual.MensagemForaDaJanela, Assert.Single(erro.Erros["observadaEm"]));
        Assert.Empty(_dados.Chamadas);
    }

    [Fact]
    public async Task Observacao_SemCampos_DevolveTodosOsErrosDeUmaVez()
    {
        await ValidacaoAsync(() => Observacoes.RegistrarAsync(new ObservacaoPassagemDto(null, 0, null, "SIM V", "Operador"), Ct),
            "paradaId", "linhaId", "observadaEm", "prefixoVeiculo", "origemRegistro");

        Assert.Empty(_dados.Chamadas);
    }

    [Fact]
    public async Task Observacao_ComLinhaQueNaoAtende_RecusadaPeloDominioSemGravar()
    {
        var (parada, _) = CentroComL004();
        Linha outra = Cenario.LinhaSimulada("SIM-L003", "SIM-203").ComId(3);
        _dados.Linhas.Add(outra);

        var erro = await Assert.ThrowsAsync<RegraNegocioException>(() => Observacoes.RegistrarAsync(
            new ObservacaoPassagemDto(parada.Id, outra.Id, Agora, null, "operador-local"), Ct));

        Assert.Equal(RegraNegocioException.LinhaNaoAtendeParada, erro.Codigo);
        Assert.Empty(_dados.Passagens);
        Assert.DoesNotContain("Salvar", _dados.Chamadas);
    }

    [Fact]
    public async Task Observacao_PrefixoDeVeiculoSegueANaturezaDoRegistro()
    {
        var (parada, linha) = CentroComL004();

        var erro = await Assert.ThrowsAsync<ValidacaoDominioException>(() => Observacoes.RegistrarAsync(
            new ObservacaoPassagemDto(parada.Id, linha.Id, Agora, "V1234", "operador-local"), Ct));

        Assert.Equal("prefixoVeiculo", erro.Campo);
    }

    [Fact]
    public async Task Operacoes_ComTokenCancelado_NaoGravam()
    {
        CentroComL004();
        using var cancelamento = new CancellationTokenSource();
        await cancelamento.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Paradas.CadastrarAsync(ParadaValida(), cancelamento.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Observacoes.RegistrarAsync(
            new ObservacaoPassagemDto(1, 4, Agora, null, "operador-local"), cancelamento.Token));

        Assert.DoesNotContain("Salvar", _dados.Chamadas);
        Assert.Empty(_dados.Passagens);
    }

    /// <summary>
    /// Repositórios de escrita e unidade de trabalho em memória. Guardam a ordem das chamadas e os
    /// tokens recebidos; respeitam o cancelamento como o EF Core (lançam se o token já foi cancelado).
    /// </summary>
    private sealed class DublesCadastro : ICadastroParadaRepository, ICadastroLinhaRepository, ICadastroPassagemRepository, IUnidadeTrabalho
    {
        public List<Parada> Paradas { get; } = [];

        public List<Linha> Linhas { get; } = [];

        public List<RegistroPassagem> Passagens { get; } = [];

        public List<string> Chamadas { get; } = [];

        public List<CancellationToken> Tokens { get; } = [];

        public Exception? FalhaAoSalvar { get; set; }

        public Task<bool> CodigoExternoExisteAsync(string codigoExterno, CancellationToken cancellationToken) =>
            Registrar("CodigoParada", cancellationToken, Paradas.Exists(p => p.CodigoExterno == codigoExterno));

        Task<Parada?> ICadastroParadaRepository.ObterParaAlteracaoAsync(int id, CancellationToken cancellationToken) =>
            Registrar("ObterParada", cancellationToken, Paradas.SingleOrDefault(p => p.Id == id));

        void ICadastroParadaRepository.Adicionar(Parada parada)
        {
            Chamadas.Add("AdicionarParada");
            Paradas.Add(parada);
        }

        public Task<bool> CodigoExisteAsync(string codigo, CancellationToken cancellationToken) =>
            Registrar("CodigoLinha", cancellationToken, Linhas.Exists(l => l.Codigo == codigo));

        Task<Linha?> ICadastroLinhaRepository.ObterParaAlteracaoAsync(int id, CancellationToken cancellationToken) =>
            Registrar("ObterLinha", cancellationToken, Linhas.SingleOrDefault(l => l.Id == id));

        void ICadastroLinhaRepository.Adicionar(Linha linha)
        {
            Chamadas.Add("AdicionarLinha");
            Linhas.Add(linha);
        }

        public void Adicionar(RegistroPassagem passagem)
        {
            Chamadas.Add("AdicionarPassagem");
            Passagens.Add(passagem);
        }

        public async Task SalvarAsync(CancellationToken cancellationToken)
        {
            await Registrar("Salvar", cancellationToken, true);
            if (FalhaAoSalvar is not null)
            {
                throw FalhaAoSalvar;
            }
        }

        private Task<T> Registrar<T>(string chamada, CancellationToken cancellationToken, T resultado)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Chamadas.Add(chamada);
            Tokens.Add(cancellationToken);
            return Task.FromResult(resultado);
        }
    }
}

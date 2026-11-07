using System.Text.Json.Nodes;

using PontoVoz.Api.Application.Consultas;
using PontoVoz.Api.Application.Dtos;
using PontoVoz.Api.Application.Erros;
using PontoVoz.Api.Application.Services;
using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.ValueObjects;
using PontoVoz.Tests.Domain;

namespace PontoVoz.Tests.Application;

/// <summary>Validação e montagem de respostas dos services de paradas, linhas e destinos, com repositórios em memória.</summary>
public class ServicosConsultaTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private readonly DublesRepositorio _dados = new();

    private ParadaService Paradas => new(_dados, _dados);

    private static async Task<ErroAplicacaoException> ValidacaoAsync(Func<Task> acao)
    {
        var erro = await Assert.ThrowsAsync<ErroAplicacaoException>(acao);
        Assert.Equal((TipoErroAplicacao.Validacao, "RequisicaoInvalida"), (erro.Tipo, erro.Codigo));
        return erro;
    }

    [Fact]
    public async Task Paradas_ComTermoCurtoETamanhoDePaginaGrande_DevolvemOsErrosDoExemploDoContrato_SemConsultar()
    {
        var erro = await ValidacaoAsync(() => Paradas.BuscarAsync(new ConsultaParadas(Termo: "a", TamanhoPagina: 100), Ct));

        JsonNode esperado = ExemplosContrato.Exemplo("erro-validacao.json");
        Assert.True(JsonNode.DeepEquals(esperado["errors"], ExemplosContrato.Serializar(erro.Erros)));
        Assert.Equal(esperado["title"]!.GetValue<string>(), erro.Titulo);
        Assert.Empty(_dados.Chamadas);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData("  centro  ", "centro")]
    [InlineData("ab", "ab")]
    public async Task Paradas_TermoAusenteOuValido_ChegaAoRepositorioSemEspacosNasPontas(string? termo, string? esperado)
    {
        await Paradas.BuscarAsync(new ConsultaParadas(Termo: termo), Ct);

        Assert.Equal(esperado, _dados.UltimaBuscaParadas.Termo);
    }

    [Fact]
    public async Task Paradas_TermoCom60Caracteres_EAceito_ECom61_ERecusado()
    {
        await Paradas.BuscarAsync(new ConsultaParadas(Termo: new string('a', 60)), Ct);

        var erro = await ValidacaoAsync(() => Paradas.BuscarAsync(new ConsultaParadas(Termo: new string('a', 61)), Ct));
        Assert.Equal(new[] { "termo" }, erro.Erros.Keys);
    }

    [Theory]
    [InlineData("AB")]
    [InlineData("SIM P001")]
    [InlineData("SIM_P001")]
    [InlineData("SIM-PÇ01")]
    [InlineData("SIM-0000000000000000000000000001")]
    public async Task Paradas_CodigoExternoForaDoFormato_ERecusado(string codigo)
    {
        var erro = await ValidacaoAsync(() => Paradas.BuscarAsync(new ConsultaParadas(CodigoExterno: codigo), Ct));

        Assert.Equal(new[] { "codigoExterno" }, erro.Erros.Keys);
        Assert.Empty(_dados.Chamadas);
    }

    [Theory]
    [InlineData("SIM-P001")]
    [InlineData("sim-p001")]
    [InlineData(" SIM-P001 ")]
    public async Task Paradas_CodigoExternoValido_ChegaAoRepositorio(string codigo)
    {
        await Paradas.BuscarAsync(new ConsultaParadas(CodigoExterno: codigo), Ct);

        Assert.Equal(codigo.Trim(), _dados.UltimaBuscaParadas.Codigo);
    }

    [Theory]
    [InlineData(0, 20, "pagina")]
    [InlineData(-3, 20, "pagina")]
    [InlineData(1, 0, "tamanhoPagina")]
    [InlineData(1, 51, "tamanhoPagina")]
    public async Task Paradas_PaginacaoForaDaFaixa_ERecusada(int pagina, int tamanho, string campo)
    {
        var erro = await ValidacaoAsync(() => Paradas.BuscarAsync(new ConsultaParadas(Pagina: pagina, TamanhoPagina: tamanho), Ct));

        Assert.Equal(new[] { campo }, erro.Erros.Keys);
    }

    [Fact]
    public async Task Paradas_SemPaginacao_UsamPagina1ETamanho20()
    {
        await Paradas.BuscarAsync(new ConsultaParadas(), Ct);

        Assert.Equal(new Paginacao(1, 20), _dados.UltimaBuscaParadas.Paginacao);
    }

    [Fact]
    public async Task Paradas_VariosErros_SaoDevolvidosJuntos()
    {
        var erro = await ValidacaoAsync(() => Paradas.BuscarAsync(new ConsultaParadas("x", "!", 0, 99), Ct));

        Assert.Equal(new[] { "termo", "codigoExterno", "pagina", "tamanhoPagina" }, erro.Erros.Keys);
    }

    [Fact]
    public async Task Paradas_SemResultado_DevolvemPaginaVaziaComTotalPaginasZero()
    {
        PaginaDto<ParadaDto> pagina = await Paradas.BuscarAsync(new ConsultaParadas(Termo: "inexistente"), Ct);

        Assert.Empty(pagina.Itens);
        Assert.Equal((1, 20, 0, 0), (pagina.Pagina, pagina.TamanhoPagina, pagina.TotalItens, pagina.TotalPaginas));
    }

    [Fact]
    public async Task Paradas_MapeiamOsCamposDoContrato_ComNaturezaDoDado()
    {
        _dados.Paradas.Add(Parada.CadastrarAdministrativa("PARADA-0001", "Parada Real", "Rua Exemplo, 1", new Coordenada(-23.5, -46.6), false, Cenario.Agora).ComId(7));
        _dados.Paradas.Add(Cenario.ParadaSimulada().ComId(8));

        PaginaDto<ParadaDto> pagina = await Paradas.BuscarAsync(new ConsultaParadas(), Ct);

        Assert.Equal(
            new ParadaDto(7, "PARADA-0001", "Parada Real", "Rua Exemplo, 1", new CoordenadaDto(-23.5, -46.6), OrigemDado.CadastroAdministrativo, false),
            pagina.Itens[0]);
        Assert.Equal(new ParadaDto(8, "SIM-P001", "Parada Demonstração Centro", null, null, OrigemDado.Simulado, true), pagina.Itens[1]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Detalhe_ComIdMenorQue1_ENaoEncontrado_SemConsultar(int id)
    {
        var erro = await Assert.ThrowsAsync<ErroAplicacaoException>(() => Paradas.ObterDetalheAsync(id, Ct));

        Assert.Equal((TipoErroAplicacao.NaoEncontrado, "ParadaNaoEncontrada"), (erro.Tipo, erro.Codigo));
        Assert.Empty(_dados.Chamadas);
    }

    [Fact]
    public async Task Detalhe_DeParadaInexistenteOuInativa_UsaOTituloEODetalheDoExemplo()
    {
        Parada inativa = Cenario.ParadaSimulada().ComId(99);
        inativa.Desativar();
        _dados.Paradas.Add(inativa);

        var erro = await Assert.ThrowsAsync<ErroAplicacaoException>(() => Paradas.ObterDetalheAsync(99, Ct));

        JsonNode exemplo = ExemplosContrato.Exemplo("erro-parada-nao-encontrada.json");
        Assert.Equal(exemplo["codigo"]!.GetValue<string>(), erro.Codigo);
        Assert.Equal(exemplo["title"]!.GetValue<string>(), erro.Titulo);
        Assert.Equal(exemplo["detail"]!.GetValue<string>(), erro.Message);
    }

    [Fact]
    public async Task Detalhe_TrazSoAsLinhasAtivasQueAtendemAParada()
    {
        Parada parada = Cenario.ParadaSimulada().ComId(1);
        Linha ativa = Cenario.LinhaSimulada("SIM-L001", "SIM-101").ComId(1);
        Linha inativa = Cenario.LinhaSimulada("SIM-L002", "SIM-102").ComId(2);
        parada.AssociarLinha(ativa, Cenario.Agora);
        parada.AssociarLinha(inativa, Cenario.Agora);
        inativa.Desativar();
        _dados.Paradas.Add(parada);

        ParadaDetalheDto detalhe = await Paradas.ObterDetalheAsync(1, Ct);

        LinhaDto linha = Assert.Single(detalhe.Linhas);
        Assert.Equal(new LinhaDto(1, "SIM-L001", "SIM-101", "Terminal Demonstração Centro", "Terminal Demonstração Norte", SentidoLinha.PrincipalParaSecundario, OrigemDado.Simulado, true), linha);
    }

    [Fact]
    public async Task Linhas_EDestinos_ValidamTermoEPaginacao()
    {
        await ValidacaoAsync(() => new LinhaService(_dados).BuscarAsync(new ConsultaPaginada(Termo: "1"), Ct));
        await ValidacaoAsync(() => new LinhaService(_dados).BuscarAsync(new ConsultaPaginada(TamanhoPagina: 51), Ct));
        await ValidacaoAsync(() => new DestinoService(_dados).BuscarAsync(new ConsultaPaginada(Termo: new string('x', 61)), Ct));
        await ValidacaoAsync(() => new DestinoService(_dados).BuscarAsync(new ConsultaPaginada(Pagina: 0), Ct));

        Assert.Empty(_dados.Chamadas);
    }

    [Fact]
    public async Task Linhas_EDestinos_MapeiamSentidoTerminaisETipo()
    {
        _dados.Linhas.Add(Linha.CriarSimulada("SIM-L009", "SIM-109", "Terminal A", "Terminal B", SentidoLinha.SecundarioParaPrincipal, Cenario.Agora).ComId(9));
        _dados.Destinos.Add(Destino.CriarSimulado("Estação Demonstração", TipoDestino.Estacao, Cenario.Agora).ComId(5));

        LinhaDto linha = Assert.Single((await new LinhaService(_dados).BuscarAsync(new ConsultaPaginada(), Ct)).Itens);
        DestinoDto destino = Assert.Single((await new DestinoService(_dados).BuscarAsync(new ConsultaPaginada(), Ct)).Itens);

        Assert.Equal(new LinhaDto(9, "SIM-L009", "SIM-109", "Terminal A", "Terminal B", SentidoLinha.SecundarioParaPrincipal, OrigemDado.Simulado, true), linha);
        Assert.Equal(new DestinoDto(5, "Estação Demonstração", TipoDestino.Estacao, OrigemDado.Simulado, true), destino);
    }
}

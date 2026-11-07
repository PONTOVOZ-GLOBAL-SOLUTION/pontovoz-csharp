using System.Reflection;

using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.Exceptions;

namespace PontoVoz.Tests.Domain;

public class LinhaTests
{
    [Fact]
    public void CriarSimulada_ComDadosValidos_CriaLinhaAtivaSimulada()
    {
        Linha linha = Linha.CriarSimulada("SIM-L004", " SIM-104 ", "Terminal Demonstração Centro", "Terminal Demonstração Norte", SentidoLinha.SecundarioParaPrincipal, Cenario.Agora);

        Assert.Equal("SIM-L004", linha.Codigo);
        Assert.Equal("SIM-104", linha.Letreiro);
        Assert.Equal("Terminal Demonstração Centro", linha.TerminalPrincipal);
        Assert.Equal("Terminal Demonstração Norte", linha.TerminalSecundario);
        Assert.Equal(SentidoLinha.SecundarioParaPrincipal, linha.Sentido);
        Assert.True(linha.Ativa);
        Assert.Equal(OrigemDado.Simulado, linha.Origem);
        Assert.True(linha.DadosSimulados);
    }

    [Fact]
    public void CadaSentido_EUmCadastroProprio()
    {
        Linha ida = Linha.CriarSimulada("SIM-L004", "SIM-104", "Centro", "Norte", SentidoLinha.PrincipalParaSecundario, Cenario.Agora);
        Linha volta = Linha.CriarSimulada("SIM-L004V", "SIM-104", "Centro", "Norte", SentidoLinha.SecundarioParaPrincipal, Cenario.Agora);

        Assert.NotEqual(ida.Codigo, volta.Codigo);
        Assert.False(ida.MesmaEntidade(volta));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("L1")]
    [InlineData("sim-l001")]
    [InlineData("SIM L001")]
    public void Codigo_Invalido_ERecusado(string? codigo)
    {
        var excecao = Assert.Throws<ValidacaoDominioException>(() =>
            Linha.CriarSimulada(codigo!, "SIM-101", "Centro", "Norte", SentidoLinha.PrincipalParaSecundario, Cenario.Agora));

        Assert.Equal("codigo", excecao.Campo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("TESTE-1050000000000000000000001")]
    public void Letreiro_VazioOuCom31Caracteres_ERecusado(string letreiro)
    {
        var excecao = Assert.Throws<ValidacaoDominioException>(() =>
            Linha.CadastrarAdministrativa("TESTE-L005", letreiro, "Centro", "Sul", SentidoLinha.PrincipalParaSecundario, true, Cenario.Agora));

        Assert.Equal("letreiro", excecao.Campo);
    }

    [Theory]
    [InlineData("5")]
    [InlineData("8000-10")]
    public void Letreiro_CurtoDeLinhaAdministrativa_EAceito(string letreiro)
    {
        Linha linha = Linha.CadastrarAdministrativa("LINHA-8000", letreiro, "Centro", "Sul", SentidoLinha.PrincipalParaSecundario, false, Cenario.Agora);

        Assert.Equal(letreiro, linha.Letreiro);
    }

    [Theory]
    [InlineData("SIM-L005", "TESTE-105", "codigo")]
    [InlineData("TESTE-L005", "SIM-105", "letreiro")]
    [InlineData("TESTE-L005", "sim-105", "letreiro")]
    public void CadastrarAdministrativa_ComPrefixoReservadoSim_ERecusada(string codigo, string letreiro, string campo)
    {
        var excecao = Assert.Throws<ValidacaoDominioException>(() =>
            Linha.CadastrarAdministrativa(codigo, letreiro, "Centro", "Sul", SentidoLinha.PrincipalParaSecundario, true, Cenario.Agora));

        Assert.Equal(campo, excecao.Campo);
    }

    [Theory]
    [InlineData("TESTE-L001", "SIM-101", "codigo")]
    [InlineData("SIM-L001", "101", "letreiro")]
    public void CriarSimulada_SemPrefixoSim_ERecusada(string codigo, string letreiro, string campo)
    {
        var excecao = Assert.Throws<ValidacaoDominioException>(() =>
            Linha.CriarSimulada(codigo, letreiro, "Centro", "Norte", SentidoLinha.PrincipalParaSecundario, Cenario.Agora));

        Assert.Equal(campo, excecao.Campo);
    }

    [Theory]
    [InlineData("AB", "Terminal Norte", "terminalPrincipal")]
    [InlineData("Terminal Centro", "", "terminalSecundario")]
    public void Terminais_ComMenosDe3Caracteres_SaoRecusados(string principal, string secundario, string campo)
    {
        var excecao = Assert.Throws<ValidacaoDominioException>(() =>
            Linha.CriarSimulada("SIM-L001", "SIM-101", principal, secundario, SentidoLinha.PrincipalParaSecundario, Cenario.Agora));

        Assert.Equal(campo, excecao.Campo);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(-1)]
    public void Sentido_ForaDoEnum_ERecusado(int sentido)
    {
        var excecao = Assert.Throws<ValidacaoDominioException>(() =>
            Linha.CriarSimulada("SIM-L001", "SIM-101", "Centro", "Norte", (SentidoLinha)sentido, Cenario.Agora));

        Assert.Equal("sentido", excecao.Campo);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CadastrarAdministrativa_MantemANaturezaDeclarada(bool dadosSimulados)
    {
        Linha linha = Cenario.LinhaAdministrativa(dadosSimulados);

        Assert.Equal(OrigemDado.CadastroAdministrativo, linha.Origem);
        Assert.Equal(dadosSimulados, linha.DadosSimulados);
    }

    [Fact]
    public void DesativarEReativar_AlternaOEstado()
    {
        Linha linha = Cenario.LinhaSimulada();

        linha.Desativar();
        Assert.False(linha.Ativa);

        linha.Ativar();
        Assert.True(linha.Ativa);
    }

    [Theory]
    [InlineData(typeof(Parada))]
    [InlineData(typeof(Linha))]
    [InlineData(typeof(ParadaLinha))]
    [InlineData(typeof(Destino))]
    [InlineData(typeof(PercursoEstimado))]
    [InlineData(typeof(PassagemSimulada))]
    [InlineData(typeof(PassagemRegistroManual))]
    public void Entidades_NaoExpoemSetterPublico(Type tipo)
    {
        IEnumerable<string> comSetterPublico = tipo
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(propriedade => propriedade.SetMethod?.IsPublic == true)
            .Select(propriedade => propriedade.Name);

        Assert.Empty(comSetterPublico);
    }

    [Theory]
    [InlineData(typeof(Parada))]
    [InlineData(typeof(Linha))]
    [InlineData(typeof(ParadaLinha))]
    [InlineData(typeof(Destino))]
    [InlineData(typeof(PercursoEstimado))]
    [InlineData(typeof(PassagemSimulada))]
    [InlineData(typeof(PassagemRegistroManual))]
    public void Entidades_TemConstrutorSemParametrosNaoPublicoParaOEfCore(Type tipo)
    {
        ConstructorInfo? construtor = tipo.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, Type.EmptyTypes);

        Assert.NotNull(construtor);
        Assert.Empty(tipo.GetConstructors(BindingFlags.Instance | BindingFlags.Public));
    }
}

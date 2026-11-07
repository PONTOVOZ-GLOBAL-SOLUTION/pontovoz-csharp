using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.Exceptions;
using PontoVoz.Api.Domain.ValueObjects;

namespace PontoVoz.Tests.Domain;

/// <summary>
/// Os formatos exigem a entrada inteira. Quebras de linha e caracteres fora do formato são
/// recusados em qualquer posição, inclusive no final, que o <c>$</c> do .NET aceitaria.
/// </summary>
public class FormatoEntradaTests
{
    public static TheoryData<string> CodigosInvalidos =>
    [
        "\nSIM-P001",
        "SIM-P001\n",
        "SIM-P001\r\n",
        "SIM-\nP001",
        "SIM-P001 ",
        "SIM-P001\t",
        "SIM-P#01",
        "SIM-P001 ",
    ];

    [Theory]
    [MemberData(nameof(CodigosInvalidos))]
    public void CodigoDeParada_ComQuebraDeLinhaOuCaractereForaDoFormato_ERecusado(string codigo)
    {
        var excecao = Assert.Throws<ValidacaoDominioException>(() => Parada.CriarSimulada(codigo, "Parada Válida", Cenario.Agora));

        Assert.Equal("codigoExterno", excecao.Campo);
    }

    [Theory]
    [MemberData(nameof(CodigosInvalidos))]
    public void CodigoDeLinha_ComQuebraDeLinhaOuCaractereForaDoFormato_ERecusado(string codigo)
    {
        var excecao = Assert.Throws<ValidacaoDominioException>(() =>
            Linha.CriarSimulada(codigo, "SIM-101", "Centro", "Norte", SentidoLinha.PrincipalParaSecundario, Cenario.Agora));

        Assert.Equal("codigo", excecao.Campo);
    }

    [Theory]
    [InlineData("\nSIM-V12")]
    [InlineData("SIM-V12\n")]
    [InlineData("SIM-\nV12")]
    [InlineData("SIM-V 12")]
    [InlineData("SIM-V12!")]
    [InlineData("SIM-Ç12")]
    public void PrefixoDeVeiculo_ComQuebraDeLinhaOuCaractereForaDoFormato_ERecusado(string prefixo)
    {
        (Parada parada, Linha linha, _) = Cenario.ParadaComLinha();

        var noRegistro = Assert.Throws<ValidacaoDominioException>(() =>
            PassagemRegistroManual.Registrar(parada, linha, Cenario.Agora, prefixo, "operador-local", Cenario.Agora));
        var naPrevisao = Assert.Throws<ValidacaoDominioException>(() =>
            new PrevisaoChegada(new ReferenciaLinha(4, "SIM-L004", "SIM-104"), Cenario.Agora, prefixo));

        Assert.Equal("prefixoVeiculo", noRegistro.Campo);
        Assert.Equal("prefixoVeiculo", naPrevisao.Campo);
    }

    [Theory]
    [InlineData("\noperador-local")]
    [InlineData("operador-local\n")]
    [InlineData("operador-local\r\n")]
    [InlineData("operador\nlocal")]
    [InlineData("operador.local")]
    [InlineData("operador-lócal")]
    public void OrigemRegistro_ComQuebraDeLinhaOuCaractereForaDoFormato_ERecusada(string origem)
    {
        (Parada parada, Linha linha, _) = Cenario.ParadaComLinha();

        var excecao = Assert.Throws<ValidacaoDominioException>(() =>
            PassagemRegistroManual.Registrar(parada, linha, Cenario.Agora, null, origem, Cenario.Agora));

        Assert.Equal("origemRegistro", excecao.Campo);
    }

    [Theory]
    [InlineData("\nSIM-OBS-001")]
    [InlineData("SIM-OBS-001\n")]
    [InlineData("SIM-OBS\n-001")]
    [InlineData("SIM-OBS-001 ")]
    [InlineData("SIM-OBS_001")]
    public void ChaveSeed_ComQuebraDeLinhaOuCaractereForaDoFormato_ERecusada(string chave)
    {
        (_, _, ParadaLinha associacao) = Cenario.ParadaComLinha();

        var excecao = Assert.Throws<ValidacaoDominioException>(() =>
            PassagemSimulada.CriarNoProvisionamento(associacao, chave, TimeSpan.FromMinutes(15), Cenario.Agora, null));

        Assert.Equal("chaveSeed", excecao.Campo);
    }

    [Fact]
    public void Formatos_ContinuamAceitandoOsLimitesDoContrato()
    {
        (Parada parada, Linha linha, ParadaLinha associacao) = Cenario.ParadaComLinha();

        Assert.Equal("ABC", Parada.CadastrarAdministrativa("ABC", "Parada Válida", null, null, false, Cenario.Agora).CodigoExterno);
        Assert.Equal(new string('A', 30), Parada.CadastrarAdministrativa(new string('A', 30), "Parada Válida", null, null, false, Cenario.Agora).CodigoExterno);
        Assert.Equal("SIM-" + new string('V', 16), PassagemRegistroManual.Registrar(parada, linha, Cenario.Agora, "SIM-" + new string('V', 16), "abc", Cenario.Agora).PrefixoVeiculo);
        Assert.Equal(new string('a', 40), PassagemRegistroManual.Registrar(parada, linha, Cenario.Agora, null, new string('a', 40), Cenario.Agora).OrigemRegistro);
        Assert.Equal("SIM-" + new string('A', 36), PassagemSimulada.CriarNoProvisionamento(associacao, "SIM-" + new string('A', 36), TimeSpan.Zero, Cenario.Agora, null).ChaveSeed);
    }
}

using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Infrastructure.Persistence.Busca;
using PontoVoz.Tests.Domain;

namespace PontoVoz.Tests.Infrastructure;

public class TextoBuscaTests
{
    [Theory]
    [InlineData("demonstracao")]
    [InlineData("Demonstração")]
    [InlineData("DEMONSTRAÇÃO")]
    [InlineData("  Demonstração   ")]
    [InlineData("Demonstração")]
    [InlineData("DEMONSTRAÇÃO")]
    public void Normalizar_FormasDiferentesDoMesmoTexto_ResultamNoMesmoValor(string texto)
    {
        Assert.Equal("demonstracao", TextoBusca.Normalizar(texto));
    }

    [Theory]
    [InlineData("Terminal   Demonstração\tNorte", "terminal demonstracao norte")]
    [InlineData("  São  Paulo \n Centro ", "sao paulo centro")]
    [InlineData("Conceição, Ação e Ônibus", "conceicao, acao e onibus")]
    [InlineData("SIM-P001", "sim-p001")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData(null, "")]
    public void Normalizar_ReduzEspacosRemoveAcentosEUsaMinusculas(string? texto, string esperado)
    {
        Assert.Equal(esperado, TextoBusca.Normalizar(texto));
    }

    [Fact]
    public void Normalizar_FormasUnicodeComposta_EDecomposta_SaoEquivalentes()
    {
        string composta = "Estação".Normalize(System.Text.NormalizationForm.FormC);
        string decomposta = "Estação".Normalize(System.Text.NormalizationForm.FormD);

        Assert.NotEqual(composta, decomposta);
        Assert.Equal(TextoBusca.Normalizar(composta), TextoBusca.Normalizar(decomposta));
    }

    [Theory]
    [InlineData("norte", "%norte%")]
    [InlineData("50%", @"%50\%%")]
    [InlineData("sim_101", @"%sim\_101%")]
    [InlineData(@"a\b", @"%a\\b%")]
    public void PadraoContem_EscapaOsCuringasDoLike(string termo, string padrao)
    {
        Assert.Equal(padrao, TextoBusca.PadraoContem(termo));
    }

    [Fact]
    public void TextoDasEntidades_ReuneOsCamposPesquisaveisSemAlterarOsOriginais()
    {
        Parada parada = Cenario.ParadaSimulada("SIM-P001", "Parada Demonstração Centro");
        Linha linha = Linha.CriarSimulada("SIM-L004", "SIM-104", "Terminal Demonstração Centro", "Terminal Demonstração Norte", SentidoLinha.PrincipalParaSecundario, Cenario.Agora);

        Assert.Equal("parada demonstracao centro\nsim-p001", TextoBusca.DeParada(parada));
        Assert.Equal("sim-l004\nsim-104\nterminal demonstracao centro\nterminal demonstracao norte", TextoBusca.DeLinha(linha));
        Assert.Equal("Parada Demonstração Centro", parada.Nome);
    }
}

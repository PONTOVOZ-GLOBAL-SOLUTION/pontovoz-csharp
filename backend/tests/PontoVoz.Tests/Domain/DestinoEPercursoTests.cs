using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.Exceptions;

namespace PontoVoz.Tests.Domain;

public class DestinoEPercursoTests
{
    [Fact]
    public void Destino_CriarSimulado_EAtivoESimulado()
    {
        Destino destino = Destino.CriarSimulado("Terminal Demonstração Norte", TipoDestino.Terminal, Cenario.Agora);

        Assert.Equal("Terminal Demonstração Norte", destino.Nome);
        Assert.Equal(TipoDestino.Terminal, destino.Tipo);
        Assert.True(destino.Ativo);
        Assert.Equal(OrigemDado.Simulado, destino.Origem);
        Assert.True(destino.DadosSimulados);
    }

    [Theory]
    [InlineData("AB", TipoDestino.Estacao, "nome")]
    [InlineData("Estação Demonstração", (TipoDestino)0, "tipo")]
    [InlineData("Estação Demonstração", (TipoDestino)9, "tipo")]
    public void Destino_ComNomeOuTipoInvalido_ERecusado(string nome, TipoDestino tipo, string campo)
    {
        var excecao = Assert.Throws<ValidacaoDominioException>(() => Destino.CriarSimulado(nome, tipo, Cenario.Agora));

        Assert.Equal(campo, excecao.Campo);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(20)]
    [InlineData(240)]
    public void Percurso_Entre1E240Minutos_EAceito(int minutos)
    {
        (Parada parada, Linha linha, ParadaLinha associacao) = Cenario.ParadaComLinha();
        Destino destino = Cenario.DestinoSimulado();

        PercursoEstimado percurso = PercursoEstimado.CriarSimulado(associacao, destino, minutos);

        Assert.Equal(minutos, percurso.MinutosEstimados);
        Assert.Same(parada, percurso.Parada);
        Assert.Same(linha, percurso.Linha);
        Assert.Same(destino, percurso.Destino);
        Assert.Equal(OrigemDado.Simulado, percurso.Origem);
        Assert.True(percurso.DadosSimulados);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(241)]
    [InlineData(-5)]
    [InlineData(int.MaxValue)]
    public void Percurso_ForaDaFaixa_ERecusado(int minutos)
    {
        (_, _, ParadaLinha associacao) = Cenario.ParadaComLinha();

        var excecao = Assert.Throws<ValidacaoDominioException>(() => PercursoEstimado.CriarSimulado(associacao, Cenario.DestinoSimulado(), minutos));

        Assert.Equal("percursoEstimadoMinutos", excecao.Campo);
    }

    [Fact]
    public void Percurso_UsaAsChavesDaAssociacaoEDoDestinoPersistidos()
    {
        Parada parada = Cenario.ParadaSimulada().ComId(1);
        ParadaLinha associacao = parada.AssociarLinha(Cenario.LinhaSimulada().ComId(4), Cenario.Agora);

        PercursoEstimado percurso = PercursoEstimado.CriarSimulado(associacao, Cenario.DestinoSimulado().ComId(3), 20);

        Assert.Equal((1, 4, 3), (percurso.ParadaId, percurso.LinhaId, percurso.DestinoId));
    }

    [Fact]
    public void Percurso_SoNasceDeUmaAssociacao_LinhaQueNaoAtendeAParadaNaoTemPercurso()
    {
        (Parada parada, _, _) = Cenario.ParadaComLinha();
        Linha linhaDeOutraParada = Cenario.LinhaSimulada("SIM-L003", "SIM-203");

        var excecao = Assert.Throws<RegraNegocioException>(() => parada.ObterAssociacao(linhaDeOutraParada));

        Assert.Equal(RegraNegocioException.LinhaNaoAtendeParada, excecao.Codigo);
    }

    [Fact]
    public void Percurso_SemAssociacaoOuSemDestino_ERecusado()
    {
        (_, _, ParadaLinha associacao) = Cenario.ParadaComLinha();

        Assert.Throws<ArgumentNullException>(() => PercursoEstimado.CriarSimulado(null!, Cenario.DestinoSimulado(), 20));
        Assert.Throws<ArgumentNullException>(() => PercursoEstimado.CriarSimulado(associacao, null!, 20));
    }
}

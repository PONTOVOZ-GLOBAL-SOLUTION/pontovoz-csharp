using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.ValueObjects;

namespace PontoVoz.Tests.Domain;

/// <summary>Previsões anteriores ao instante da consulta não são devolvidas nem comparadas.</summary>
public class PrevisaoVencidaTests
{
    private static readonly ReferenciaLinha L002 = new(2, "SIM-L002", "SIM-102");
    private static readonly ReferenciaLinha L004 = new(4, "SIM-L004", "SIM-104");

    private static PrevisaoChegada Previsao(ReferenciaLinha linha, int segundosAposConsulta) =>
        new(linha, Cenario.Agora.AddSeconds(segundosAposConsulta), null);

    private static PercursoEstimado Percurso(ReferenciaLinha linha, int minutos)
    {
        Parada parada = Cenario.ParadaSimulada().ComId(1);
        Linha entidade = Cenario.LinhaSimulada(linha.CodigoLinha, linha.Letreiro).ComId(linha.LinhaId);
        return PercursoEstimado.CriarSimulado(parada.AssociarLinha(entidade, Cenario.Agora), Cenario.DestinoSimulado().ComId(3), minutos);
    }

    [Theory]
    [InlineData(-3600, true)]
    [InlineData(-1, true)]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(480, false)]
    public void VencidaEm_SoAntesDoInstanteDaConsulta(int segundosAposConsulta, bool vencida)
    {
        Assert.Equal(vencida, Previsao(L004, segundosAposConsulta).VencidaEm(Cenario.Agora));
    }

    [Fact]
    public void VencidaEm_UmTickAntes_JaEstaVencida()
    {
        var previsao = new PrevisaoChegada(L004, Cenario.Agora - TimeSpan.FromTicks(1), null);

        Assert.True(previsao.VencidaEm(Cenario.Agora));
    }

    [Fact]
    public void Respondido_ComSomentePrevisoesVencidas_FicaSemPrevisoes()
    {
        ResultadoProvedor resultado = ResultadoProvedor.Respondido(OrigemDado.Simulado, Cenario.Agora,
            [Previsao(L002, -60), Previsao(L004, -1)]);

        Assert.Equal(StatusPrevisao.SemPrevisoes, resultado.Status);
        Assert.Empty(resultado.Previsoes);
        Assert.True(resultado.ProvedorDisponivel);
        Assert.Null(resultado.ProximaChegada(2));
    }

    [Fact]
    public void Respondido_Misto_MantemSoAsPrevisoesDoInstanteEmDiante()
    {
        ResultadoProvedor resultado = ResultadoProvedor.Respondido(OrigemDado.Simulado, Cenario.Agora,
            [Previsao(L002, -120), Previsao(L002, 0), Previsao(L004, -30), Previsao(L004, 480)]);

        Assert.Equal(StatusPrevisao.Disponivel, resultado.Status);
        Assert.Equal(new[] { Cenario.Agora, Cenario.Agora.AddMinutes(8) }, resultado.Previsoes.Select(p => p.PrevistaParaUtc));
        Assert.All(resultado.Previsoes, previsao => Assert.False(previsao.VencidaEm(resultado.ConsultadoEmUtc)));
    }

    [Fact]
    public void ProximaChegada_IgnoraAPrevisaoVencidaMesmoSendoAMaisAntiga()
    {
        ResultadoProvedor resultado = ResultadoProvedor.Respondido(OrigemDado.Simulado, Cenario.Agora,
            [Previsao(L002, -300), Previsao(L002, 1140)]);

        Assert.Equal(Cenario.Agora.AddMinutes(19), resultado.ProximaChegada(2)!.PrevistaParaUtc);
    }

    [Fact]
    public void Respondido_ComPrevisaoExatamenteNoInstante_ManteveOOnibusChegando()
    {
        ResultadoProvedor resultado = ResultadoProvedor.Respondido(OrigemDado.Simulado, Cenario.Agora, [Previsao(L004, 0)]);

        PrevisaoChegada previsao = Assert.Single(resultado.Previsoes);
        Assert.Equal(0, previsao.EsperaMinutos(Cenario.Agora));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-60)]
    [InlineData(-3600)]
    public void Alternativa_ComPrevisaoVencida_NaoEComparavel(int segundosAposConsulta)
    {
        var alternativa = AlternativaDeslocamento.Calcular(Percurso(L004, 20), Previsao(L004, segundosAposConsulta), Cenario.Agora);

        Assert.Equal(SituacaoAlternativa.SemPrevisao, alternativa.Situacao);
        Assert.Null(alternativa.PrevistaParaUtc);
        Assert.Null(alternativa.EsperaMinutos);
        Assert.Null(alternativa.TempoTotalEstimadoMinutos);
        Assert.Null(alternativa.ChegadaEstimadaAoDestinoUtc);
        Assert.Equal(20, alternativa.PercursoEstimadoMinutos);
    }

    [Fact]
    public void Alternativa_ComPrevisaoNoInstanteDaConsulta_TemEsperaZeroETempoIgualAoPercurso()
    {
        var alternativa = AlternativaDeslocamento.Calcular(Percurso(L004, 20), Previsao(L004, 0), Cenario.Agora);

        Assert.Equal(SituacaoAlternativa.Comparavel, alternativa.Situacao);
        Assert.Equal(0, alternativa.EsperaMinutos);
        Assert.Equal(20, alternativa.TempoTotalEstimadoMinutos);
        Assert.Equal(Cenario.Agora.AddMinutes(20), alternativa.ChegadaEstimadaAoDestinoUtc);
    }

    [Theory]
    [InlineData(30, 20, 0, 20)]
    [InlineData(59, 20, 0, 20)]
    [InlineData(60, 20, 1, 21)]
    [InlineData(150, 35, 2, 37)]
    public void Alternativa_ComPrevisaoFutura_EsperaTotalEChegadaSaoCoerentes(int segundosAposConsulta, int percurso, int espera, int total)
    {
        PrevisaoChegada previsao = Previsao(L004, segundosAposConsulta);

        var alternativa = AlternativaDeslocamento.Calcular(Percurso(L004, percurso), previsao, Cenario.Agora);

        Assert.Equal(espera, alternativa.EsperaMinutos);
        Assert.Equal(total, alternativa.TempoTotalEstimadoMinutos);
        Assert.Equal(alternativa.EsperaMinutos + alternativa.PercursoEstimadoMinutos, alternativa.TempoTotalEstimadoMinutos);
        Assert.Equal(previsao.PrevistaParaUtc.AddMinutes(percurso), alternativa.ChegadaEstimadaAoDestinoUtc);
    }

    [Fact]
    public void Alternativa_ComReferenciaPosteriorAConsulta_DescartaAPrevisaoQueVenceuNesseIntervalo()
    {
        ResultadoProvedor resultado = ResultadoProvedor.Respondido(OrigemDado.Simulado, Cenario.Agora, [Previsao(L004, 30)]);
        PrevisaoChegada previsao = resultado.ProximaChegada(4)!;

        var alternativa = AlternativaDeslocamento.Calcular(Percurso(L004, 20), previsao, Cenario.Agora.AddMinutes(1));

        Assert.Equal(SituacaoAlternativa.SemPrevisao, alternativa.Situacao);
    }
}

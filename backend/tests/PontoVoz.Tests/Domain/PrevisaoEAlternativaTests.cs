using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.Exceptions;
using PontoVoz.Api.Domain.ValueObjects;

namespace PontoVoz.Tests.Domain;

/// <summary>Usa o cenário de contracts/exemplos: consulta às 14h32 na parada SIM-P001.</summary>
public class PrevisaoEAlternativaTests
{
    private static readonly ReferenciaLinha L001 = new(1, "SIM-L001", "SIM-101");
    private static readonly ReferenciaLinha L002 = new(2, "SIM-L002", "SIM-102");
    private static readonly ReferenciaLinha L004 = new(4, "SIM-L004", "SIM-104");

    private static PrevisaoChegada Previsao(ReferenciaLinha linha, int minutos) =>
        new(linha, Cenario.Agora.AddMinutes(minutos), null);

    private static PercursoEstimado Percurso(ReferenciaLinha linha, int minutos)
    {
        Parada parada = Cenario.ParadaSimulada().ComId(1);
        Linha entidade = Cenario.LinhaSimulada(linha.CodigoLinha, linha.Letreiro).ComId(linha.LinhaId);
        return PercursoEstimado.CriarSimulado(parada.AssociarLinha(entidade, Cenario.Agora), Cenario.DestinoSimulado().ComId(3), minutos);
    }

    [Fact]
    public void Previsao_NuncaEPassagemConfirmada()
    {
        Assert.Equal(NivelConfirmacao.Previsao, Previsao(L002, 1).NivelConfirmacao);
    }

    [Theory]
    [InlineData(8 * 60, 8)]
    [InlineData(59, 0)]
    [InlineData(60, 1)]
    [InlineData(119, 1)]
    [InlineData(0, 0)]
    [InlineData(-30, 0)]
    public void Previsao_EsperaEmMinutosInteirosParaBaixoENuncaNegativa(int segundosAteChegada, int espera)
    {
        var previsao = new PrevisaoChegada(L004, Cenario.Agora.AddSeconds(segundosAteChegada), "SIM-V4001");

        Assert.Equal(espera, previsao.EsperaMinutos(Cenario.Agora));
    }

    [Fact]
    public void Previsao_GuardaOHorarioEmUtc_EValidaOPrefixoDoVeiculo()
    {
        PrevisaoChegada previsao = Previsao(L002, 1);

        Assert.Equal(TimeSpan.Zero, previsao.PrevistaParaUtc.Offset);
        Assert.Throws<ValidacaoDominioException>(() => new PrevisaoChegada(L002, Cenario.Agora, "veículo 1"));
    }

    [Fact]
    public void ReferenciaLinha_ExigeLinhaPersistida()
    {
        Assert.Throws<ValidacaoDominioException>(() => new ReferenciaLinha(0, "SIM-L001", "SIM-101"));
        Assert.Throws<ValidacaoDominioException>(() => ReferenciaLinha.De(Cenario.LinhaSimulada()));
    }

    [Fact]
    public void Resultado_ComPrevisoes_EDisponivel_EAchaAProximaChegadaDeCadaLinha()
    {
        ResultadoProvedor resultado = ResultadoProvedor.Respondido(OrigemDado.Simulado, Cenario.Agora,
            [Previsao(L002, 19), Previsao(L004, 8), Previsao(L002, 1)]);

        Assert.Equal(StatusPrevisao.Disponivel, resultado.Status);
        Assert.True(resultado.ProvedorDisponivel);
        Assert.Equal(Cenario.Agora.AddMinutes(1), resultado.ProximaChegada(2)!.PrevistaParaUtc);
        Assert.Equal(Cenario.Agora.AddMinutes(8), resultado.ProximaChegada(4)!.PrevistaParaUtc);
        Assert.Null(resultado.ProximaChegada(1));
    }

    [Fact]
    public void Resultado_SemPrevisoes_ESemPrevisoes()
    {
        ResultadoProvedor resultado = ResultadoProvedor.Respondido(OrigemDado.Simulado, Cenario.Agora, []);

        Assert.Equal(StatusPrevisao.SemPrevisoes, resultado.Status);
        Assert.True(resultado.ProvedorDisponivel);
        Assert.Empty(resultado.Previsoes);
    }

    [Fact]
    public void Resultado_Indisponivel_NaoTemPrevisoes()
    {
        ResultadoProvedor resultado = ResultadoProvedor.Indisponivel(OrigemDado.OlhoVivo, Cenario.Agora);

        Assert.Equal(StatusPrevisao.ProvedorIndisponivel, resultado.Status);
        Assert.False(resultado.ProvedorDisponivel);
        Assert.Empty(resultado.Previsoes);
        Assert.Equal(OrigemDado.OlhoVivo, resultado.Origem);
        Assert.Equal(TimeSpan.Zero, resultado.ConsultadoEmUtc.Offset);
    }

    [Fact]
    public void Resultado_ComOrigemDeCadastro_ERecusado()
    {
        Assert.Throws<ValidacaoDominioException>(() => ResultadoProvedor.Indisponivel(OrigemDado.CadastroAdministrativo, Cenario.Agora));
    }

    [Theory]
    [InlineData(4, 8, 20, 28)]
    [InlineData(2, 1, 40, 41)]
    [InlineData(1, 0, 1, 1)]
    [InlineData(1, 30, 240, 270)]
    public void Alternativa_ComPrevisao_SomaEsperaEPercurso(int linhaId, int espera, int percurso, int total)
    {
        ReferenciaLinha linha = new[] { L001, L002, L004 }.Single(l => l.LinhaId == linhaId);

        var alternativa = AlternativaDeslocamento.Calcular(Percurso(linha, percurso), Previsao(linha, espera), Cenario.Agora);

        Assert.Equal(SituacaoAlternativa.Comparavel, alternativa.Situacao);
        Assert.Equal(espera, alternativa.EsperaMinutos);
        Assert.Equal(total, alternativa.TempoTotalEstimadoMinutos);
        Assert.Equal(Cenario.Agora.AddMinutes(espera + percurso), alternativa.ChegadaEstimadaAoDestinoUtc);
        Assert.Equal(0, alternativa.Baldeacoes);
        Assert.Equal(OrigemDado.Simulado, alternativa.OrigemPercurso);
    }

    [Fact]
    public void Alternativa_QueChegaPrimeiroPodeTerTempoTotalMaior()
    {
        var sim104 = AlternativaDeslocamento.Calcular(Percurso(L004, 20), Previsao(L004, 8), Cenario.Agora);
        var sim102 = AlternativaDeslocamento.Calcular(Percurso(L002, 40), Previsao(L002, 1), Cenario.Agora);

        Assert.True(sim102.EsperaMinutos < sim104.EsperaMinutos);
        Assert.True(sim104.TempoTotalEstimadoMinutos < sim102.TempoTotalEstimadoMinutos);
    }

    [Fact]
    public void Alternativa_SemPrevisao_MantemOPercursoETemTemposNulos()
    {
        var alternativa = AlternativaDeslocamento.Calcular(Percurso(L001, 30), null, Cenario.Agora);

        Assert.Equal(SituacaoAlternativa.SemPrevisao, alternativa.Situacao);
        Assert.Equal(30, alternativa.PercursoEstimadoMinutos);
        Assert.Null(alternativa.PrevistaParaUtc);
        Assert.Null(alternativa.EsperaMinutos);
        Assert.Null(alternativa.TempoTotalEstimadoMinutos);
        Assert.Null(alternativa.ChegadaEstimadaAoDestinoUtc);
    }

    [Fact]
    public void Alternativa_ComPrevisaoDeOutraLinha_ERecusada()
    {
        Assert.Throws<ValidacaoDominioException>(() => AlternativaDeslocamento.Calcular(Percurso(L004, 20), Previsao(L002, 1), Cenario.Agora));
    }
}

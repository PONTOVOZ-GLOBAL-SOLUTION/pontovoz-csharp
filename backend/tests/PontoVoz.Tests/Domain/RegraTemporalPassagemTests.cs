using PontoVoz.Api.Domain.Rules;

namespace PontoVoz.Tests.Domain;

/// <summary>Casos da seção 4.6.3 do contrato, mais valores intermediários e extremos.</summary>
public class RegraTemporalPassagemTests
{
    private static readonly DateTimeOffset Referencia = Cenario.Agora;

    [Theory]
    [InlineData("J1", -121, false)]
    [InlineData("J2", -120, true)]
    [InlineData("J3", 0, true)]
    [InlineData("J4", 86400, true)]
    [InlineData("J5", 86401, false)]
    public void JanelaDeCriacao_CasosDoContrato(string caso, int intervaloSegundos, bool aceita)
    {
        DateTimeOffset observadaEm = Referencia.AddSeconds(-intervaloSegundos);

        Assert.True(aceita == RegraTemporalPassagem.ObservacaoAceitaNoRegistro(observadaEm, Referencia), caso);
    }

    [Theory]
    [InlineData(-119, true)]
    [InlineData(-30, true)]
    [InlineData(-180, false)]
    [InlineData(3600, true)]
    [InlineData(86399, true)]
    [InlineData(90000, false)]
    [InlineData(7 * 86400, false)]
    public void JanelaDeCriacao_OutrosValores(int intervaloSegundos, bool aceita)
    {
        DateTimeOffset observadaEm = Referencia.AddSeconds(-intervaloSegundos);

        Assert.Equal(aceita, RegraTemporalPassagem.ObservacaoAceitaNoRegistro(observadaEm, Referencia));
    }

    [Fact]
    public void JanelaDeCriacao_UmTickAlemDosLimites_ERecusada()
    {
        DateTimeOffset alemDoFuturo = Referencia + RegraTemporalPassagem.ToleranciaFuturo + TimeSpan.FromTicks(1);
        DateTimeOffset alemDoPassado = Referencia - RegraTemporalPassagem.JanelaPassado - TimeSpan.FromTicks(1);

        Assert.False(RegraTemporalPassagem.ObservacaoAceitaNoRegistro(alemDoFuturo, Referencia));
        Assert.False(RegraTemporalPassagem.ObservacaoAceitaNoRegistro(alemDoPassado, Referencia));
    }

    [Theory]
    [InlineData("T1", -120, 0, false, false)]
    [InlineData("T2", -1, 0, false, false)]
    [InlineData("T3", 0, 0, false, true)]
    [InlineData("T4", 59, 0, false, true)]
    [InlineData("T5", 3600, 60, false, true)]
    [InlineData("T6", 3601, 60, true, true)]
    [InlineData("T7", 3660, 61, true, true)]
    public void Calculo_CasosDoContrato(string caso, int intervaloSegundos, int minutos, bool desatualizada, bool jaOcorreu)
    {
        DateTimeOffset observadaEm = Referencia.AddSeconds(-intervaloSegundos);

        Assert.True(minutos == RegraTemporalPassagem.MinutosDesdeObservacao(observadaEm, Referencia), $"{caso}: minutos");
        Assert.True(desatualizada == RegraTemporalPassagem.EstaDesatualizada(observadaEm, Referencia), $"{caso}: desatualizada");
        Assert.True(jaOcorreu == RegraTemporalPassagem.JaOcorreu(observadaEm, Referencia), $"{caso}: aparece na consulta");
    }

    [Theory]
    [InlineData(-86400, 0, false)]
    [InlineData(1, 0, false)]
    [InlineData(60, 1, false)]
    [InlineData(119, 1, false)]
    [InlineData(1799, 29, false)]
    [InlineData(3599, 59, false)]
    [InlineData(3661, 61, true)]
    [InlineData(5220, 87, true)]
    [InlineData(193620, 3227, true)]
    public void Calculo_OutrosValores(int intervaloSegundos, int minutos, bool desatualizada)
    {
        DateTimeOffset observadaEm = Referencia.AddSeconds(-intervaloSegundos);

        Assert.Equal(minutos, RegraTemporalPassagem.MinutosDesdeObservacao(observadaEm, Referencia));
        Assert.Equal(desatualizada, RegraTemporalPassagem.EstaDesatualizada(observadaEm, Referencia));
    }

    [Fact]
    public void Desatualizacao_UsaOIntervaloExato_UmTickAcimaDe60MinutosJaEDesatualizada()
    {
        DateTimeOffset observadaEm = Referencia - RegraTemporalPassagem.LimiteDesatualizacao - TimeSpan.FromTicks(1);

        Assert.Equal(60, RegraTemporalPassagem.MinutosDesdeObservacao(observadaEm, Referencia));
        Assert.True(RegraTemporalPassagem.EstaDesatualizada(observadaEm, Referencia));
    }

    [Fact]
    public void Calculo_NaoDependeDoFusoDosInstantes()
    {
        DateTimeOffset observadaEmSaoPaulo = new(2026, 11, 10, 13, 5, 0, TimeSpan.FromHours(-3));
        DateTimeOffset referenciaUtc = Referencia.ToUniversalTime();

        Assert.Equal(87, RegraTemporalPassagem.MinutosDesdeObservacao(observadaEmSaoPaulo, referenciaUtc));
        Assert.Equal(87, RegraTemporalPassagem.MinutosDesdeObservacao(observadaEmSaoPaulo.ToUniversalTime(), Referencia));
    }

    [Fact]
    public void Constantes_BatemComOContrato()
    {
        Assert.Equal(TimeSpan.FromMinutes(2), RegraTemporalPassagem.ToleranciaFuturo);
        Assert.Equal(TimeSpan.FromHours(24), RegraTemporalPassagem.JanelaPassado);
        Assert.Equal(RegraTemporalPassagem.LimiteDesatualizacaoMinutos, (int)RegraTemporalPassagem.LimiteDesatualizacao.TotalMinutes);
    }
}

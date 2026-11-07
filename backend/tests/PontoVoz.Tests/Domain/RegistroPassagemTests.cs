using Microsoft.Extensions.Time.Testing;

using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.Exceptions;

namespace PontoVoz.Tests.Domain;

public class RegistroPassagemTests
{
    private readonly FakeTimeProvider _relogio = new(Cenario.Agora);

    private static PassagemSimulada Simulada(ParadaLinha associacao, DateTimeOffset provisionamento, int minutosAntes = 15) =>
        PassagemSimulada.CriarNoProvisionamento(associacao, "SIM-OBS-001", TimeSpan.FromMinutes(minutosAntes), provisionamento, "SIM-V2003");

    [Fact]
    public void Hierarquia_CadaSubclasseDefineNivelAvisoEOrigem_ENenhumaEVerificada()
    {
        (Parada parada, Linha linha, ParadaLinha associacao) = Cenario.ParadaComLinha();
        DateTimeOffset agora = _relogio.GetUtcNow();
        RegistroPassagem[] registros =
        [
            Simulada(associacao, agora),
            PassagemRegistroManual.Registrar(parada, linha, agora.AddMinutes(-8), null, "operador-local", agora),
        ];

        Assert.Collection(
            registros,
            simulada =>
            {
                Assert.Equal(NivelConfirmacao.ObservacaoSimulada, simulada.NivelConfirmacao);
                Assert.Equal("ObservacaoSimulada", simulada.CodigoAviso);
                Assert.Null(simulada.OrigemRegistro);
            },
            manual =>
            {
                Assert.Equal(NivelConfirmacao.RegistroManual, manual.NivelConfirmacao);
                Assert.Equal("RegistroManualNaoVerificado", manual.CodigoAviso);
                Assert.Equal("operador-local", manual.OrigemRegistro);
            });

        Assert.All(registros, registro =>
        {
            Assert.False(registro.Verificada);
            Assert.False(registro.OficialSptrans);
            Assert.NotEqual(NivelConfirmacao.VerificadaFonteConfiavel, registro.NivelConfirmacao);
            Assert.NotEqual(NivelConfirmacao.Previsao, registro.NivelConfirmacao);
        });
    }

    [Fact]
    public void Hierarquia_CalculoTemporalEComumATodasAsSubclasses()
    {
        (Parada parada, Linha linha, ParadaLinha associacao) = Cenario.ParadaComLinha();
        DateTimeOffset agora = _relogio.GetUtcNow();
        RegistroPassagem[] registros =
        [
            Simulada(associacao, agora, minutosAntes: 20),
            PassagemRegistroManual.Registrar(parada, linha, agora.AddMinutes(-20), null, "operador-local", agora),
        ];

        _relogio.Advance(TimeSpan.FromMinutes(41));

        Assert.All(registros, registro =>
        {
            Assert.Equal(61, registro.MinutosDesdeObservacao(_relogio.GetUtcNow()));
            Assert.True(registro.EstaDesatualizada(_relogio.GetUtcNow()));
        });
    }

    [Fact]
    public void Simulada_NoProvisionamento_DefineHorariosRelativosAoInstanteDoProvisionamento()
    {
        (_, _, ParadaLinha associacao) = Cenario.ParadaComLinha();
        DateTimeOffset provisionamento = _relogio.GetUtcNow();

        PassagemSimulada passagem = Simulada(associacao, provisionamento);

        Assert.Equal(provisionamento.AddMinutes(-15), passagem.ObservadaEmUtc);
        Assert.Equal(provisionamento, passagem.RegistradaEmUtc);
        Assert.Equal(provisionamento, passagem.CriadoEmUtc);
        Assert.Equal(TimeSpan.Zero, passagem.ObservadaEmUtc.Offset);
        Assert.Equal("SIM-OBS-001", passagem.ChaveSeed);
        Assert.True(passagem.DadosSimulados);
    }

    [Fact]
    public void Simulada_ComOPassarDoTempo_MantemOHorarioOriginalEFicaDesatualizada()
    {
        (_, _, ParadaLinha associacao) = Cenario.ParadaComLinha();
        PassagemSimulada passagem = Simulada(associacao, _relogio.GetUtcNow());
        DateTimeOffset observadaOriginal = passagem.ObservadaEmUtc;
        Assert.False(passagem.EstaDesatualizada(_relogio.GetUtcNow()));

        _relogio.Advance(TimeSpan.FromDays(2));

        Assert.Equal(observadaOriginal, passagem.ObservadaEmUtc);
        Assert.True(passagem.EstaDesatualizada(_relogio.GetUtcNow()));
        Assert.Equal((2 * 24 * 60) + 15, passagem.MinutosDesdeObservacao(_relogio.GetUtcNow()));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("OBS-001")]
    [InlineData("SIM-")]
    [InlineData("sim-obs-001")]
    [InlineData("SIM-OBS 001")]
    public void Simulada_ComChaveSeedInvalida_ERecusada(string? chave)
    {
        (_, _, ParadaLinha associacao) = Cenario.ParadaComLinha();

        var excecao = Assert.Throws<ValidacaoDominioException>(() =>
            PassagemSimulada.CriarNoProvisionamento(associacao, chave!, TimeSpan.FromMinutes(15), Cenario.Agora, null));

        Assert.Equal("chaveSeed", excecao.Campo);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(24 * 60 + 1)]
    public void Simulada_ComAntecedenciaNegativaOuMaiorQue24Horas_ERecusada(int minutos)
    {
        (_, _, ParadaLinha associacao) = Cenario.ParadaComLinha();

        Assert.Throws<ValidacaoDominioException>(() =>
            PassagemSimulada.CriarNoProvisionamento(associacao, "SIM-OBS-001", TimeSpan.FromMinutes(minutos), Cenario.Agora, null));
    }

    [Fact]
    public void Simulada_ComVeiculoSemPrefixoSim_ERecusada()
    {
        (_, _, ParadaLinha associacao) = Cenario.ParadaComLinha();

        var excecao = Assert.Throws<ValidacaoDominioException>(() =>
            PassagemSimulada.CriarNoProvisionamento(associacao, "SIM-OBS-001", TimeSpan.FromMinutes(15), Cenario.Agora, "V2003"));

        Assert.Equal("prefixoVeiculo", excecao.Campo);
    }

    [Theory]
    [InlineData("operador-local")]
    [InlineData("abc")]
    [InlineData("totem-01")]
    [InlineData("a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0")]
    public void Manual_ComOrigemRegistroValida_EAceita(string origem)
    {
        (Parada parada, Linha linha, _) = Cenario.ParadaComLinha();

        PassagemRegistroManual passagem = PassagemRegistroManual.Registrar(parada, linha, Cenario.Agora, null, origem, Cenario.Agora);

        Assert.Equal(origem, passagem.OrigemRegistro);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("ab")]
    [InlineData("Operador")]
    [InlineData("operador local")]
    [InlineData("operador_local")]
    [InlineData("pessoa@exemplo.com")]
    [InlineData("11999990000+")]
    [InlineData("a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0x")]
    public void Manual_ComOrigemRegistroForaDoFormatoTecnico_ERecusada(string? origem)
    {
        (Parada parada, Linha linha, _) = Cenario.ParadaComLinha();

        var excecao = Assert.Throws<ValidacaoDominioException>(() =>
            PassagemRegistroManual.Registrar(parada, linha, Cenario.Agora, null, origem!, Cenario.Agora));

        Assert.Equal("origemRegistro", excecao.Campo);
    }

    [Theory]
    [InlineData("J1", -121, false)]
    [InlineData("J2", -120, true)]
    [InlineData("J3", 0, true)]
    [InlineData("J4", 86400, true)]
    [InlineData("J5", 86401, false)]
    public void Manual_JanelaDeCriacao_CasosDoContrato(string caso, int intervaloSegundos, bool aceita)
    {
        (Parada parada, Linha linha, _) = Cenario.ParadaComLinha();
        DateTimeOffset agora = _relogio.GetUtcNow();
        DateTimeOffset observadaEm = agora.AddSeconds(-intervaloSegundos);

        Exception? erro = Record.Exception(() => PassagemRegistroManual.Registrar(parada, linha, observadaEm, null, "operador-local", agora));

        if (aceita)
        {
            Assert.True(erro is null, $"{caso}: deveria ser aceito");
        }
        else
        {
            var validacao = Assert.IsType<ValidacaoDominioException>(erro);
            Assert.Equal("observadaEm", validacao.Campo);
        }
    }

    [Fact]
    public void Manual_SeteDiasAtras_ERecusado()
    {
        (Parada parada, Linha linha, _) = Cenario.ParadaComLinha();
        DateTimeOffset agora = _relogio.GetUtcNow();

        Assert.Throws<ValidacaoDominioException>(() =>
            PassagemRegistroManual.Registrar(parada, linha, agora.AddDays(-7), null, "operador-local", agora));
    }

    [Fact]
    public void Manual_GuardaInstantesEmUtc_ERegistradaEmEOInstanteDoRegistro()
    {
        (Parada parada, Linha linha, _) = Cenario.ParadaComLinha();
        DateTimeOffset agora = _relogio.GetUtcNow();
        DateTimeOffset observadaSaoPaulo = new(2026, 11, 10, 14, 24, 0, TimeSpan.FromHours(-3));

        PassagemRegistroManual passagem = PassagemRegistroManual.Registrar(parada, linha, observadaSaoPaulo, "SIM-V12", "operador-local", agora);

        Assert.Equal(TimeSpan.Zero, passagem.ObservadaEmUtc.Offset);
        Assert.Equal(observadaSaoPaulo, passagem.ObservadaEmUtc);
        Assert.Equal(agora, passagem.RegistradaEmUtc);
        Assert.Equal(passagem.RegistradaEmUtc, passagem.CriadoEmUtc);
        Assert.Equal("SIM-V12", passagem.PrefixoVeiculo);
        Assert.Equal(8, passagem.MinutosDesdeObservacao(agora));
    }

    [Fact]
    public void Manual_ComHorarioAindaNoFuturo_NaoContaComoPassagemOcorridaAteOHorarioChegar()
    {
        (Parada parada, Linha linha, _) = Cenario.ParadaComLinha();
        DateTimeOffset agora = _relogio.GetUtcNow();

        PassagemRegistroManual passagem = PassagemRegistroManual.Registrar(parada, linha, agora.AddSeconds(90), null, "operador-local", agora);

        Assert.False(passagem.JaOcorreu(agora));
        Assert.Equal(0, passagem.MinutosDesdeObservacao(agora));
        Assert.False(passagem.EstaDesatualizada(agora));

        _relogio.Advance(TimeSpan.FromSeconds(89));
        Assert.False(passagem.JaOcorreu(_relogio.GetUtcNow()));

        _relogio.Advance(TimeSpan.FromSeconds(1));
        Assert.True(passagem.JaOcorreu(_relogio.GetUtcNow()));
        Assert.Equal(0, passagem.MinutosDesdeObservacao(_relogio.GetUtcNow()));
    }

    [Fact]
    public void Manual_EmParadaInativa_LancaParadaInativa()
    {
        (Parada parada, Linha linha, _) = Cenario.ParadaComLinha();
        parada.Desativar();

        var excecao = Assert.Throws<RegraNegocioException>(() =>
            PassagemRegistroManual.Registrar(parada, linha, Cenario.Agora, null, "operador-local", Cenario.Agora));

        Assert.Equal(RegraNegocioException.ParadaInativa, excecao.Codigo);
    }

    [Fact]
    public void Manual_ComLinhaInativa_LancaLinhaInativa()
    {
        (Parada parada, Linha linha, _) = Cenario.ParadaComLinha();
        linha.Desativar();

        var excecao = Assert.Throws<RegraNegocioException>(() =>
            PassagemRegistroManual.Registrar(parada, linha, Cenario.Agora, null, "operador-local", Cenario.Agora));

        Assert.Equal(RegraNegocioException.LinhaInativa, excecao.Codigo);
    }

    [Fact]
    public void Manual_ComLinhaQueNaoAtendeAParada_LancaLinhaNaoAtendeParada()
    {
        Parada parada = Cenario.ParadaSimulada();

        var excecao = Assert.Throws<RegraNegocioException>(() =>
            PassagemRegistroManual.Registrar(parada, Cenario.LinhaSimulada(), Cenario.Agora, null, "operador-local", Cenario.Agora));

        Assert.Equal(RegraNegocioException.LinhaNaoAtendeParada, excecao.Codigo);
    }

    [Fact]
    public void Manual_ValidaOsDadosAntesDoEstado_ComoNaOrdem400Antes422()
    {
        (Parada parada, Linha linha, _) = Cenario.ParadaComLinha();
        parada.Desativar();

        Assert.Throws<ValidacaoDominioException>(() =>
            PassagemRegistroManual.Registrar(parada, linha, Cenario.Agora.AddDays(-2), null, "operador-local", Cenario.Agora));
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(false, false, false)]
    public void Manual_DadosSimulados_VemDaParadaOuDaLinha(bool paradaFicticia, bool linhaFicticia, bool esperado)
    {
        Parada parada = Parada.CadastrarAdministrativa("PARADA-0001", "Parada Cadastrada", null, null, paradaFicticia, Cenario.Agora);
        Linha linha = Linha.CadastrarAdministrativa("LINHA-0001", "8000-10", "Centro", "Sul", SentidoLinha.PrincipalParaSecundario, linhaFicticia, Cenario.Agora);
        parada.AssociarLinha(linha, Cenario.Agora);

        PassagemRegistroManual passagem = PassagemRegistroManual.Registrar(parada, linha, Cenario.Agora, null, "operador-local", Cenario.Agora);

        Assert.Equal(esperado, passagem.DadosSimulados);
        Assert.False(passagem.Verificada);
    }
}

using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.Exceptions;

namespace PontoVoz.Tests.Domain;

/// <summary>
/// O prefixo do veículo de uma passagem segue a natureza do registro (fictício ou real),
/// não a origem do cadastro.
/// </summary>
public class PrefixoVeiculoTests
{
    private static (Parada Parada, Linha Linha) Cadastro(bool paradaFicticia, bool linhaFicticia)
    {
        Parada parada = Parada.CadastrarAdministrativa("PARADA-0001", "Parada Cadastrada", null, null, paradaFicticia, Cenario.Agora);
        Linha linha = Linha.CadastrarAdministrativa("LINHA-0001", "8000-10", "Centro", "Sul", SentidoLinha.PrincipalParaSecundario, linhaFicticia, Cenario.Agora);
        parada.AssociarLinha(linha, Cenario.Agora);
        return (parada, linha);
    }

    private static PassagemRegistroManual Registrar(Parada parada, Linha linha, string? prefixo) =>
        PassagemRegistroManual.Registrar(parada, linha, Cenario.Agora, prefixo, "operador-local", Cenario.Agora);

    [Theory]
    [InlineData("SIM-V12")]
    [InlineData("SIM-4001")]
    [InlineData("SIM-veiculo-teste-1")]
    public void Manual_EmParadaELinhaDoSeed_AceitaVeiculoComPrefixoSim(string prefixo)
    {
        (Parada parada, Linha linha, _) = Cenario.ParadaComLinha();

        Assert.Equal(prefixo, Registrar(parada, linha, prefixo).PrefixoVeiculo);
    }

    [Theory]
    [InlineData("V-12")]
    [InlineData("4001")]
    [InlineData("sim-V12")]
    [InlineData("SIM")]
    [InlineData("SIM-")]
    [InlineData("SIMV12")]
    public void Manual_EmParadaELinhaDoSeed_RecusaVeiculoSemPrefixoSim(string prefixo)
    {
        (Parada parada, Linha linha, _) = Cenario.ParadaComLinha();

        var excecao = Assert.Throws<ValidacaoDominioException>(() => Registrar(parada, linha, prefixo));

        Assert.Equal("prefixoVeiculo", excecao.Campo);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Manual_ComCadastroAdministrativoFicticio_ExigeVeiculoSim(bool paradaFicticia, bool linhaFicticia)
    {
        (Parada parada, Linha linha) = Cadastro(paradaFicticia, linhaFicticia);

        Assert.Equal("SIM-V1", Registrar(parada, linha, "SIM-V1").PrefixoVeiculo);
        Assert.Throws<ValidacaoDominioException>(() => Registrar(parada, linha, "V1"));
    }

    [Theory]
    [InlineData("V-12")]
    [InlineData("4001")]
    [InlineData("A")]
    [InlineData("AB12CD34EF56GH78IJ90")]
    public void Manual_ComCadastroAdministrativoReal_NaoExigePrefixoSim(string prefixo)
    {
        (Parada parada, Linha linha) = Cadastro(paradaFicticia: false, linhaFicticia: false);

        PassagemRegistroManual passagem = Registrar(parada, linha, prefixo);

        Assert.Equal(prefixo, passagem.PrefixoVeiculo);
        Assert.False(passagem.DadosSimulados);
    }

    [Theory]
    [InlineData("SIM-V12")]
    [InlineData("sim-v12")]
    public void Manual_ComCadastroAdministrativoReal_RecusaPrefixoReservadoSim(string prefixo)
    {
        (Parada parada, Linha linha) = Cadastro(paradaFicticia: false, linhaFicticia: false);

        var excecao = Assert.Throws<ValidacaoDominioException>(() => Registrar(parada, linha, prefixo));

        Assert.Equal("prefixoVeiculo", excecao.Campo);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Manual_SemPrefixoDeVeiculo_EAceitoEmQualquerNatureza(bool ficticio)
    {
        (Parada parada, Linha linha) = Cadastro(ficticio, ficticio);

        Assert.Null(Registrar(parada, linha, null).PrefixoVeiculo);
    }

    [Theory]
    [InlineData("SIM-V2003", true)]
    [InlineData(null, true)]
    [InlineData("V2003", false)]
    [InlineData("sim-v2003", false)]
    [InlineData("SIM-", false)]
    public void Simulada_UsaAMesmaRegraDeVeiculoFicticio(string? prefixo, bool aceito)
    {
        (_, _, ParadaLinha associacao) = Cenario.ParadaComLinha();

        Exception? erro = Record.Exception(() =>
            PassagemSimulada.CriarNoProvisionamento(associacao, "SIM-OBS-001", TimeSpan.FromMinutes(15), Cenario.Agora, prefixo));

        Assert.Equal(aceito, erro is null);
    }
}

using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.Exceptions;
using PontoVoz.Api.Domain.ValueObjects;

namespace PontoVoz.Tests.Domain;

public class ParadaTests
{
    [Fact]
    public void CriarSimulada_ComDadosValidos_CriaParadaAtivaSimuladaAindaNaoPersistida()
    {
        Parada parada = Parada.CriarSimulada("SIM-P001", "  Parada Demonstração Centro  ", Cenario.Agora);

        Assert.Equal("SIM-P001", parada.CodigoExterno);
        Assert.Equal("Parada Demonstração Centro", parada.Nome);
        Assert.Null(parada.Endereco);
        Assert.Null(parada.Coordenada);
        Assert.True(parada.Ativa);
        Assert.Equal(OrigemDado.Simulado, parada.Origem);
        Assert.True(parada.DadosSimulados);
        Assert.Equal(0, parada.Id);
        Assert.False(parada.Persistida);
        Assert.Empty(parada.LinhasAssociadas);
    }

    [Fact]
    public void CriadoEmUtc_RecebidoEmHorarioDeSaoPaulo_EGuardadoEmUtcNoMesmoInstante()
    {
        Parada parada = Cenario.ParadaSimulada();

        Assert.Equal(TimeSpan.Zero, parada.CriadoEmUtc.Offset);
        Assert.Equal(Cenario.Agora, parada.CriadoEmUtc);
        Assert.Equal(17, parada.CriadoEmUtc.Hour);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("SI")]
    [InlineData("sim-p001")]
    [InlineData("SIM P001")]
    [InlineData("SIM_P001")]
    [InlineData("SIM-PÇ01")]
    [InlineData("SIM-P0000000000000000000000000001")]
    public void CriarSimulada_ComCodigoInvalido_LancaValidacaoNoCampoCodigoExterno(string? codigo)
    {
        var excecao = Assert.Throws<ValidacaoDominioException>(() => Parada.CriarSimulada(codigo!, "Parada Válida", Cenario.Agora));

        Assert.Equal("codigoExterno", excecao.Campo);
        Assert.Equal("RequisicaoInvalida", excecao.Codigo);
    }

    [Theory]
    [InlineData("ABC")]
    [InlineData("PARADA-00000000000000000000001")]
    public void CadastrarAdministrativa_ComCodigoNosLimitesDe3E30Caracteres_EAceita(string codigo)
    {
        Parada parada = Parada.CadastrarAdministrativa(codigo, "Parada Válida", null, null, false, Cenario.Agora);

        Assert.Equal(codigo, parada.CodigoExterno);
    }

    [Fact]
    public void CadastrarAdministrativa_ComCodigoDe31Caracteres_ERecusada()
    {
        Assert.Throws<ValidacaoDominioException>(() =>
            Parada.CadastrarAdministrativa("PARADA-000000000000000000000001", "Parada Válida", null, null, false, Cenario.Agora));
    }

    [Fact]
    public void CriarSimulada_SemPrefixoSim_ERecusada()
    {
        var excecao = Assert.Throws<ValidacaoDominioException>(() => Parada.CriarSimulada("TESTE-P001", "Parada Válida", Cenario.Agora));

        Assert.Equal("codigoExterno", excecao.Campo);
    }

    [Theory]
    [InlineData("SIM-P003")]
    [InlineData("SIM-")]
    public void CadastrarAdministrativa_ComPrefixoReservadoSim_ERecusada(string codigo)
    {
        Assert.Throws<ValidacaoDominioException>(() => Parada.CadastrarAdministrativa(codigo, "Parada Válida", null, null, true, Cenario.Agora));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("AB")]
    [InlineData("  AB  ")]
    public void Nome_AusenteOuCurto_ERecusado(string? nome)
    {
        var excecao = Assert.Throws<ValidacaoDominioException>(() => Parada.CriarSimulada("SIM-P001", nome!, Cenario.Agora));

        Assert.Equal("nome", excecao.Campo);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(60)]
    [InlineData(120)]
    public void Nome_ComComprimentoDentroDaFaixa_EAceito(int comprimento)
    {
        Parada parada = Parada.CriarSimulada("SIM-P001", new string('a', comprimento), Cenario.Agora);

        Assert.Equal(comprimento, parada.Nome.Length);
    }

    [Fact]
    public void Nome_Com121Caracteres_ERecusado()
    {
        Assert.Throws<ValidacaoDominioException>(() => Parada.CriarSimulada("SIM-P001", new string('a', 121), Cenario.Agora));
    }

    [Fact]
    public void CriarSimulada_SempreTemOrigemSimuladoEDadosSimulados()
    {
        Parada parada = Cenario.ParadaSimulada();

        Assert.Equal(OrigemDado.Simulado, parada.Origem);
        Assert.True(parada.DadosSimulados);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CadastrarAdministrativa_MantemANaturezaDeclaradaSeparadaDaOrigem(bool dadosSimulados)
    {
        Parada parada = Cenario.ParadaAdministrativa(dadosSimulados);

        Assert.Equal(OrigemDado.CadastroAdministrativo, parada.Origem);
        Assert.Equal(dadosSimulados, parada.DadosSimulados);
    }

    [Fact]
    public void CadastrarAdministrativa_NaoFicticia_AceitaEnderecoECoordenada()
    {
        var coordenada = new Coordenada(-23.5505, -46.6333);

        Parada parada = Parada.CadastrarAdministrativa("PARADA-0001", "Parada Real", "  Avenida Exemplo, 100  ", coordenada, false, Cenario.Agora);

        Assert.Equal("Avenida Exemplo, 100", parada.Endereco);
        Assert.Equal(coordenada, parada.Coordenada);
    }

    [Fact]
    public void CadastrarAdministrativa_Ficticia_NaoAceitaEnderecoNemCoordenada()
    {
        Assert.Throws<ValidacaoDominioException>(() =>
            Parada.CadastrarAdministrativa("TESTE-P003", "Parada Cadastro de Teste", "Rua Inventada, 1", null, true, Cenario.Agora));
        Assert.Throws<ValidacaoDominioException>(() =>
            Parada.CadastrarAdministrativa("TESTE-P003", "Parada Cadastro de Teste", null, new Coordenada(0, 0), true, Cenario.Agora));
    }

    [Fact]
    public void Endereco_SoComEspacos_ViraNulo_ECom201Caracteres_ERecusado()
    {
        Parada parada = Parada.CadastrarAdministrativa("PARADA-0001", "Parada Real", "   ", null, false, Cenario.Agora);
        Assert.Null(parada.Endereco);

        var excecao = Assert.Throws<ValidacaoDominioException>(() =>
            Parada.CadastrarAdministrativa("PARADA-0002", "Parada Real", new string('r', 201), null, false, Cenario.Agora));
        Assert.Equal("endereco", excecao.Campo);
    }

    [Fact]
    public void AtualizarDados_TrocaOsDadosEditaveis_SemMudarCodigoOrigemENatureza()
    {
        Parada parada = Parada.CadastrarAdministrativa("PARADA-0001", "Nome Antigo", null, null, false, Cenario.Agora);

        parada.AtualizarDados("Nome Novo", "Rua Nova, 10", new Coordenada(-23.6, -46.7));

        Assert.Equal("Nome Novo", parada.Nome);
        Assert.Equal("Rua Nova, 10", parada.Endereco);
        Assert.Equal(new Coordenada(-23.6, -46.7), parada.Coordenada);
        Assert.Equal("PARADA-0001", parada.CodigoExterno);
        Assert.Equal(OrigemDado.CadastroAdministrativo, parada.Origem);
        Assert.False(parada.DadosSimulados);
    }

    [Fact]
    public void AtualizarDados_ComValorInvalido_NaoAlteraNenhumCampo()
    {
        Parada parada = Parada.CadastrarAdministrativa("PARADA-0001", "Nome Antigo", "Rua Antiga, 1", null, false, Cenario.Agora);

        Assert.Throws<ValidacaoDominioException>(() => parada.AtualizarDados("Nome Novo", new string('r', 201), null));

        Assert.Equal("Nome Antigo", parada.Nome);
        Assert.Equal("Rua Antiga, 1", parada.Endereco);
    }

    [Fact]
    public void AtualizarDados_DeParadaFicticia_ContinuaSemEndereco()
    {
        Parada parada = Cenario.ParadaSimulada();

        Assert.Throws<ValidacaoDominioException>(() => parada.AtualizarDados("Outro Nome", "Rua Real, 5", null));

        parada.AtualizarDados("Outro Nome", null, null);
        Assert.Equal("Outro Nome", parada.Nome);
    }

    [Fact]
    public void DesativarEReativar_AlternaOEstado()
    {
        Parada parada = Cenario.ParadaSimulada();

        parada.Desativar();
        Assert.False(parada.Ativa);

        parada.Ativar();
        Assert.True(parada.Ativa);
    }

    [Fact]
    public void AssociarLinha_CriaAssociacaoComParadaLinhaEInstanteUtc()
    {
        Parada parada = Cenario.ParadaSimulada();
        Linha linha = Cenario.LinhaSimulada();

        ParadaLinha associacao = parada.AssociarLinha(linha, Cenario.Agora);

        Assert.Same(parada, associacao.Parada);
        Assert.Same(linha, associacao.Linha);
        Assert.Equal(TimeSpan.Zero, associacao.AssociadaEmUtc.Offset);
        Assert.True(parada.AtendidaPor(linha));
        Assert.Single(parada.LinhasAssociadas);
    }

    [Fact]
    public void AssociarLinha_RepetidaComAMesmaInstancia_NaoDuplicaEDevolveAExistente()
    {
        (Parada parada, Linha linha, ParadaLinha primeira) = Cenario.ParadaComLinha();

        ParadaLinha segunda = parada.AssociarLinha(linha, Cenario.Agora.AddMinutes(5));

        Assert.Same(primeira, segunda);
        Assert.Single(parada.LinhasAssociadas);
        Assert.Equal(Cenario.Agora, segunda.AssociadaEmUtc);
    }

    [Fact]
    public void AssociarLinha_ComOutraInstanciaDaMesmaLinhaPersistida_NaoDuplica()
    {
        Parada parada = Cenario.ParadaSimulada().ComId(1);
        parada.AssociarLinha(Cenario.LinhaSimulada().ComId(4), Cenario.Agora);
        Linha mesmaLinhaCarregadaDeNovo = Cenario.LinhaSimulada().ComId(4);

        parada.AssociarLinha(mesmaLinhaCarregadaDeNovo, Cenario.Agora);

        Assert.Single(parada.LinhasAssociadas);
        Assert.True(parada.AtendidaPor(mesmaLinhaCarregadaDeNovo));
    }

    [Fact]
    public void AssociarLinha_DuasInstanciasNaoPersistidasComOMesmoCodigo_NaoDuplica()
    {
        Parada parada = Cenario.ParadaSimulada();

        parada.AssociarLinha(Cenario.LinhaSimulada(), Cenario.Agora);
        parada.AssociarLinha(Cenario.LinhaSimulada(), Cenario.Agora);

        Assert.Single(parada.LinhasAssociadas);
    }

    [Fact]
    public void AssociarLinha_LinhasDiferentes_CriaUmaAssociacaoParaCada()
    {
        Parada parada = Cenario.ParadaSimulada();

        parada.AssociarLinha(Cenario.LinhaSimulada("SIM-L001", "SIM-101"), Cenario.Agora);
        parada.AssociarLinha(Cenario.LinhaSimulada("SIM-L002", "SIM-102"), Cenario.Agora);
        parada.AssociarLinha(Cenario.LinhaSimulada("SIM-L004", "SIM-104"), Cenario.Agora);

        Assert.Equal(3, parada.LinhasAssociadas.Count);
    }

    [Fact]
    public void AssociarLinha_Inativa_LancaLinhaInativa()
    {
        Parada parada = Cenario.ParadaSimulada();
        Linha linha = Cenario.LinhaSimulada();
        linha.Desativar();

        var excecao = Assert.Throws<RegraNegocioException>(() => parada.AssociarLinha(linha, Cenario.Agora));

        Assert.Equal(RegraNegocioException.LinhaInativa, excecao.Codigo);
        Assert.Empty(parada.LinhasAssociadas);
    }

    [Fact]
    public void AssociarLinha_EmParadaInativa_LancaParadaInativa_EVoltaAFuncionarAoReativar()
    {
        Parada parada = Cenario.ParadaSimulada();
        Linha linha = Cenario.LinhaSimulada();
        parada.Desativar();

        var excecao = Assert.Throws<RegraNegocioException>(() => parada.AssociarLinha(linha, Cenario.Agora));
        Assert.Equal(RegraNegocioException.ParadaInativa, excecao.Codigo);

        parada.Ativar();
        parada.AssociarLinha(linha, Cenario.Agora);
        Assert.True(parada.AtendidaPor(linha));
    }

    [Fact]
    public void ObterAssociacao_DeLinhaQueNaoAtendeAParada_LancaLinhaNaoAtendeParada()
    {
        (Parada parada, _, _) = Cenario.ParadaComLinha();

        var excecao = Assert.Throws<RegraNegocioException>(() => parada.ObterAssociacao(Cenario.LinhaSimulada("SIM-L003", "SIM-203")));

        Assert.Equal(RegraNegocioException.LinhaNaoAtendeParada, excecao.Codigo);
    }

    [Fact]
    public void LinhasAssociadas_NaoPodeSerAlteradaPorFora()
    {
        (Parada parada, _, _) = Cenario.ParadaComLinha();

        var colecao = Assert.IsAssignableFrom<ICollection<ParadaLinha>>(parada.LinhasAssociadas);

        Assert.True(colecao.IsReadOnly);
        Assert.Throws<NotSupportedException>(colecao.Clear);
    }
}

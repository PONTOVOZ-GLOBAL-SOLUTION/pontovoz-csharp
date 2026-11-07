using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.Rules;

namespace PontoVoz.Api.Domain.Entities;

/// <summary>
/// Linha de ônibus em um sentido. Cada sentido é um cadastro próprio (contracts/api-v1.md, 3.4).
/// O código, a origem e a natureza do dado não mudam depois da criação.
/// </summary>
public sealed class Linha : EntidadeBase
{
    private Linha()
    {
        Codigo = Letreiro = TerminalPrincipal = TerminalSecundario = string.Empty;
    }

    private Linha(string codigo, string letreiro, string terminalPrincipal, string terminalSecundario, SentidoLinha sentido, OrigemDado origem, bool dadosSimulados, DateTimeOffset criadoEm)
        : base(criadoEm)
    {
        Codigo = RegrasCadastro.Codigo(codigo, "codigo", origem);
        Letreiro = RegrasCadastro.Texto(letreiro, "letreiro", 1, 30);
        RegrasCadastro.PrefixoReservado(Letreiro, "letreiro", origem);
        TerminalPrincipal = RegrasCadastro.Texto(terminalPrincipal, "terminalPrincipal", 3, 120);
        TerminalSecundario = RegrasCadastro.Texto(terminalSecundario, "terminalSecundario", 3, 120);
        Sentido = RegrasCadastro.Definido(sentido, "sentido");
        Origem = origem;
        DadosSimulados = dadosSimulados;
        Ativa = true;
    }

    public string Codigo { get; private set; }

    public string Letreiro { get; private set; }

    public string TerminalPrincipal { get; private set; }

    public string TerminalSecundario { get; private set; }

    public SentidoLinha Sentido { get; private set; }

    public bool Ativa { get; private set; }

    public OrigemDado Origem { get; private set; }

    public bool DadosSimulados { get; private set; }

    /// <summary>Linha do seed de demonstração: código e letreiro com <c>SIM-</c> e sempre simulada.</summary>
    public static Linha CriarSimulada(string codigo, string letreiro, string terminalPrincipal, string terminalSecundario, SentidoLinha sentido, DateTimeOffset criadoEm) =>
        new(codigo, letreiro, terminalPrincipal, terminalSecundario, sentido, OrigemDado.Simulado, dadosSimulados: true, criadoEm);

    /// <summary>
    /// Linha cadastrada pela rota administrativa. O operador declara se o cadastro é fictício;
    /// o domínio não presume nenhum dos dois.
    /// </summary>
    public static Linha CadastrarAdministrativa(string codigo, string letreiro, string terminalPrincipal, string terminalSecundario, SentidoLinha sentido, bool dadosSimulados, DateTimeOffset criadoEm) =>
        new(codigo, letreiro, terminalPrincipal, terminalSecundario, sentido, OrigemDado.CadastroAdministrativo, dadosSimulados, criadoEm);

    public void Ativar() => Ativa = true;

    /// <summary>Desativar substitui a exclusão: associações e passagens continuam no histórico.</summary>
    public void Desativar() => Ativa = false;
}

using System.Reflection;

using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;

namespace PontoVoz.Tests.Domain;

/// <summary>Dados e utilitários comuns aos testes de domínio.</summary>
internal static class Cenario
{
    /// <summary>10/11/2026 às 14h32 em São Paulo, o mesmo instante dos exemplos do contrato.</summary>
    public static readonly DateTimeOffset Agora = new(2026, 11, 10, 14, 32, 0, TimeSpan.FromHours(-3));

    public static Parada ParadaSimulada(string codigo = "SIM-P001", string nome = "Parada Demonstração Centro") =>
        Parada.CriarSimulada(codigo, nome, Agora);

    public static Parada ParadaAdministrativa(bool dadosSimulados, string codigo = "TESTE-P003") =>
        Parada.CadastrarAdministrativa(codigo, "Parada Cadastro de Teste", null, null, dadosSimulados, Agora);

    public static Linha LinhaSimulada(string codigo = "SIM-L004", string letreiro = "SIM-104") =>
        Linha.CriarSimulada(codigo, letreiro, "Terminal Demonstração Centro", "Terminal Demonstração Norte", SentidoLinha.PrincipalParaSecundario, Agora);

    public static Linha LinhaAdministrativa(bool dadosSimulados, string codigo = "TESTE-L005") =>
        Linha.CadastrarAdministrativa(codigo, "TESTE-105", "Terminal Demonstração Centro", "Terminal Demonstração Sul", SentidoLinha.PrincipalParaSecundario, dadosSimulados, Agora);

    public static Destino DestinoSimulado() =>
        Destino.CriarSimulado("Terminal Demonstração Norte", TipoDestino.Terminal, Agora);

    /// <summary>Parada e linha simuladas já associadas.</summary>
    public static (Parada Parada, Linha Linha, ParadaLinha Associacao) ParadaComLinha()
    {
        Parada parada = ParadaSimulada();
        Linha linha = LinhaSimulada();
        ParadaLinha associacao = parada.AssociarLinha(linha, Agora);
        return (parada, linha, associacao);
    }

    /// <summary>
    /// Simula o identificador que o banco atribuirá a partir do B3. O domínio não permite
    /// definir o <c>Id</c>, então o teste usa reflexão, como faria o EF Core ao materializar.
    /// </summary>
    public static T ComId<T>(this T entidade, int id)
        where T : EntidadeBase
    {
        typeof(EntidadeBase).GetProperty(nameof(EntidadeBase.Id), BindingFlags.Instance | BindingFlags.Public)!
            .SetValue(entidade, id);
        return entidade;
    }
}

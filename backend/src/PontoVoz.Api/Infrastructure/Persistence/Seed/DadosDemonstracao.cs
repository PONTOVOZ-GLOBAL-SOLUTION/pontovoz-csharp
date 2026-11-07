using PontoVoz.Api.Domain.Entities;
using PontoVoz.Api.Domain.Enums;

namespace PontoVoz.Api.Infrastructure.Persistence.Seed;

/// <summary>
/// Dados fictícios de demonstração, coerentes com <c>contracts/exemplos/</c>. Todos são criados
/// pelas fábricas do domínio, com origem <see cref="OrigemDado.Simulado"/> e prefixo <c>SIM-</c>.
/// Esta classe só monta os objetos; quem decide se eles são gravados é
/// <see cref="ProvisionamentoDemonstracao"/>.
/// </summary>
public sealed class DadosDemonstracao
{
    private const string TerminalCentro = "Terminal Demonstração Centro";
    private const string TerminalNorte = "Terminal Demonstração Norte";
    private const string TerminalSul = "Terminal Demonstração Sul";

    private DadosDemonstracao(DateTimeOffset instante)
    {
        Parada centro = Parada.CriarSimulada("SIM-P001", "Parada Demonstração Centro", instante);
        Parada avenida = Parada.CriarSimulada("SIM-P002", "Parada Demonstração Avenida", instante);

        Linha l001 = Linha.CriarSimulada("SIM-L001", "SIM-101", TerminalCentro, TerminalNorte, SentidoLinha.PrincipalParaSecundario, instante);
        Linha l002 = Linha.CriarSimulada("SIM-L002", "SIM-102", TerminalCentro, TerminalNorte, SentidoLinha.PrincipalParaSecundario, instante);
        Linha l003 = Linha.CriarSimulada("SIM-L003", "SIM-203", TerminalCentro, TerminalSul, SentidoLinha.PrincipalParaSecundario, instante);
        Linha l004 = Linha.CriarSimulada("SIM-L004", "SIM-104", TerminalCentro, TerminalNorte, SentidoLinha.PrincipalParaSecundario, instante);

        Destino destinoCentro = Destino.CriarSimulado(TerminalCentro, TipoDestino.Terminal, instante);
        Destino destinoSul = Destino.CriarSimulado(TerminalSul, TipoDestino.Terminal, instante);
        Destino destinoNorte = Destino.CriarSimulado(TerminalNorte, TipoDestino.Terminal, instante);

        ParadaLinha centroL001 = centro.AssociarLinha(l001, instante);
        ParadaLinha centroL002 = centro.AssociarLinha(l002, instante);
        ParadaLinha centroL004 = centro.AssociarLinha(l004, instante);
        ParadaLinha avenidaL002 = avenida.AssociarLinha(l002, instante);
        ParadaLinha avenidaL003 = avenida.AssociarLinha(l003, instante);

        Paradas = [centro, avenida];
        Linhas = [l001, l002, l003, l004];
        Destinos = [destinoCentro, destinoSul, destinoNorte];
        Percursos =
        [
            PercursoEstimado.CriarSimulado(centroL001, destinoNorte, 30),
            PercursoEstimado.CriarSimulado(centroL002, destinoNorte, 40),
            PercursoEstimado.CriarSimulado(centroL004, destinoNorte, 20),
            PercursoEstimado.CriarSimulado(avenidaL002, destinoNorte, 35),
            PercursoEstimado.CriarSimulado(avenidaL003, destinoSul, 25),
        ];
        Passagens =
        [
            PassagemSimulada.CriarNoProvisionamento(centroL002, "SIM-OBS-001", TimeSpan.FromMinutes(15), instante, "SIM-V2003"),
            PassagemSimulada.CriarNoProvisionamento(avenidaL003, "SIM-OBS-002", TimeSpan.FromMinutes(25), instante, "SIM-V3001"),
        ];
    }

    /// <summary>Código das paradas do seed; a existência de qualquer uma indica banco já provisionado.</summary>
    public static IReadOnlyList<string> CodigosParadas { get; } = ["SIM-P001", "SIM-P002"];

    public IReadOnlyList<Parada> Paradas { get; }

    public IReadOnlyList<Linha> Linhas { get; }

    public IReadOnlyList<Destino> Destinos { get; }

    public IReadOnlyList<PercursoEstimado> Percursos { get; }

    public IReadOnlyList<PassagemSimulada> Passagens { get; }

    public int TotalRegistros =>
        Paradas.Count + Linhas.Count + Destinos.Count + Paradas.Sum(parada => parada.LinhasAssociadas.Count) + Percursos.Count + Passagens.Count;

    /// <summary>Monta os dados com horários relativos ao instante do provisionamento.</summary>
    public static DadosDemonstracao Criar(DateTimeOffset instanteProvisionamento) => new(instanteProvisionamento);

    /// <summary>
    /// Adiciona o grafo ao contexto. As associações entram junto com as paradas, pela navegação.
    /// Os ids são gerados pelo banco e não fazem parte do contrato: os dados de demonstração são
    /// identificados pelos códigos <c>SIM-</c> e, nos destinos, pelo nome. Nenhuma regra depende
    /// da ordem de inserção.
    /// </summary>
    public void AdicionarA(PontoVozDbContext contexto)
    {
        ArgumentNullException.ThrowIfNull(contexto);
        contexto.Linhas.AddRange(Linhas);
        contexto.Paradas.AddRange(Paradas);
        contexto.Destinos.AddRange(Destinos);
        contexto.PercursosEstimados.AddRange(Percursos);
        contexto.RegistrosPassagem.AddRange(Passagens);
    }
}

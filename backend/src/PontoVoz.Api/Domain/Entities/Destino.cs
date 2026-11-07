using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.Rules;

namespace PontoVoz.Api.Domain.Entities;

/// <summary>
/// Destino que a pessoa pode pedir ao totem. No MVP os destinos são só os de demonstração
/// criados pelo seed; o contrato não tem rota administrativa de destinos.
/// </summary>
public sealed class Destino : EntidadeBase
{
    private Destino()
    {
        Nome = string.Empty;
    }

    private Destino(string nome, TipoDestino tipo, DateTimeOffset criadoEm)
        : base(criadoEm)
    {
        Nome = RegrasCadastro.Texto(nome, "nome", 3, 120);
        Tipo = RegrasCadastro.Definido(tipo, "tipo");
        Origem = OrigemDado.Simulado;
        DadosSimulados = true;
        Ativo = true;
    }

    public string Nome { get; private set; }

    public TipoDestino Tipo { get; private set; }

    public bool Ativo { get; private set; }

    public OrigemDado Origem { get; private set; }

    public bool DadosSimulados { get; private set; }

    public static Destino CriarSimulado(string nome, TipoDestino tipo, DateTimeOffset criadoEm) =>
        new(nome, tipo, criadoEm);
}

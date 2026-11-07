using System.Diagnostics.CodeAnalysis;

using PontoVoz.Api.Domain.Enums;
using PontoVoz.Api.Domain.Exceptions;
using PontoVoz.Api.Domain.Rules;
using PontoVoz.Api.Domain.ValueObjects;

namespace PontoVoz.Api.Domain.Entities;

/// <summary>
/// Ponto de ônibus onde o totem pode estar instalado.
/// O código externo, a origem e a natureza do dado (<see cref="DadosSimulados"/>) são definidos
/// na criação e não mudam. Nome, endereço e coordenada mudam só por <see cref="AtualizarDados"/>.
/// </summary>
public sealed class Parada : EntidadeBase
{
    private readonly List<ParadaLinha> _linhasAssociadas = [];

    private Parada()
    {
        CodigoExterno = Nome = string.Empty;
    }

    private Parada(string codigoExterno, string nome, string? endereco, Coordenada? coordenada, OrigemDado origem, bool dadosSimulados, DateTimeOffset criadoEm)
        : base(criadoEm)
    {
        CodigoExterno = RegrasCadastro.Codigo(codigoExterno, "codigoExterno", origem);
        Origem = origem;
        DadosSimulados = dadosSimulados;
        Ativa = true;
        DefinirDados(nome, endereco, coordenada);
    }

    public string CodigoExterno { get; private set; }

    public string Nome { get; private set; }

    /// <summary><c>null</c> nos dados fictícios, para não associar um ponto inventado a um endereço real.</summary>
    public string? Endereco { get; private set; }

    /// <summary><c>null</c> nos dados fictícios, pelo mesmo motivo de <see cref="Endereco"/>.</summary>
    public Coordenada? Coordenada { get; private set; }

    public bool Ativa { get; private set; }

    public OrigemDado Origem { get; private set; }

    public bool DadosSimulados { get; private set; }

    public IReadOnlyCollection<ParadaLinha> LinhasAssociadas => _linhasAssociadas.AsReadOnly();

    /// <summary>Parada do seed de demonstração: código com <c>SIM-</c>, sem endereço nem coordenada.</summary>
    public static Parada CriarSimulada(string codigoExterno, string nome, DateTimeOffset criadoEm) =>
        new(codigoExterno, nome, endereco: null, coordenada: null, OrigemDado.Simulado, dadosSimulados: true, criadoEm);

    /// <summary>
    /// Parada cadastrada pela rota administrativa. O operador declara se o cadastro é fictício
    /// (<paramref name="dadosSimulados"/>); o domínio não presume que um cadastro manual é real
    /// nem que foi verificado.
    /// </summary>
    public static Parada CadastrarAdministrativa(string codigoExterno, string nome, string? endereco, Coordenada? coordenada, bool dadosSimulados, DateTimeOffset criadoEm) =>
        new(codigoExterno, nome, endereco, coordenada, OrigemDado.CadastroAdministrativo, dadosSimulados, criadoEm);

    /// <summary>Substitui os dados editáveis. Código, origem e natureza do dado não mudam.</summary>
    public void AtualizarDados(string nome, string? endereco, Coordenada? coordenada) =>
        DefinirDados(nome, endereco, coordenada);

    public void Ativar() => Ativa = true;

    /// <summary>Desativar substitui a exclusão: associações e passagens continuam no histórico.</summary>
    public void Desativar() => Ativa = false;

    public bool AtendidaPor(Linha linha)
    {
        ArgumentNullException.ThrowIfNull(linha);
        return _linhasAssociadas.Exists(associacao => associacao.Envolve(linha));
    }

    /// <summary>
    /// Associa uma linha à parada. É idempotente, como <c>PUT /admin/paradas/{id}/linhas/{linhaId}</c>:
    /// repetir a associação não cria outra e devolve a existente.
    /// </summary>
    public ParadaLinha AssociarLinha(Linha linha, DateTimeOffset associadaEm)
    {
        ArgumentNullException.ThrowIfNull(linha);
        DateTimeOffset instante = RegrasCadastro.Utc(associadaEm, "associadaEm");

        if (!Ativa)
        {
            throw new RegraNegocioException(RegraNegocioException.ParadaInativa, "A parada está inativa e não recebe novas linhas.");
        }

        if (!linha.Ativa)
        {
            throw new RegraNegocioException(RegraNegocioException.LinhaInativa, "A linha está inativa e não pode ser associada.");
        }

        ParadaLinha? existente = _linhasAssociadas.Find(associacao => associacao.Envolve(linha));
        if (existente is not null)
        {
            return existente;
        }

        var associacao = new ParadaLinha(this, linha, instante);
        _linhasAssociadas.Add(associacao);
        return associacao;
    }

    /// <summary>A associação desta parada com a linha, ou exceção 422 quando a linha não a atende.</summary>
    public ParadaLinha ObterAssociacao(Linha linha)
    {
        ArgumentNullException.ThrowIfNull(linha);
        return _linhasAssociadas.Find(associacao => associacao.Envolve(linha))
            ?? throw new RegraNegocioException(RegraNegocioException.LinhaNaoAtendeParada, "A linha não atende esta parada.");
    }

    [MemberNotNull(nameof(Nome))]
    private void DefinirDados(string nome, string? endereco, Coordenada? coordenada)
    {
        string nomeValido = RegrasCadastro.Texto(nome, "nome", 3, 120);
        string? enderecoValido = RegrasCadastro.TextoOpcional(endereco, "endereco", 200);

        if (DadosSimulados && (enderecoValido is not null || coordenada is not null))
        {
            throw new ValidacaoDominioException("endereco", "Uma parada fictícia não pode ter endereço nem coordenada reais.");
        }

        Nome = nomeValido;
        Endereco = enderecoValido;
        Coordenada = coordenada;
    }
}

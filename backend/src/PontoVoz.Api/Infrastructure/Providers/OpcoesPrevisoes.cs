namespace PontoVoz.Api.Infrastructure.Providers;

/// <summary>
/// Seção <c>Previsoes</c> da configuração. Os padrões mantêm a demonstração sem dependência
/// externa: provedor simulado, sem indisponibilidade simulada e Olho Vivo desligado.
/// </summary>
public sealed class OpcoesPrevisoes
{
    public const string Secao = "Previsoes";

    /// <summary>
    /// Requisito R6: faz o provedor responder como indisponível (503 no endpoint de previsões),
    /// para testes e para demonstrar o erro. Desligado por padrão; não há rota para ligá-lo.
    /// </summary>
    public bool SimularIndisponibilidade { get; set; }

    public OpcoesOlhoVivo OlhoVivo { get; set; } = new();
}

/// <summary>
/// Adaptador da API Olho Vivo da SPTrans (requisito R8). Desligado por padrão. Mesmo ligado, só
/// consulta paradas e linhas com código oficial declarado em <see cref="Paradas"/> e
/// <see cref="Linhas"/>; os dados <c>SIM-</c> sempre usam o provedor simulado.
/// </summary>
public sealed class OpcoesOlhoVivo
{
    /// <summary>Endereço base verificado na documentação oficial da SPTrans (versão 2.1, só HTTPS).</summary>
    public const string UrlBasePadrao = "https://api.olhovivo.sptrans.com.br/v2.1/";

    public bool Habilitado { get; set; }

    public string UrlBase { get; set; } = UrlBasePadrao;

    /// <summary>Token de acesso da SPTrans. Só por user-secrets ou variável de ambiente; nunca em arquivo versionado.</summary>
    public string? Token { get; set; }

    /// <summary>Tempo máximo de cada chamada HTTP (autenticação e previsão). Padrão: 5 segundos.</summary>
    public int TempoLimiteSegundos { get; set; } = 5;

    /// <summary>
    /// Correspondência declarada entre o <c>codigoExterno</c> de uma parada do PontoVoz e o
    /// <c>codigoParada</c> da SPTrans. Sem correspondência, a parada não é enviada à SPTrans.
    /// </summary>
    public Dictionary<string, int> Paradas { get; set; } = [];

    /// <summary>Correspondência declarada entre o código de uma linha do PontoVoz e o <c>codigoLinha</c> (<c>cl</c>) da SPTrans.</summary>
    public Dictionary<string, int> Linhas { get; set; } = [];

    /// <summary>O prazo em uso: pelo menos 1 segundo, mesmo com um valor configurado menor.</summary>
    public TimeSpan Prazo => TimeSpan.FromSeconds(Math.Max(1, TempoLimiteSegundos));

    /// <summary>Habilitado e com token: só então o adaptador pode fazer chamadas reais.</summary>
    public bool PodeConsultar => Habilitado && !string.IsNullOrWhiteSpace(Token);
}

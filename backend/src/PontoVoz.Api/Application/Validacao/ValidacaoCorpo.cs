using PontoVoz.Api.Application.Dtos;
using PontoVoz.Api.Domain.Exceptions;
using PontoVoz.Api.Domain.ValueObjects;

namespace PontoVoz.Api.Application.Validacao;

/// <summary>
/// Valida o corpo de uma rota administrativa antes de qualquer consulta ao banco (ordem da
/// seção 2 do contrato: 400 antes de 404 e 422). Verifica a presença dos campos obrigatórios e
/// aplica as <b>mesmas</b> regras de formato e de faixa do domínio (<c>RegrasCadastro</c>,
/// <see cref="Coordenada"/>), sem reescrevê-las: cada regra que lança
/// <see cref="ValidacaoDominioException"/> vira um erro do campo, e todos são devolvidos juntos.
/// </summary>
internal sealed class ValidacaoCorpo : ColetorErros
{
    public const string MensagemCorpoAusente = "O corpo da requisição é obrigatório.";

    /// <summary>Corpo inexistente (requisição sem JSON ou com <c>null</c>): erro em <c>corpo</c>, lançado na hora.</summary>
    public static T Exigir<T>(T? corpo)
        where T : class
    {
        if (corpo is null)
        {
            var validacao = new ValidacaoCorpo();
            validacao.Adicionar("corpo", MensagemCorpoAusente);
            validacao.Confirmar();
        }

        return corpo!;
    }

    /// <summary>Campo obrigatório de tipo valor (<c>bool</c>, enum, número, data). <c>null</c> quando ausente.</summary>
    public T? Obrigatorio<T>(T? valor, string campo)
        where T : struct
    {
        if (valor is null)
        {
            AdicionarAusente(campo);
        }

        return valor;
    }

    /// <summary>
    /// Campo de texto obrigatório: ausente vira "obrigatório"; presente passa pela
    /// <paramref name="regra"/> do domínio, que devolve o valor normalizado.
    /// </summary>
    public string? Texto(string? valor, string campo, Func<string, string> regra)
    {
        if (valor is null)
        {
            AdicionarAusente(campo);
            return null;
        }

        return Regra(() => regra(valor));
    }

    /// <summary>Identificador obrigatório do corpo (por exemplo, <c>paradaId</c>): inteiro ≥ 1.</summary>
    public int? Id(int? valor, string campo)
    {
        if (valor is null)
        {
            AdicionarAusente(campo);
        }
        else if (valor < 1)
        {
            Adicionar(campo, $"O campo {campo} deve ser um inteiro maior ou igual a 1.");
        }

        return valor;
    }

    /// <summary>Coordenada opcional; quando informada, latitude e longitude são obrigatórias e seguem as faixas do domínio.</summary>
    public Coordenada? Coordenada(CoordenadaEntradaDto? coordenada)
    {
        if (coordenada is null)
        {
            return null;
        }

        double? latitude = Obrigatorio(coordenada.Latitude, "coordenada.latitude");
        double? longitude = Obrigatorio(coordenada.Longitude, "coordenada.longitude");
        return latitude is double lat && longitude is double lon
            ? Regra(() => new Coordenada(lat, lon))
            : null;
    }

    /// <summary>Executa uma regra do domínio e guarda o erro de formato ou de faixa, se houver.</summary>
    public T? Regra<T>(Func<T> regra)
    {
        try
        {
            return regra();
        }
        catch (ValidacaoDominioException erro)
        {
            Adicionar(erro.Campo, erro.Message);
            return default;
        }
    }

    /// <summary>Acrescenta um erro calculado pelo service (por exemplo, a janela de horário de uma observação).</summary>
    public void Erro(string campo, string mensagem) => Adicionar(campo, mensagem);

    private void AdicionarAusente(string campo) => Adicionar(campo, $"O campo {campo} é obrigatório.");
}

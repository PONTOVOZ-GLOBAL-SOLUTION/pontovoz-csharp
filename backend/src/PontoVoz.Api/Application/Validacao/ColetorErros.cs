using PontoVoz.Api.Application.Erros;

namespace PontoVoz.Api.Application.Validacao;

/// <summary>
/// Junta os erros de validação por campo (camelCase) e lança todos de uma vez, para o cliente
/// receber a lista completa em <c>errors</c> do ProblemDetails 400.
/// </summary>
internal abstract class ColetorErros
{
    private readonly Dictionary<string, List<string>> _erros = [];

    public bool TemErro(string campo) => _erros.ContainsKey(campo);

    /// <summary>Lança <see cref="ErroAplicacaoException"/> de validação se algum campo falhou.</summary>
    public void Confirmar()
    {
        if (_erros.Count > 0)
        {
            throw ErroAplicacaoException.Validacao(_erros.ToDictionary(par => par.Key, par => par.Value.ToArray()));
        }
    }

    protected void Adicionar(string campo, string mensagem)
    {
        if (!_erros.TryGetValue(campo, out List<string>? mensagens))
        {
            mensagens = [];
            _erros[campo] = mensagens;
        }

        mensagens.Add(mensagem);
    }
}

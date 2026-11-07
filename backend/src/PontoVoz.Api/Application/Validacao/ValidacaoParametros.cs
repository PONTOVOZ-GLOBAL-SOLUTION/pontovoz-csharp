using System.Text.RegularExpressions;

using PontoVoz.Api.Application.Consultas;
using PontoVoz.Api.Application.Erros;

namespace PontoVoz.Api.Application.Validacao;

/// <summary>
/// Valida os parâmetros de uma consulta pelas regras do contrato (seções 1.5, 1.6 e 4.1) e reúne
/// todos os erros antes de lançar, para o cliente receber de uma vez a lista completa
/// (<c>errors</c> do ProblemDetails). Não consulta o banco.
/// </summary>
internal sealed partial class ValidacaoParametros
{
    public const int TermoMinimo = 2;
    public const int TermoMaximo = 60;

    private readonly Dictionary<string, List<string>> _erros = [];

    [GeneratedRegex(@"\A[A-Za-z0-9-]{3,30}\z")]
    private static partial Regex FormatoCodigoExterno();

    /// <summary>Termo sem os espaços das pontas; vazio ou só com espaços conta como ausente.</summary>
    public string? Termo(string? termo)
    {
        if (string.IsNullOrWhiteSpace(termo))
        {
            return null;
        }

        string texto = termo.Trim();
        if (texto.Length is < TermoMinimo or > TermoMaximo)
        {
            Adicionar("termo", $"O termo deve ter entre {TermoMinimo} e {TermoMaximo} caracteres.");
        }

        return texto;
    }

    public string? CodigoExterno(string? codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            return null;
        }

        string texto = codigo.Trim();
        if (!FormatoCodigoExterno().IsMatch(texto))
        {
            Adicionar("codigoExterno", "O código externo deve ter de 3 a 30 caracteres entre letras, dígitos e hífen.");
        }

        return texto;
    }

    /// <summary>Página e tamanho com os padrões do contrato; <c>null</c> quando algum está fora da faixa.</summary>
    public Paginacao? Paginacao(int? pagina, int? tamanhoPagina)
    {
        int numero = pagina ?? Consultas.Paginacao.PaginaPadrao;
        int tamanho = tamanhoPagina ?? Consultas.Paginacao.TamanhoPadrao;
        bool valida = true;

        if (numero < 1)
        {
            Adicionar("pagina", "A página deve ser maior ou igual a 1.");
            valida = false;
        }

        if (tamanho is < 1 or > Consultas.Paginacao.TamanhoMaximo)
        {
            Adicionar("tamanhoPagina", $"O tamanho da página deve estar entre 1 e {Consultas.Paginacao.TamanhoMaximo}.");
            valida = false;
        }

        return valida ? new Paginacao(numero, tamanho) : null;
    }

    /// <summary>Identificador opcional da query string (por exemplo, <c>linhaId</c>): se informado, ≥ 1.</summary>
    public int? IdOpcional(int? id, string campo)
    {
        if (id is < 1)
        {
            Adicionar(campo, $"O campo {campo} deve ser um inteiro maior ou igual a 1.");
        }

        return id;
    }

    /// <summary>Lança <see cref="ErroAplicacaoException"/> de validação se algum parâmetro falhou.</summary>
    public void Confirmar()
    {
        if (_erros.Count > 0)
        {
            throw ErroAplicacaoException.Validacao(_erros.ToDictionary(par => par.Key, par => par.Value.ToArray()));
        }
    }

    private void Adicionar(string campo, string mensagem)
    {
        if (!_erros.TryGetValue(campo, out List<string>? mensagens))
        {
            mensagens = [];
            _erros[campo] = mensagens;
        }

        mensagens.Add(mensagem);
    }
}

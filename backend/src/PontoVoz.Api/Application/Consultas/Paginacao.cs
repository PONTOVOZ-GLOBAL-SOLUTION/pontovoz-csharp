namespace PontoVoz.Api.Application.Consultas;

/// <summary>
/// Página pedida em uma listagem (contracts/api-v1.md, seção 1.5): <c>pagina</c> ≥ 1 e
/// <c>tamanhoPagina</c> de 1 a 50. Valores fora da faixa são erro de programação aqui; a
/// validação que devolve 400 ao cliente fica nos services e na camada HTTP.
/// É uma classe, não uma <c>struct</c>: assim <c>new Paginacao()</c> sempre passa pelo construtor
/// e recebe os valores padrão, em vez de campos zerados.
/// </summary>
public sealed record Paginacao
{
    public const int PaginaPadrao = 1;
    public const int TamanhoPadrao = 20;
    public const int TamanhoMaximo = 50;

    public Paginacao(int pagina = PaginaPadrao, int tamanhoPagina = TamanhoPadrao)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(pagina, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(tamanhoPagina, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(tamanhoPagina, TamanhoMaximo);
        Pagina = pagina;
        TamanhoPagina = tamanhoPagina;
    }

    public int Pagina { get; }

    public int TamanhoPagina { get; }

    /// <summary>Quantos itens pular no banco (<c>Skip</c>).</summary>
    public int Deslocamento => (Pagina - 1) * TamanhoPagina;
}

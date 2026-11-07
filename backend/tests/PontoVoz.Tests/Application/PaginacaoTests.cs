using PontoVoz.Api.Application.Consultas;

namespace PontoVoz.Tests.Application;

/// <summary>Faixas e cálculos da paginação (contracts/api-v1.md, seções 1.5 e 3.1).</summary>
public class PaginacaoTests
{
    [Fact]
    public void SemArgumentos_UsaPagina1ETamanho20_NuncaTamanhoZero()
    {
        var paginacao = new Paginacao();

        Assert.Equal((1, 20, 0), (paginacao.Pagina, paginacao.TamanhoPagina, paginacao.Deslocamento));
    }

    [Theory]
    [InlineData(1, 1, 0)]
    [InlineData(1, 50, 0)]
    [InlineData(2, 20, 20)]
    [InlineData(3, 7, 14)]
    public void DentroDaFaixa_CalculaODeslocamento(int pagina, int tamanho, int deslocamento)
    {
        var paginacao = new Paginacao(pagina, tamanho);

        Assert.Equal((pagina, tamanho, deslocamento), (paginacao.Pagina, paginacao.TamanhoPagina, paginacao.Deslocamento));
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-1, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 51)]
    [InlineData(1, -5)]
    public void ForaDaFaixa_ERecusada(int pagina, int tamanho)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Paginacao(pagina, tamanho));
    }

    [Theory]
    [InlineData(0, 20, 0)]
    [InlineData(1, 20, 1)]
    [InlineData(20, 20, 1)]
    [InlineData(21, 20, 2)]
    [InlineData(7, 2, 4)]
    public void TotalPaginas_SegueOContrato(int totalItens, int tamanho, int totalPaginas)
    {
        var pagina = new Pagina<string>([], 1, tamanho, totalItens);

        Assert.Equal(totalPaginas, pagina.TotalPaginas);
    }
}

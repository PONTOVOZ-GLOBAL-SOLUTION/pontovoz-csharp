using PontoVoz.Api.Domain.Exceptions;
using PontoVoz.Api.Domain.ValueObjects;

namespace PontoVoz.Tests.Domain;

public class CoordenadaTests
{
    [Theory]
    [InlineData(-90, -180)]
    [InlineData(90, 180)]
    [InlineData(0, 0)]
    [InlineData(-23.5505, -46.6333)]
    public void Coordenada_DentroDosLimites_EAceita(double latitude, double longitude)
    {
        var coordenada = new Coordenada(latitude, longitude);

        Assert.Equal(latitude, coordenada.Latitude);
        Assert.Equal(longitude, coordenada.Longitude);
    }

    [Theory]
    [InlineData(-90.0001, 0, "coordenada.latitude")]
    [InlineData(90.0001, 0, "coordenada.latitude")]
    [InlineData(double.NaN, 0, "coordenada.latitude")]
    [InlineData(0, -180.0001, "coordenada.longitude")]
    [InlineData(0, 180.0001, "coordenada.longitude")]
    [InlineData(0, double.PositiveInfinity, "coordenada.longitude")]
    [InlineData(0, double.NaN, "coordenada.longitude")]
    public void Coordenada_ForaDosLimites_ERecusada(double latitude, double longitude, string campo)
    {
        var excecao = Assert.Throws<ValidacaoDominioException>(() => new Coordenada(latitude, longitude));

        Assert.Equal(campo, excecao.Campo);
    }

    [Fact]
    public void Coordenada_EComparadaPorValor()
    {
        var primeira = new Coordenada(-23.5505, -46.6333);
        var segunda = new Coordenada(-23.5505, -46.6333);

        Assert.Equal(primeira, segunda);
        Assert.True(primeira == segunda);
        Assert.Equal(primeira.GetHashCode(), segunda.GetHashCode());
        Assert.NotEqual(primeira, new Coordenada(-23.5505, -46.6334));
    }
}

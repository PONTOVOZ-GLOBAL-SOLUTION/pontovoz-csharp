using PontoVoz.Api.Domain.Exceptions;

namespace PontoVoz.Api.Domain.ValueObjects;

/// <summary>Posição geográfica imutável, comparada por valor.</summary>
public sealed record Coordenada
{
    public Coordenada(double latitude, double longitude)
    {
        if (double.IsNaN(latitude) || latitude is < -90 or > 90)
        {
            throw new ValidacaoDominioException("coordenada.latitude", "A latitude deve estar entre -90 e 90.");
        }

        if (double.IsNaN(longitude) || longitude is < -180 or > 180)
        {
            throw new ValidacaoDominioException("coordenada.longitude", "A longitude deve estar entre -180 e 180.");
        }

        Latitude = latitude;
        Longitude = longitude;
    }

    public double Latitude { get; }

    public double Longitude { get; }
}

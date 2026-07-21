namespace FraudAnalysis.Application.Validators;

// Valida coordenadas geográficas e calcula distância via Haversine.
public static class GeoLocationValidator
{
    public static bool IsValid(decimal latitude, decimal longitude)
    {
        return latitude is >= -90 and <= 90 &&
               longitude is >= -180 and <= 180;
    }

    // Retorna true se a coordenada for (0,0) — indica ausência de dado de GPS.
    public static bool IsNullIsland(decimal latitude, decimal longitude)
    {
        return latitude == 0 && longitude == 0;
    }

    // Calcula distância em km entre dois pontos usando fórmula de Haversine.
    public static double DistanceKm(
        decimal lat1, decimal lon1,
        decimal lat2, decimal lon2)
    {
        const double earthRadiusKm = 6371;

        var dLat = ToRadians((double)(lat2 - lat1));
        var dLon = ToRadians((double)(lon2 - lon1));

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians((double)lat1)) *
                Math.Cos(ToRadians((double)lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusKm * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}

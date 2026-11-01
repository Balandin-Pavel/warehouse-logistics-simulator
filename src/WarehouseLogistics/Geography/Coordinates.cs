namespace WarehouseLogistics.Geography;

public readonly record struct Coordinates
{
    private const double MinimumLatitude = -90;
    private const double MaximumLatitude = 90;
    private const double MinimumLongitude = -180;
    private const double MaximumLongitude = 180;
    private const double DegreesInHalfTurn = 180;
    private const double EarthRadiusMeters = 6_371_000;

    public Coordinates(double latitude, double longitude)
    {
        if (latitude < MinimumLatitude || latitude > MaximumLatitude || double.IsFinite(latitude) is false)
        {
            throw new ArgumentOutOfRangeException(nameof(latitude));
        }

        if (longitude < MinimumLongitude || longitude > MaximumLongitude || double.IsFinite(longitude) is false)
        {
            throw new ArgumentOutOfRangeException(nameof(longitude));
        }

        Latitude = latitude;
        Longitude = longitude;
    }

    public double Latitude { get; }

    public double Longitude { get; }

    public Distance CalculateDistanceTo(Coordinates destination)
    {
        double sourceLatitudeRadians = Latitude * (Math.PI / DegreesInHalfTurn);
        double destinationLatitudeRadians = destination.Latitude * (Math.PI / DegreesInHalfTurn);
        double latitudeDifferenceRadians = (destination.Latitude - Latitude) * (Math.PI / DegreesInHalfTurn);
        double longitudeDifferenceRadians = (destination.Longitude - Longitude) * (Math.PI / DegreesInHalfTurn);

        double latitudeSine = Math.Sin(latitudeDifferenceRadians / 2);
        double longitudeSine = Math.Sin(longitudeDifferenceRadians / 2);
        double haversine = (latitudeSine * latitudeSine) + (Math.Cos(sourceLatitudeRadians) * Math.Cos(destinationLatitudeRadians) * longitudeSine * longitudeSine);

        haversine = Math.Clamp(haversine, 0, 1);

        double centralAngle = 2 * Math.Atan2(Math.Sqrt(haversine), Math.Sqrt(1 - haversine));

        return new Distance(EarthRadiusMeters * centralAngle);
    }
}

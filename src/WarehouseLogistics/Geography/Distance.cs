namespace WarehouseLogistics.Geography;

public readonly record struct Distance
{
    public Distance(double meters)
    {
        if (meters < 0 || double.IsFinite(meters) is false)
        {
            throw new ArgumentOutOfRangeException(nameof(meters));
        }

        Meters = meters;
    }

    public double Meters { get; }
}

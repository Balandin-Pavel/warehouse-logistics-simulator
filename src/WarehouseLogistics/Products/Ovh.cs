namespace WarehouseLogistics.Products;

public readonly record struct Ovh
{
    public Ovh(double volume, double weight)
    {
        if (volume < 0 || double.IsFinite(volume) is false)
        {
            throw new ArgumentOutOfRangeException(nameof(volume));
        }

        if (weight < 0 || double.IsFinite(weight) is false)
        {
            throw new ArgumentOutOfRangeException(nameof(weight));
        }

        Volume = volume;
        Weight = weight;
    }

    public double Volume { get; }

    public double Weight { get; }

    public static Ovh operator +(Ovh left, Ovh right)
    {
        return new Ovh(left.Volume + right.Volume, left.Weight + right.Weight);
    }

    public static Ovh operator -(Ovh left, Ovh right)
    {
        return new Ovh(left.Volume - right.Volume, left.Weight - right.Weight);
    }

    public static Ovh operator *(Ovh ovh, Quantity multiplier)
    {
        return new Ovh(ovh.Volume * multiplier.Value, ovh.Weight * multiplier.Value);
    }

    public static bool operator <=(Ovh left, Ovh right)
    {
        return left.Volume <= right.Volume
            && left.Weight <= right.Weight;
    }

    public static bool operator >=(Ovh left, Ovh right)
    {
        return left.Volume >= right.Volume
            && left.Weight >= right.Weight;
    }
}

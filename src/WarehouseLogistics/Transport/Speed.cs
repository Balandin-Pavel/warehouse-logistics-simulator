namespace WarehouseLogistics.Transport;

public readonly record struct Speed
{
    public Speed(double value)
    {
        if (value <= 0 || double.IsFinite(value) is false)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }

        Value = value;
    }

    public double Value { get; }
}

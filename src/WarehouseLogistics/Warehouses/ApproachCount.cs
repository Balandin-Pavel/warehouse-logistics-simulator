namespace WarehouseLogistics.Warehouses;

public sealed record ApproachCount
{
    public ApproachCount(long value)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);

        Value = value;
    }

    public long Value { get; }
}

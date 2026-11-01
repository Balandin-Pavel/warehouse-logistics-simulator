namespace WarehouseLogistics.Products;

public readonly record struct SkuId
{
    public SkuId(int value)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);

        Value = value;
    }

    public int Value { get; }
}

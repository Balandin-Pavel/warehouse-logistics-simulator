namespace WarehouseLogistics.Warehouses;

public readonly record struct WarehouseId
{
    public WarehouseId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        Value = value;
    }

    public string Value { get; }
}

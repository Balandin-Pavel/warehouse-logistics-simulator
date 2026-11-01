namespace WarehouseLogistics.Products;

public readonly record struct Quantity
{
    public Quantity(int value)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);

        Value = value;
    }

    public int Value { get; }

    public static Quantity operator +(Quantity left, Quantity right)
    {
        return new Quantity(checked(left.Value + right.Value));
    }

    public static Quantity operator -(Quantity left, Quantity right)
    {
        return new Quantity(checked(left.Value - right.Value));
    }
}

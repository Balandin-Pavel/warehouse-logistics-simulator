namespace WarehouseLogistics.Time;

public readonly record struct Duration
{
    public Duration(TimeSpan value)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(value, TimeSpan.Zero);

        Value = value;
    }

    public TimeSpan Value { get; }

    public static Duration Zero { get; } = new Duration(TimeSpan.Zero);

    public static Duration operator +(Duration left, Duration right)
    {
        return new Duration(left.Value + right.Value);
    }

    public static Duration operator *(Duration duration, int multiplier)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(multiplier);

        return new Duration(duration.Value * multiplier);
    }
}

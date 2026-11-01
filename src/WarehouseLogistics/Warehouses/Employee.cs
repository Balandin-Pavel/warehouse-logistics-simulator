using WarehouseLogistics.Products;
using WarehouseLogistics.Time;

namespace WarehouseLogistics.Warehouses;

public sealed class Employee
{
    public Employee(Ovh maxCarryOvh, Duration deliveryTime)
    {
        if (maxCarryOvh.Volume <= 0 || maxCarryOvh.Weight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCarryOvh));
        }

        if (deliveryTime.Value <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(deliveryTime));
        }

        DeliveryTime = deliveryTime;
        MaxCarryOvh = maxCarryOvh;
    }

    public Ovh MaxCarryOvh { get; }

    public Duration DeliveryTime { get; }

    public bool CanCarry(Ovh ovh)
    {
        return ovh <= MaxCarryOvh;
    }
}

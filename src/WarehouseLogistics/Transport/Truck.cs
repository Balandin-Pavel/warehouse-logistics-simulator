using WarehouseLogistics.Geography;
using WarehouseLogistics.Products;
using WarehouseLogistics.Products.Results;
using WarehouseLogistics.Time;

namespace WarehouseLogistics.Transport;

public sealed class Truck
{
    public Truck(Ovh capacity, Speed speed, Coordinates startPoint)
    {
        CargoStorage = new ProductStorage(capacity);
        Speed = speed;
        Position = startPoint;
    }

    public Ovh Capacity => CargoStorage.Capacity;

    public Speed Speed { get; }

    public Coordinates Position { get; private set; }

    public Ovh LoadedOvh => CargoStorage.StoredOvh;

    public IReadOnlyCollection<ProductQuantity> Cargo => CargoStorage.Items;

    internal ProductStorage CargoStorage { get; }

    public Duration MoveTo(Coordinates destination)
    {
        Distance distance = Position.CalculateDistanceTo(destination);
        var travelTime = new Duration(TimeSpan.FromSeconds(distance.Meters / Speed.Value));
        Position = destination;

        return travelTime;
    }

    public ProductStorageAddResult Load(ProductBatch load)
    {
        return CargoStorage.Add(load);
    }

    public ProductStorageRemoveResult Unload(ProductBatch load)
    {
        return CargoStorage.Remove(load);
    }
}

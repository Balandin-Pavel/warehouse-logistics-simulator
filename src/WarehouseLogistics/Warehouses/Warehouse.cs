using WarehouseLogistics.Geography;
using WarehouseLogistics.Products;
using WarehouseLogistics.Products.Results;
using WarehouseLogistics.Warehouses.Results;

namespace WarehouseLogistics.Warehouses;

public sealed class Warehouse
{
    private readonly WarehouseStaff _staff;
    private readonly ProductStorage _stockStorage;

    public Warehouse(WarehouseId id, Ovh capacity, Coordinates position, IReadOnlyCollection<Employee> staff)
    {
        _stockStorage = new ProductStorage(capacity);
        _staff = new WarehouseStaff(staff);
        Id = id;
        Position = position;
    }

    public WarehouseId Id { get; }

    public Ovh Capacity => _stockStorage.Capacity;

    public Coordinates Position { get; }

    public Ovh StoredOvh => _stockStorage.StoredOvh;

    public IReadOnlyCollection<ProductQuantity> Stock => _stockStorage.Items;

    public IReadOnlyList<Employee> Staff => _staff.Employees;

    public WarehouseTransferResult Receive(ProductStorage source, ProductBatch batch)
    {
        WarehouseTransferResult calculationResult = _staff.CalculateReceivingTime(batch);

        return Transfer(source, _stockStorage, batch, calculationResult);
    }

    public WarehouseTransferResult Ship(ProductStorage destination, ProductBatch batch)
    {
        WarehouseTransferResult calculationResult = _staff.CalculateShippingTime(batch);

        return Transfer(_stockStorage, destination, batch, calculationResult);
    }

    private static WarehouseTransferResult Transfer(
        ProductStorage source,
        ProductStorage destination,
        ProductBatch batch,
        WarehouseTransferResult calculationResult)
    {
        if (calculationResult is WarehouseTransferResult.UncarryableProduct)
        {
            return calculationResult;
        }

        if (calculationResult is not WarehouseTransferResult.Success success)
        {
            throw new InvalidOperationException("Warehouse staff returned an unknown transfer result");
        }

        ProductStorageTransferResult storageResult = source.TransferTo(destination, batch);

        if (storageResult is ProductStorageTransferResult.RemoveFailure removeFailure)
        {
            return new WarehouseTransferResult.SourceFailure(removeFailure.Reason);
        }

        if (storageResult is ProductStorageTransferResult.AddFailure addFailure)
        {
            return new WarehouseTransferResult.DestinationFailure(addFailure.Reason);
        }

        if (storageResult is not ProductStorageTransferResult.Success)
        {
            throw new InvalidOperationException("Product storage returned an unknown transfer result");
        }

        return success;
    }
}

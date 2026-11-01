using WarehouseLogistics.Products;
using WarehouseLogistics.Products.Results;
using WarehouseLogistics.Time;

namespace WarehouseLogistics.Warehouses.Results;

public abstract record WarehouseTransferResult
{
    public sealed record Success(Duration Duration, ApproachCount ApproachCount) : WarehouseTransferResult;

    public sealed record SourceFailure(ProductStorageRemoveResult.Failure Reason) : WarehouseTransferResult;

    public sealed record DestinationFailure(ProductStorageAddResult.Failure Reason) : WarehouseTransferResult;

    public sealed record UncarryableProduct(
        Sku Sku,
        IReadOnlyCollection<Ovh> EmployeeCapacities) : WarehouseTransferResult;
}

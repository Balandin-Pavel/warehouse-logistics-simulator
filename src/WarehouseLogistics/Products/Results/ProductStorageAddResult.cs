namespace WarehouseLogistics.Products.Results;

public abstract record ProductStorageAddResult
{
    public sealed record Success : ProductStorageAddResult;

    public abstract record Failure : ProductStorageAddResult;

    public sealed record InsufficientCapacity(Ovh Capacity, Ovh StoredOvh, Ovh RequestedOvh) : Failure;

    public sealed record QuantityOverflow(Sku Sku, Quantity StoredQuantity, Quantity AddedQuantity) : Failure;

    public sealed record ConflictingSkuDefinition(Sku ExistingSku, Sku IncomingSku) : Failure;
}

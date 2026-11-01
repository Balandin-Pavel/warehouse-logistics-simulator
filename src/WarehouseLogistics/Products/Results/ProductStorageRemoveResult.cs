namespace WarehouseLogistics.Products.Results;

public abstract record ProductStorageRemoveResult
{
    public sealed record Success : ProductStorageRemoveResult;

    public abstract record Failure : ProductStorageRemoveResult;

    public sealed record InsufficientQuantity(
        Sku Sku,
        Quantity? AvailableQuantity,
        Quantity RequestedQuantity) : Failure;

    public sealed record ConflictingSkuDefinition(Sku ExistingSku, Sku RequestedSku) : Failure;
}

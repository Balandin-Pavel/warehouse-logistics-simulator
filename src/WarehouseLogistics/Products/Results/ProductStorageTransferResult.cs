namespace WarehouseLogistics.Products.Results;

public abstract record ProductStorageTransferResult
{
    public sealed record Success : ProductStorageTransferResult;

    public sealed record RemoveFailure(ProductStorageRemoveResult.Failure Reason) : ProductStorageTransferResult;

    public sealed record AddFailure(ProductStorageAddResult.Failure Reason) : ProductStorageTransferResult;
}

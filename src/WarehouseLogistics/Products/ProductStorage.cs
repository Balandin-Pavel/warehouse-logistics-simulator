using WarehouseLogistics.Products.Results;

namespace WarehouseLogistics.Products;

public sealed class ProductStorage
{
    private readonly List<ProductQuantity> _items;

    public ProductStorage(Ovh capacity)
    {
        if (capacity.Volume <= 0 || capacity.Weight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }

        _items = new List<ProductQuantity>();
        Capacity = capacity;
        StoredOvh = new Ovh(0, 0);
    }

    public Ovh Capacity { get; }

    public Ovh StoredOvh { get; private set; }

    public IReadOnlyCollection<ProductQuantity> Items => _items.ToList();

    public ProductStorageAddResult Add(ProductBatch batch)
    {
        ProductStorageAddResult validationResult = ValidateAdd(batch);

        if (validationResult is ProductStorageAddResult.Failure failure)
        {
            return failure;
        }

        if (validationResult is not ProductStorageAddResult.Success)
        {
            throw new InvalidOperationException("Product storage returned an unknown add validation result");
        }

        AddUnchecked(batch);

        return validationResult;
    }

    public ProductStorageRemoveResult Remove(ProductBatch batch)
    {
        ProductStorageRemoveResult validationResult = ValidateRemove(batch);

        if (validationResult is ProductStorageRemoveResult.Failure failure)
        {
            return failure;
        }

        if (validationResult is not ProductStorageRemoveResult.Success)
        {
            throw new InvalidOperationException("Product storage returned an unknown remove validation result");
        }

        RemoveUnchecked(batch);

        return validationResult;
    }

    public ProductStorageTransferResult TransferTo(ProductStorage destination, ProductBatch batch)
    {
        ProductStorageRemoveResult removeValidationResult = ValidateRemove(batch);

        if (removeValidationResult is ProductStorageRemoveResult.Failure removeFailure)
        {
            return new ProductStorageTransferResult.RemoveFailure(
                removeFailure);
        }

        if (removeValidationResult is not ProductStorageRemoveResult.Success)
        {
            throw new InvalidOperationException("Product storage returned an unknown remove validation result");
        }

        ProductStorageAddResult addValidationResult = destination.ValidateAdd(batch);

        if (addValidationResult is ProductStorageAddResult.Failure addFailure)
        {
            return new ProductStorageTransferResult.AddFailure(
                addFailure);
        }

        if (addValidationResult is not ProductStorageAddResult.Success)
        {
            throw new InvalidOperationException("Product storage returned an unknown add validation result");
        }

        RemoveUnchecked(batch);
        destination.AddUnchecked(batch);

        return new ProductStorageTransferResult.Success();
    }

    private static bool HasConflictingSku(
        IReadOnlyCollection<ProductQuantity> storedItems,
        ProductQuantity item)
    {
        ProductQuantity? storedItem = storedItems.FirstOrDefault(
            storedProduct => storedProduct.Sku.Id == item.Sku.Id);

        return storedItem is not null
            && storedItem.Sku != item.Sku;
    }

    private static bool QuantityWillOverflow(
        IReadOnlyCollection<ProductQuantity> storedItems,
        ProductQuantity item)
    {
        ProductQuantity? storedItem = storedItems.FirstOrDefault(
            storedProduct => storedProduct.Sku.Id == item.Sku.Id);

        return storedItem is not null
            && storedItem.Quantity.Value
            > int.MaxValue - item.Quantity.Value;
    }

    private ProductStorageAddResult ValidateAdd(ProductBatch batch)
    {
        Ovh updatedOvh = StoredOvh + batch.TotalOvh;

        if (updatedOvh <= Capacity is false)
        {
            return new ProductStorageAddResult.InsufficientCapacity(Capacity, StoredOvh, batch.TotalOvh);
        }

        ProductQuantity? conflictingProduct = batch.Items.FirstOrDefault(item => HasConflictingSku(_items, item));

        if (conflictingProduct is not null)
        {
            ProductQuantity existingProduct = _items.FirstOrDefault(storedProduct => storedProduct.Sku.Id == conflictingProduct.Sku.Id) ?? throw new InvalidOperationException("Product storage invariant was violated");

            return new ProductStorageAddResult.ConflictingSkuDefinition(existingProduct.Sku, conflictingProduct.Sku);
        }

        ProductQuantity? overflowingProduct = batch.Items.FirstOrDefault(item => QuantityWillOverflow(_items, item));

        if (overflowingProduct is not null)
        {
            ProductQuantity existingProduct = _items.FirstOrDefault(storedProduct => storedProduct.Sku.Id == overflowingProduct.Sku.Id) ?? throw new InvalidOperationException("Product storage invariant was violated");

            return new ProductStorageAddResult.QuantityOverflow(existingProduct.Sku, existingProduct.Quantity, overflowingProduct.Quantity);
        }

        return new ProductStorageAddResult.Success();
    }

    private ProductStorageRemoveResult ValidateRemove(ProductBatch batch)
    {
        foreach (ProductQuantity item in batch.Items)
        {
            ProductQuantity? storedItem = _items.FirstOrDefault(storedProduct => storedProduct.Sku.Id == item.Sku.Id);

            if (storedItem is null)
            {
                return new ProductStorageRemoveResult.InsufficientQuantity(item.Sku, null, item.Quantity);
            }

            if (storedItem.Sku != item.Sku)
            {
                return new ProductStorageRemoveResult.ConflictingSkuDefinition(storedItem.Sku, item.Sku);
            }

            if (storedItem.Quantity.Value < item.Quantity.Value)
            {
                return new ProductStorageRemoveResult.InsufficientQuantity(item.Sku, storedItem.Quantity, item.Quantity);
            }
        }

        return new ProductStorageRemoveResult.Success();
    }

    private void AddUnchecked(ProductBatch batch)
    {
        foreach (ProductQuantity item in batch.Items)
        {
            ProductQuantity? storedItem = _items.FirstOrDefault(storedProduct => storedProduct.Sku.Id == item.Sku.Id);

            if (storedItem is null)
            {
                _items.Add(item);
                continue;
            }

            Quantity updatedQuantity = storedItem.Quantity + item.Quantity;
            var updatedItem = new ProductQuantity(item.Sku, updatedQuantity);

            _items.Remove(storedItem);
            _items.Add(updatedItem);
        }

        StoredOvh = StoredOvh + batch.TotalOvh;
    }

    private void RemoveUnchecked(ProductBatch batch)
    {
        foreach (ProductQuantity item in batch.Items)
        {
            ProductQuantity storedItem = _items.FirstOrDefault(
                storedProduct => storedProduct.Sku.Id == item.Sku.Id) ?? throw new InvalidOperationException("Product storage invariant was violated");

            _items.Remove(storedItem);

            if (storedItem.Quantity.Value > item.Quantity.Value)
            {
                _items.Add(new ProductQuantity(item.Sku, storedItem.Quantity - item.Quantity));
            }
        }

        StoredOvh = StoredOvh - batch.TotalOvh;
    }
}

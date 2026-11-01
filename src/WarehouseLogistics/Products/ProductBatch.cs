namespace WarehouseLogistics.Products;

public sealed class ProductBatch
{
    private readonly IReadOnlyCollection<ProductQuantity> _items;

    public ProductBatch(IReadOnlyCollection<ProductQuantity> items)
    {
        if (items.Count == 0)
        {
            throw new ArgumentException("Product batch must not be empty", nameof(items));
        }

        var itemsCopy = items.ToList();

        if (itemsCopy.Select(item => item.Sku.Id).Distinct().Count() != itemsCopy.Count)
        {
            throw new ArgumentException("Product batch must not contain duplicate SKU identifiers", nameof(items));
        }

        _items = itemsCopy;
        TotalOvh = _items.Aggregate(new Ovh(0, 0), AddProductOvh);
    }

    public IReadOnlyCollection<ProductQuantity> Items => _items.ToList();

    public Ovh TotalOvh { get; }

    private static Ovh AddProductOvh(Ovh totalOvh, ProductQuantity item)
    {
        return totalOvh + item.TotalOvh;
    }
}

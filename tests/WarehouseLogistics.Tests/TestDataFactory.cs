using WarehouseLogistics.Products;
using Xunit;

namespace WarehouseLogistics.Tests;

public static class TestDataFactory
{
    public static ProductBatch CreateProductBatch(Sku sku, int quantity)
    {
        var items = new List<ProductQuantity>
        {
            new ProductQuantity(sku, new Quantity(quantity)),
        };

        return new ProductBatch(items);
    }

    public static int GetProductQuantity(IReadOnlyCollection<ProductQuantity> products, Sku sku)
    {
        bool HasMatchingSku(ProductQuantity item)
        {
            return item.Sku.Equals(sku);
        }

        ProductQuantity product = Assert.Single(products, HasMatchingSku);

        return product.Quantity.Value;
    }
}

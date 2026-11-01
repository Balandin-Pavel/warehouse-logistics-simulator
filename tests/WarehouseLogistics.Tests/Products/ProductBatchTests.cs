using WarehouseLogistics.Products;
using Xunit;

namespace WarehouseLogistics.Tests.Products;

public class ProductBatchTests
{
    [Fact]
    public void ConstructorCalculatesTotalOvh()
    {
        // Arrange
        var firstSku = new Sku(new SkuId(1), "Phone", new Ovh(10, 4));
        var secondSku = new Sku(new SkuId(2), "TV", new Ovh(3, 2));
        var items = new List<ProductQuantity>
        {
            new ProductQuantity(firstSku, new Quantity(2)),
            new ProductQuantity(secondSku, new Quantity(5)),
        };

        // Act
        var batch = new ProductBatch(items);

        // Assert
        Assert.Equal(new Ovh(35, 18), batch.TotalOvh);
    }

    [Fact]
    public void ConstructorWithEmptyCollectionThrows()
    {
        // Arrange
        var items = new List<ProductQuantity>();

        // Act
        Action act = () =>
        {
            new ProductBatch(items);
        };

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void ConstructorCopiesItemsCollection()
    {
        // Arrange
        var firstSku = new Sku(new SkuId(1), "Phone", new Ovh(10, 10));
        var secondSku = new Sku(new SkuId(2), "TV", new Ovh(3, 3));
        var items = new List<ProductQuantity>
        {
            new ProductQuantity(firstSku, new Quantity(2)),
        };
        var batch = new ProductBatch(items);
        bool HasSecondSku(ProductQuantity item)
        {
            return item.Sku.Equals(secondSku);
        }

        // Act
        items.Add(new ProductQuantity(secondSku, new Quantity(5)));

        // Assert
        Assert.Equal(new Ovh(20, 20), batch.TotalOvh);
        Assert.Single(batch.Items);
        Assert.DoesNotContain(batch.Items, HasSecondSku);
    }

    [Fact]
    public void ItemsReturnsDefensiveCopy()
    {
        // Arrange
        var firstSku = new Sku(new SkuId(1), "Phone", new Ovh(10, 10));
        var secondSku = new Sku(new SkuId(2), "TV", new Ovh(3, 3));
        var items = new List<ProductQuantity>
        {
            new ProductQuantity(firstSku, new Quantity(2)),
        };
        var batch = new ProductBatch(items);

        // Act
        List<ProductQuantity> returnedItems = Assert.IsType<List<ProductQuantity>>(batch.Items);
        returnedItems[0] = new ProductQuantity(secondSku, new Quantity(5));

        // Assert
        ProductQuantity storedItem = Assert.Single(batch.Items);
        Assert.Equal(firstSku, storedItem.Sku);
        Assert.Equal(new Ovh(20, 20), batch.TotalOvh);
    }

    [Fact]
    public void ConstructorWithDuplicateSkuThrows()
    {
        // Arrange
        var sku = new Sku(new SkuId(1), "Phone", new Ovh(10, 10));
        var items = new List<ProductQuantity>
        {
            new ProductQuantity(sku, new Quantity(2)),
            new ProductQuantity(sku, new Quantity(3)),
        };

        // Act
        Action act = () =>
        {
            new ProductBatch(items);
        };

        // Assert
        Assert.Throws<ArgumentException>(act);
    }
}

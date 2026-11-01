using WarehouseLogistics.Products;
using Xunit;

namespace WarehouseLogistics.Tests.Products;

public class ProductQuantityTests
{
    [Fact]
    public void ConstructorCalculatesTotalOvh()
    {
        // Arrange
        var sku = new Sku(new SkuId(1), "Phone", new Ovh(10, 10));

        // Act
        var productQuantity = new ProductQuantity(sku, new Quantity(3));

        // Assert
        Assert.Equal(new Ovh(30, 30), productQuantity.TotalOvh);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ConstructorWithNonPositiveQuantityThrows(int quantity)
    {
        // Arrange
        var sku = new Sku(new SkuId(1), "Phone", new Ovh(10, 10));

        // Act
        Action act = () =>
        {
            new ProductQuantity(sku, new Quantity(quantity));
        };

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }
}

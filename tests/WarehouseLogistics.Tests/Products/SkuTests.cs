using WarehouseLogistics.Products;
using Xunit;

namespace WarehouseLogistics.Tests.Products;

public class SkuTests
{
    [Fact]
    public void ConstructorWithValidArgumentsCreatesSku()
    {
        // Arrange

        // Act
        var sku = new Sku(new SkuId(1), "TV", new Ovh(10, 10));

        // Assert
        Assert.Equal(new SkuId(1), sku.Id);
        Assert.Equal("TV", sku.Name);
        Assert.Equal(new Ovh(10, 10), sku.Ovh);
    }

    [Fact]
    public void ConstructorWithZeroOvhThrows()
    {
        // Arrange

        // Act
        Action act = static () =>
        {
            new Sku(new SkuId(1), "TV", new Ovh(0, 0));
        };

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    public void ConstructorWithZeroOvhComponentThrows(double volume, double weight)
    {
        // Arrange

        // Act
        Action act = () =>
        {
            new Sku(new SkuId(1), "TV", new Ovh(volume, weight));
        };

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }
}

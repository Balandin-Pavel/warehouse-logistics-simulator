using WarehouseLogistics.Products;
using Xunit;

namespace WarehouseLogistics.Tests.Products;

public class SkuIdTests
{
    [Fact]
    public void ConstructorWithValidValueCreatesSkuId()
    {
        // Arrange

        // Act
        var id = new SkuId(1);

        // Assert
        Assert.Equal(1, id.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ConstructorWithInvalidValueThrows(int value)
    {
        // Arrange

        // Act
        Action act = () =>
        {
            new SkuId(value);
        };

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }
}

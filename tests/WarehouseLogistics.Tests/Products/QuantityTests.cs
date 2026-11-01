using WarehouseLogistics.Products;
using Xunit;

namespace WarehouseLogistics.Tests.Products;

public class QuantityTests
{
    [Fact]
    public void ConstructorWithPositiveValueCreatesQuantity()
    {
        // Arrange

        // Act
        var quantity = new Quantity(3);

        // Assert
        Assert.Equal(3, quantity.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ConstructorWithNonPositiveValueThrows(int value)
    {
        // Arrange

        // Act
        Action act = () =>
        {
            new Quantity(value);
        };

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void AddReturnsCombinedQuantity()
    {
        // Arrange
        var first = new Quantity(2);
        var second = new Quantity(3);

        // Act
        Quantity result = first + second;

        // Assert
        Assert.Equal(new Quantity(5), result);
    }

    [Fact]
    public void SubtractReturnsDifference()
    {
        // Arrange
        var first = new Quantity(5);
        var second = new Quantity(3);

        // Act
        Quantity result = first - second;

        // Assert
        Assert.Equal(new Quantity(2), result);
    }
}

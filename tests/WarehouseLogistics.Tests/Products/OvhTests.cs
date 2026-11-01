using WarehouseLogistics.Products;
using Xunit;

namespace WarehouseLogistics.Tests.Products;

public class OvhTests
{
    [Fact]
    public void ConstructorWithValidValuesCreatesOvh()
    {
        // Arrange

        // Act
        var ovh = new Ovh(10.5, 7.5);

        // Assert
        Assert.Equal(10.5, ovh.Volume);
        Assert.Equal(7.5, ovh.Weight);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void ConstructorWithInvalidVolumeThrows(double volume)
    {
        // Arrange

        // Act
        Action act = () =>
        {
            new Ovh(volume, 1);
        };

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void ConstructorWithInvalidWeightThrows(double weight)
    {
        // Arrange

        // Act
        Action act = () =>
        {
            new Ovh(1, weight);
        };

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void AddReturnsComponentWiseSum()
    {
        // Arrange
        var first = new Ovh(10, 4);
        var second = new Ovh(5, 3);

        // Act
        Ovh result = first + second;

        // Assert
        Assert.Equal(new Ovh(15, 7), result);
    }

    [Fact]
    public void SubtractReturnsComponentWiseDifference()
    {
        // Arrange
        var first = new Ovh(10, 8);
        var second = new Ovh(4, 3);

        // Act
        Ovh result = first - second;

        // Assert
        Assert.Equal(new Ovh(6, 5), result);
    }

    [Theory]
    [InlineData(10, 1)]
    [InlineData(1, 10)]
    public void SubtractLargerComponentThrows(double volume, double weight)
    {
        // Arrange
        var first = new Ovh(5, 5);
        var second = new Ovh(volume, weight);

        // Act
        Action act = () =>
        {
            _ = first - second;
        };

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void MultiplyReturnsComponentWiseProduct()
    {
        // Arrange
        var ovh = new Ovh(7.5, 2.5);

        // Act
        Ovh result = ovh * new Quantity(4);

        // Assert
        Assert.Equal(new Ovh(30, 10), result);
    }

    [Fact]
    public void FitsWithinReturnsTrueWhenBothComponentsFit()
    {
        // Arrange
        var ovh = new Ovh(5, 10);

        // Act
        bool result = ovh <= new Ovh(5, 11);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(6, 10)]
    [InlineData(5, 12)]
    public void FitsWithinReturnsFalseWhenAnyComponentExceedsLimit(double volume, double weight)
    {
        // Arrange
        var ovh = new Ovh(volume, weight);

        // Act
        bool result = ovh <= new Ovh(5, 11);

        // Assert
        Assert.False(result);
    }
}

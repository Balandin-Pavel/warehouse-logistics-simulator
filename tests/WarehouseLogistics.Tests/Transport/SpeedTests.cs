using WarehouseLogistics.Transport;
using Xunit;

namespace WarehouseLogistics.Tests.Transport;

public class SpeedTests
{
    [Fact]
    public void ConstructorWithPositiveFiniteValueCreatesSpeed()
    {
        // Arrange

        // Act
        var speed = new Speed(10);

        // Assert
        Assert.Equal(10, speed.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void ConstructorWithInvalidValueThrows(double value)
    {
        // Arrange

        // Act
        Action act = () =>
        {
            new Speed(value);
        };

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }
}

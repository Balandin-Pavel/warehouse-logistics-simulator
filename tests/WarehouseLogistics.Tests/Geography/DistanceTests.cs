using WarehouseLogistics.Geography;
using Xunit;

namespace WarehouseLogistics.Tests.Geography;

public class DistanceTests
{
    [Fact]
    public void ConstructorWithPositiveMetersCreatesDistance()
    {
        // Arrange
        const double meters = 10;

        // Act
        var distance = new Distance(meters);

        // Assert
        Assert.Equal(meters, distance.Meters);
    }

    [Fact]
    public void ConstructorWithZeroMetersCreatesDistance()
    {
        // Arrange
        const double meters = 0;

        // Act
        var distance = new Distance(meters);

        // Assert
        Assert.Equal(meters, distance.Meters);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void ConstructorWithInvalidMetersThrows(double meters)
    {
        // Arrange

        // Act
        Action act = () =>
        {
            new Distance(meters);
        };

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }
}

using WarehouseLogistics.Geography;
using Xunit;

namespace WarehouseLogistics.Tests.Geography;

public class CoordinatesTests
{
    [Fact]
    public void ConstructorWithValidArgumentsCreatesCoordinates()
    {
        // Arrange

        // Act
        var coordinates = new Coordinates(44.555, 55.444);

        // Assert
        Assert.Equal(44.555, coordinates.Latitude);
        Assert.Equal(55.444, coordinates.Longitude);
    }

    [Theory]
    [InlineData(91, 0)]
    [InlineData(-91, 0)]
    [InlineData(0, 181)]
    [InlineData(0, -181)]
    public void ConstructorWithInvalidCoordinatesThrows(double latitude, double longitude)
    {
        // Arrange

        // Act
        Action act = () =>
        {
            new Coordinates(latitude, longitude);
        };

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void CalculateDistanceToReturnsExpectedDistance()
    {
        // Arrange
        var origin = new Coordinates(0, 0);
        var destination = new Coordinates(0, 1);

        // Act
        Distance distance = origin.CalculateDistanceTo(destination);

        // Assert
        Assert.InRange(distance.Meters, 111000, 111300);
    }
}

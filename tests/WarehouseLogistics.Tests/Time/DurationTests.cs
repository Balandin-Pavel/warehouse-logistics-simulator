using WarehouseLogistics.Time;
using Xunit;

namespace WarehouseLogistics.Tests.Time;

public class DurationTests
{
    [Fact]
    public void ConstructorWithPositiveValueCreatesDuration()
    {
        // Arrange
        var value = TimeSpan.FromMinutes(5);

        // Act
        var duration = new Duration(value);

        // Assert
        Assert.Equal(value, duration.Value);
    }

    [Fact]
    public void ZeroReturnsZeroDuration()
    {
        // Arrange

        // Act
        Duration duration = Duration.Zero;

        // Assert
        Assert.Equal(TimeSpan.Zero, duration.Value);
    }

    [Fact]
    public void ConstructorWithNegativeValueThrows()
    {
        // Arrange
        var value = TimeSpan.FromTicks(-1);

        // Act
        Action act = () =>
        {
            new Duration(value);
        };

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void AddReturnsSum()
    {
        // Arrange
        var first = new Duration(TimeSpan.FromMinutes(2));
        var second = new Duration(TimeSpan.FromMinutes(3));

        // Act
        Duration result = first + second;

        // Assert
        Assert.Equal(TimeSpan.FromMinutes(5), result.Value);
    }

    [Fact]
    public void MultiplyReturnsProduct()
    {
        // Arrange
        var duration = new Duration(TimeSpan.FromMinutes(2));

        // Act
        Duration result = duration * 3;

        // Assert
        Assert.Equal(TimeSpan.FromMinutes(6), result.Value);
    }

    [Fact]
    public void MultiplyByNegativeValueThrows()
    {
        // Arrange
        var duration = new Duration(TimeSpan.FromMinutes(2));

        // Act
        Action act = () =>
        {
            _ = duration * -1;
        };

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }
}

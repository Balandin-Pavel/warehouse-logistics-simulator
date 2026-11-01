using WarehouseLogistics.Warehouses;
using Xunit;

namespace WarehouseLogistics.Tests.Warehouses;

public class ApproachCountTests
{
    [Fact]
    public void ConstructorWithPositiveValueCreatesApproachCount()
    {
        // Arrange

        // Act
        var approachCount = new ApproachCount(3);

        // Assert
        Assert.Equal(3, approachCount.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ConstructorWithNonPositiveValueThrows(long value)
    {
        // Arrange

        // Act
        Action act = () =>
        {
            new ApproachCount(value);
        };

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }
}

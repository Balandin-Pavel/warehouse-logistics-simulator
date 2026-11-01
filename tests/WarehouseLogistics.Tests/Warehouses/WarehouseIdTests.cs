using WarehouseLogistics.Warehouses;
using Xunit;

namespace WarehouseLogistics.Tests.Warehouses;

public class WarehouseIdTests
{
    [Fact]
    public void ConstructorWithValidValueCreatesWarehouseId()
    {
        // Arrange

        // Act
        var id = new WarehouseId("warehouse-1");

        // Assert
        Assert.Equal("warehouse-1", id.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ConstructorWithInvalidValueThrows(string value)
    {
        // Arrange

        // Act
        Action act = () =>
        {
            new WarehouseId(value);
        };

        // Assert
        Assert.Throws<ArgumentException>(act);
    }
}

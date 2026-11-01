using WarehouseLogistics.Products;
using WarehouseLogistics.Time;
using WarehouseLogistics.Warehouses;
using Xunit;

namespace WarehouseLogistics.Tests.Warehouses;

public class EmployeeTests
{
    [Fact]
    public void ConstructorWithValidArgumentsCreatesEmployee()
    {
        // Arrange
        var deliveryTime = TimeSpan.FromMinutes(5);

        // Act
        var employee = new Employee(new Ovh(20, 20), new Duration(deliveryTime));

        // Assert
        Assert.Equal(new Ovh(20, 20), employee.MaxCarryOvh);
        Assert.Equal(deliveryTime, employee.DeliveryTime.Value);
    }

    [Fact]
    public void ConstructorWithZeroCapacityThrows()
    {
        // Arrange

        // Act
        Action act = static () =>
        {
            new Employee(new Ovh(0, 0), new Duration(TimeSpan.FromMinutes(5)));
        };

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ConstructorWithNonPositiveDeliveryTimeThrows(long deliveryTimeTicks)
    {
        // Arrange
        var deliveryTime = TimeSpan.FromTicks(deliveryTimeTicks);

        // Act
        Action act = () =>
        {
            new Employee(new Ovh(20, 20), new Duration(deliveryTime));
        };

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    public void ConstructorWithZeroCapacityComponentThrows(double volume, double weight)
    {
        // Arrange

        // Act
        Action act = () =>
        {
            new Employee(new Ovh(volume, weight), new Duration(TimeSpan.FromMinutes(5)));
        };

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }
}

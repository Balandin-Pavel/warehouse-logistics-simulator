using WarehouseLogistics.Geography;
using WarehouseLogistics.Products;
using WarehouseLogistics.Routing.Results;
using WarehouseLogistics.Routing.Stages.Movement;
using WarehouseLogistics.Transport;
using Xunit;

namespace WarehouseLogistics.Tests.Routing.Stages.Movement;

public class MovementStageTests
{
    [Fact]
    public void ExecuteMovesTruckAndReturnsTravelTime()
    {
        // Arrange
        var destination = new Coordinates(0, 1);
        var truck = new Truck(new Ovh(100, 100), new Speed(10), new Coordinates(0, 0));
        var stage = new MovementStage(destination);

        // Act
        RouteStageExecutionResult result = stage.Execute(truck);

        // Assert
        RouteStageExecutionResult.Success success = Assert.IsType<RouteStageExecutionResult.Success>(result);
        Assert.Equal(destination, truck.Position);
        Assert.InRange(success.Duration.Value.TotalSeconds, 11_100, 11_130);
    }
}

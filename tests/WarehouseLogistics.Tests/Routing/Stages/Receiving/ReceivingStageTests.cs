using WarehouseLogistics.Geography;
using WarehouseLogistics.Products;
using WarehouseLogistics.Products.Results;
using WarehouseLogistics.Routing.Results;
using WarehouseLogistics.Routing.Stages.Receiving;
using WarehouseLogistics.Routing.Stages.Receiving.Errors;
using WarehouseLogistics.Time;
using WarehouseLogistics.Transport;
using WarehouseLogistics.Warehouses;
using WarehouseLogistics.Warehouses.Results;
using Xunit;

namespace WarehouseLogistics.Tests.Routing.Stages.Receiving;

public class ReceivingStageTests
{
    [Fact]
    public void ExecuteWithValidStateTransfersProductsAndReturnsDuration()
    {
        // Arrange
        ProductBatch manifest = CreateManifest(new Ovh(5, 5), 5);
        var truck = new Truck(new Ovh(100, 100), new Speed(10), new Coordinates(0, 0));
        Warehouse warehouse = CreateWarehouse(new Ovh(100, 100), new Coordinates(0, 0));
        truck.Load(manifest);
        var stage = new ReceivingStage(warehouse, manifest);

        // Act
        RouteStageExecutionResult result = stage.Execute(truck);

        // Assert
        RouteStageExecutionResult.Success success = Assert.IsType<RouteStageExecutionResult.Success>(result);
        ProductQuantity storedProduct = Assert.Single(warehouse.Stock);
        Assert.Equal(5, storedProduct.Quantity.Value);
        Assert.Empty(truck.Cargo);
        Assert.Equal(TimeSpan.FromMinutes(5), success.Duration.Value);
    }

    [Fact]
    public void ExecuteWhenTruckIsTooFarReturnsFailureWithoutChangingState()
    {
        // Arrange
        ProductBatch manifest = CreateManifest(new Ovh(5, 5), 1);
        var truck = new Truck(new Ovh(100, 100), new Speed(10), new Coordinates(0, 1));
        Warehouse warehouse = CreateWarehouse(new Ovh(100, 100), new Coordinates(0, 0));
        truck.Load(manifest);
        var stage = new ReceivingStage(warehouse, manifest);

        // Act
        RouteStageExecutionResult result = stage.Execute(truck);

        // Assert
        RouteStageExecutionResult.Failure failure = Assert.IsType<RouteStageExecutionResult.Failure>(result);
        Assert.IsType<TruckTooFarFromWarehouseError>(failure.Error);
        Assert.Single(truck.Cargo);
        Assert.Empty(warehouse.Stock);
    }

    [Fact]
    public void ExecuteWithMissingTruckCargoReturnsFailureWithoutChangingState()
    {
        // Arrange
        ProductBatch manifest = CreateManifest(new Ovh(5, 5), 1);
        var truck = new Truck(new Ovh(100, 100), new Speed(10), new Coordinates(0, 0));
        Warehouse warehouse = CreateWarehouse(new Ovh(100, 100), new Coordinates(0, 0));
        var stage = new ReceivingStage(warehouse, manifest);

        // Act
        RouteStageExecutionResult result = stage.Execute(truck);

        // Assert
        RouteStageExecutionResult.Failure failure = Assert.IsType<RouteStageExecutionResult.Failure>(result);
        TruckUnloadFailedError error = Assert.IsType<TruckUnloadFailedError>(failure.Error);
        Assert.IsType<ProductStorageRemoveResult.InsufficientQuantity>(error.Reason);
        Assert.Empty(truck.Cargo);
        Assert.Empty(warehouse.Stock);
    }

    [Fact]
    public void ExecuteWithoutWarehouseCapacityReturnsFailureWithoutChangingState()
    {
        // Arrange
        ProductBatch manifest = CreateManifest(new Ovh(10, 10), 6);
        var truck = new Truck(new Ovh(100, 100), new Speed(10), new Coordinates(0, 0));
        Warehouse warehouse = CreateWarehouse(new Ovh(50, 50), new Coordinates(0, 0));
        truck.Load(manifest);
        var stage = new ReceivingStage(warehouse, manifest);

        // Act
        RouteStageExecutionResult result = stage.Execute(truck);

        // Assert
        RouteStageExecutionResult.Failure failure = Assert.IsType<RouteStageExecutionResult.Failure>(result);
        WarehouseReceiveFailedError error = Assert.IsType<WarehouseReceiveFailedError>(failure.Error);
        Assert.IsType<ProductStorageAddResult.InsufficientCapacity>(error.Reason);
        Assert.Single(truck.Cargo);
        Assert.Empty(warehouse.Stock);
    }

    [Fact]
    public void ExecuteWithUncarryableProductReturnsFailureWithoutChangingState()
    {
        // Arrange
        ProductBatch manifest = CreateManifest(new Ovh(21, 21), 1);
        var truck = new Truck(new Ovh(100, 100), new Speed(10), new Coordinates(0, 0));
        Warehouse warehouse = CreateWarehouse(new Ovh(100, 100), new Coordinates(0, 0));
        truck.Load(manifest);
        var stage = new ReceivingStage(warehouse, manifest);

        // Act
        RouteStageExecutionResult result = stage.Execute(truck);

        // Assert
        RouteStageExecutionResult.Failure failure = Assert.IsType<RouteStageExecutionResult.Failure>(result);
        ProductTransferFailedError error = Assert.IsType<ProductTransferFailedError>(failure.Error);
        Assert.IsType<WarehouseTransferResult.UncarryableProduct>(error.Reason);
        Assert.Single(truck.Cargo);
        Assert.Empty(warehouse.Stock);
    }

    private static Warehouse CreateWarehouse(Ovh capacity, Coordinates position)
    {
        var employees = new List<Employee>
        {
            new Employee(new Ovh(20, 20), new Duration(TimeSpan.FromMinutes(5))),
        };

        return new Warehouse(new WarehouseId("warehouse-1"), capacity, position, employees);
    }

    private static ProductBatch CreateManifest(Ovh ovh, int quantity)
    {
        var sku = new Sku(new SkuId(1), "TV", ovh);

        return TestDataFactory.CreateProductBatch(sku, quantity);
    }
}

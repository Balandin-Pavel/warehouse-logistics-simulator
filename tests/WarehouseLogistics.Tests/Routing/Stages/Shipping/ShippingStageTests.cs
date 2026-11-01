using WarehouseLogistics.Geography;
using WarehouseLogistics.Products;
using WarehouseLogistics.Products.Results;
using WarehouseLogistics.Routing.Results;
using WarehouseLogistics.Routing.Stages.Shipping;
using WarehouseLogistics.Routing.Stages.Shipping.Errors;
using WarehouseLogistics.Time;
using WarehouseLogistics.Transport;
using WarehouseLogistics.Warehouses;
using WarehouseLogistics.Warehouses.Results;
using Xunit;

namespace WarehouseLogistics.Tests.Routing.Stages.Shipping;

public class ShippingStageTests
{
    [Fact]
    public void ExecuteWithValidStateTransfersProductsAndReturnsDuration()
    {
        // Arrange
        ProductBatch manifest = CreateManifest(new Ovh(5, 5), 5);
        var truck = new Truck(new Ovh(100, 100), new Speed(10), new Coordinates(0, 0));
        Warehouse warehouse = CreateWarehouse(new Ovh(100, 100), new Coordinates(0, 0));
        StockWarehouse(warehouse, manifest);
        var stage = new ShippingStage(warehouse, manifest);

        // Act
        RouteStageExecutionResult result = stage.Execute(truck);

        // Assert
        RouteStageExecutionResult.Success success = Assert.IsType<RouteStageExecutionResult.Success>(result);
        ProductQuantity loadedProduct = Assert.Single(truck.Cargo);
        Assert.Equal(5, loadedProduct.Quantity.Value);
        Assert.Empty(warehouse.Stock);
        Assert.Equal(TimeSpan.FromMinutes(10), success.Duration.Value);
    }

    [Fact]
    public void ExecuteWhenTruckIsTooFarReturnsFailureWithoutChangingState()
    {
        // Arrange
        ProductBatch manifest = CreateManifest(new Ovh(5, 5), 1);
        var truck = new Truck(new Ovh(100, 100), new Speed(10), new Coordinates(0, 1));
        Warehouse warehouse = CreateWarehouse(new Ovh(100, 100), new Coordinates(0, 0));
        StockWarehouse(warehouse, manifest);
        var stage = new ShippingStage(warehouse, manifest);

        // Act
        RouteStageExecutionResult result = stage.Execute(truck);

        // Assert
        RouteStageExecutionResult.Failure failure = Assert.IsType<RouteStageExecutionResult.Failure>(result);
        Assert.IsType<TruckTooFarFromWarehouseError>(failure.Error);
        Assert.Empty(truck.Cargo);
        Assert.Single(warehouse.Stock);
    }

    [Fact]
    public void ExecuteWithMissingWarehouseStockReturnsFailureWithoutChangingState()
    {
        // Arrange
        ProductBatch manifest = CreateManifest(new Ovh(5, 5), 1);
        var truck = new Truck(new Ovh(100, 100), new Speed(10), new Coordinates(0, 0));
        Warehouse warehouse = CreateWarehouse(new Ovh(100, 100), new Coordinates(0, 0));
        var stage = new ShippingStage(warehouse, manifest);

        // Act
        RouteStageExecutionResult result = stage.Execute(truck);

        // Assert
        RouteStageExecutionResult.Failure failure = Assert.IsType<RouteStageExecutionResult.Failure>(result);
        WarehouseShipFailedError error = Assert.IsType<WarehouseShipFailedError>(failure.Error);
        Assert.IsType<ProductStorageRemoveResult.InsufficientQuantity>(error.Reason);
        Assert.Empty(truck.Cargo);
        Assert.Empty(warehouse.Stock);
    }

    [Fact]
    public void ExecuteWithoutTruckCapacityReturnsFailureWithoutChangingState()
    {
        // Arrange
        ProductBatch manifest = CreateManifest(new Ovh(10, 10), 6);
        var truck = new Truck(new Ovh(50, 50), new Speed(10), new Coordinates(0, 0));
        Warehouse warehouse = CreateWarehouse(new Ovh(100, 100), new Coordinates(0, 0));
        StockWarehouse(warehouse, manifest);
        var stage = new ShippingStage(warehouse, manifest);

        // Act
        RouteStageExecutionResult result = stage.Execute(truck);

        // Assert
        RouteStageExecutionResult.Failure failure = Assert.IsType<RouteStageExecutionResult.Failure>(result);
        TruckLoadFailedError error = Assert.IsType<TruckLoadFailedError>(failure.Error);
        Assert.IsType<ProductStorageAddResult.InsufficientCapacity>(error.Reason);
        Assert.Empty(truck.Cargo);
        Assert.Single(warehouse.Stock);
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

    private static void StockWarehouse(Warehouse warehouse, ProductBatch batch)
    {
        var source = new ProductStorage(new Ovh(1000, 1000));
        source.Add(batch);
        WarehouseTransferResult result = warehouse.Receive(source, batch);

        Assert.IsType<WarehouseTransferResult.Success>(result);
    }
}

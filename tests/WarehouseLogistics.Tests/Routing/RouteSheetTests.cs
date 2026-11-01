using WarehouseLogistics.Geography;
using WarehouseLogistics.Products;
using WarehouseLogistics.Routing;
using WarehouseLogistics.Routing.Results;
using WarehouseLogistics.Routing.Stages.Movement;
using WarehouseLogistics.Routing.Stages.Receiving;
using WarehouseLogistics.Routing.Stages.Receiving.Errors;
using WarehouseLogistics.Routing.Stages.Shipping;
using WarehouseLogistics.Time;
using WarehouseLogistics.Transport;
using WarehouseLogistics.Warehouses;
using WarehouseLogistics.Warehouses.Results;
using Xunit;

namespace WarehouseLogistics.Tests.Routing;

public class RouteSheetTests
{
    [Fact]
    public void ConstructorWithEmptyStagesThrows()
    {
        // Arrange
        var stages = new List<IRouteStage>();

        // Act
        Action act = () =>
        {
            new RouteSheet(stages);
        };

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void ConstructorCopiesStageCollection()
    {
        // Arrange
        var firstStage = new MovementStage(new Coordinates(0, 1));
        var stages = new List<IRouteStage>
        {
            firstStage,
        };
        var routeSheet = new RouteSheet(stages);

        // Act
        stages.Add(new MovementStage(new Coordinates(1, 1)));

        // Assert
        Assert.Single(routeSheet.Stages);
        Assert.Same(firstStage, routeSheet.Stages[0]);
    }

    [Fact]
    public void StagesReturnsDefensiveCopy()
    {
        // Arrange
        var firstStage = new MovementStage(new Coordinates(0, 1));
        var replacementStage = new MovementStage(new Coordinates(1, 1));
        var stages = new List<IRouteStage>
        {
            firstStage,
        };
        var routeSheet = new RouteSheet(stages);

        // Act
        IList<IRouteStage> returnedStages = Assert.IsAssignableFrom<IList<IRouteStage>>(routeSheet.Stages);
        returnedStages[0] = replacementStage;

        // Assert
        Assert.Same(firstStage, routeSheet.Stages[0]);
    }

    [Fact]
    public void ExecuteSuccessfulRouteTransfersProductsAndReturnsTotalDuration()
    {
        // Arrange
        var sourcePosition = new Coordinates(0, 0);
        var destinationPosition = new Coordinates(0, 1);
        Warehouse source = CreateWarehouse("source", sourcePosition);
        Warehouse destination = CreateWarehouse("destination", destinationPosition);
        ProductBatch manifest = CreateManifest();
        StockWarehouse(source, manifest);
        var truck = new Truck(new Ovh(100, 100), new Speed(10), sourcePosition);
        var stages = new List<IRouteStage>
        {
            new ShippingStage(source, manifest),
            new MovementStage(destinationPosition),
            new ReceivingStage(destination, manifest),
        };
        var routeSheet = new RouteSheet(stages);
        var movementDuration = TimeSpan.FromSeconds(sourcePosition.CalculateDistanceTo(destinationPosition).Meters / 10);
        TimeSpan expectedDuration = TimeSpan.FromMinutes(5) + movementDuration;

        // Act
        RouteExecutionResult result = routeSheet.Execute(truck);

        // Assert
        RouteExecutionResult.Success success = Assert.IsType<RouteExecutionResult.Success>(result);
        Assert.Equal(expectedDuration, success.Duration.Value);
        Assert.Empty(source.Stock);
        Assert.Empty(truck.Cargo);
        Assert.Single(destination.Stock);
        Assert.Equal(destinationPosition, truck.Position);
    }

    [Fact]
    public void ExecuteStopsAfterFirstFailureAndReturnsCompletedDuration()
    {
        // Arrange
        var startPosition = new Coordinates(0, 0);
        var warehousePosition = new Coordinates(0, 1);
        var skippedDestination = new Coordinates(1, 1);
        Warehouse warehouse = CreateWarehouse("warehouse", warehousePosition);
        ProductBatch manifest = CreateManifest();
        var truck = new Truck(new Ovh(100, 100), new Speed(10), startPosition);
        var stages = new List<IRouteStage>
        {
            new MovementStage(warehousePosition),
            new ReceivingStage(warehouse, manifest),
            new MovementStage(skippedDestination),
        };
        var routeSheet = new RouteSheet(stages);
        var expectedDuration = TimeSpan.FromSeconds(startPosition.CalculateDistanceTo(warehousePosition).Meters / 10);

        // Act
        RouteExecutionResult result = routeSheet.Execute(truck);

        // Assert
        RouteExecutionResult.Failure failure = Assert.IsType<RouteExecutionResult.Failure>(result);
        Assert.IsType<TruckUnloadFailedError>(failure.Error);
        Assert.Equal(1, failure.FailedStageIndex);
        Assert.Equal(expectedDuration, failure.ElapsedDuration.Value);
        Assert.Equal(warehousePosition, truck.Position);
        Assert.Empty(warehouse.Stock);
    }

    [Fact]
    public void ExecuteCanUseSameRouteSheetWithDifferentTrucks()
    {
        // Arrange
        var destination = new Coordinates(0, 1);
        var stages = new IRouteStage[]
        {
            new MovementStage(destination),
        };
        var routeSheet = new RouteSheet(stages);
        var firstTruck = new Truck(new Ovh(100, 100), new Speed(10), new Coordinates(0, 0));
        var secondTruck = new Truck(new Ovh(100, 100), new Speed(20), new Coordinates(1, 0));

        // Act
        RouteExecutionResult firstResult = routeSheet.Execute(firstTruck);
        RouteExecutionResult secondResult = routeSheet.Execute(secondTruck);

        // Assert
        Assert.IsType<RouteExecutionResult.Success>(firstResult);
        Assert.IsType<RouteExecutionResult.Success>(secondResult);
        Assert.Equal(destination, firstTruck.Position);
        Assert.Equal(destination, secondTruck.Position);
    }

    [Fact]
    public void ExecuteTransfersManifestWithSeveralSku()
    {
        // Arrange
        var position = new Coordinates(0, 0);
        Warehouse source = CreateWarehouse("source", position);
        Warehouse destination = CreateWarehouse("destination", position);
        var firstSku = new Sku(new SkuId(1), "Phone", new Ovh(6, 4));
        var secondSku = new Sku(new SkuId(2), "TV", new Ovh(4, 6));
        var products = new ProductQuantity[]
        {
            new ProductQuantity(firstSku, new Quantity(2)),
            new ProductQuantity(secondSku, new Quantity(2)),
        };
        var manifest = new ProductBatch(products);
        StockWarehouse(source, manifest);
        var truck = new Truck(new Ovh(100, 100), new Speed(10), position);
        var stages = new IRouteStage[]
        {
            new ShippingStage(source, manifest),
            new ReceivingStage(destination, manifest),
        };
        var routeSheet = new RouteSheet(stages);

        // Act
        RouteExecutionResult result = routeSheet.Execute(truck);

        // Assert
        Assert.IsType<RouteExecutionResult.Success>(result);
        Assert.Empty(source.Stock);
        Assert.Empty(truck.Cargo);
        Assert.Equal(2, destination.Stock.Count);
    }

    private static Warehouse CreateWarehouse(string id, Coordinates position)
    {
        var employees = new List<Employee>
        {
            new Employee(new Ovh(20, 20), new Duration(TimeSpan.FromMinutes(5))),
        };

        return new Warehouse(new WarehouseId(id), new Ovh(100, 100), position, employees);
    }

    private static ProductBatch CreateManifest()
    {
        var sku = new Sku(new SkuId(1), "TV", new Ovh(5, 5));

        return TestDataFactory.CreateProductBatch(sku, 1);
    }

    private static void StockWarehouse(Warehouse warehouse, ProductBatch batch)
    {
        var source = new ProductStorage(new Ovh(1000, 1000));
        source.Add(batch);
        WarehouseTransferResult result = warehouse.Receive(source, batch);

        Assert.IsType<WarehouseTransferResult.Success>(result);
    }
}

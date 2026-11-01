using WarehouseLogistics.Geography;
using WarehouseLogistics.Products;
using WarehouseLogistics.Products.Results;
using WarehouseLogistics.Time;
using WarehouseLogistics.Warehouses;
using WarehouseLogistics.Warehouses.Results;
using Xunit;

namespace WarehouseLogistics.Tests.Warehouses;

public class WarehouseTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ConstructorWithInvalidIdThrows(string id)
    {
        // Arrange
        Employee[] staff = CreateStaff();

        // Act
        Action act = () =>
        {
            new Warehouse(new WarehouseId(id), new Ovh(100, 100), new Coordinates(0, 0), staff);
        };

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void ConstructorWithZeroCapacityThrows()
    {
        // Arrange
        Employee[] staff = CreateStaff();

        // Act
        Action act = () =>
        {
            new Warehouse(new WarehouseId("warehouse-1"), new Ovh(0, 0), new Coordinates(0, 0), staff);
        };

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void ConstructorWithEmptyStaffThrows()
    {
        // Arrange
        var staff = new List<Employee>();

        // Act
        Action act = () =>
        {
            new Warehouse(new WarehouseId("warehouse-1"), new Ovh(100, 100), new Coordinates(0, 0), staff);
        };

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void ConstructorCopiesStaffCollection()
    {
        // Arrange
        var firstEmployee = new Employee(new Ovh(20, 20), new Duration(TimeSpan.FromMinutes(5)));
        var staff = new List<Employee>
        {
            firstEmployee,
        };
        var warehouse = new Warehouse(new WarehouseId("warehouse-1"), new Ovh(100, 100), new Coordinates(0, 0), staff);

        // Act
        staff.Add(new Employee(new Ovh(30, 30), new Duration(TimeSpan.FromMinutes(4))));

        // Assert
        Assert.Single(warehouse.Staff);
        Assert.Same(firstEmployee, warehouse.Staff[0]);
    }

    [Fact]
    public void ReceiveValidBatchTransfersProductsAndReturnsDuration()
    {
        // Arrange
        var sku = new Sku(new SkuId(1), "TV", new Ovh(10, 10));
        ProductBatch batch = TestDataFactory.CreateProductBatch(sku, 3);
        Warehouse warehouse = CreateWarehouse(new Ovh(50, 50));
        var source = new ProductStorage(new Ovh(100, 100));
        source.Add(batch);

        // Act
        WarehouseTransferResult result = warehouse.Receive(source, batch);

        // Assert
        WarehouseTransferResult.Success success = Assert.IsType<WarehouseTransferResult.Success>(result);
        Assert.Equal(TimeSpan.FromMinutes(5), success.Duration.Value);
        Assert.Equal(2, success.ApproachCount.Value);
        Assert.Empty(source.Items);
        Assert.Equal(3, TestDataFactory.GetProductQuantity(warehouse.Stock, sku));
    }

    [Fact]
    public void ReceiveMissingSourceProductsReturnsFailureWithoutChangingState()
    {
        // Arrange
        var sku = new Sku(new SkuId(1), "TV", new Ovh(10, 10));
        ProductBatch batch = TestDataFactory.CreateProductBatch(sku, 1);
        Warehouse warehouse = CreateWarehouse(new Ovh(100, 100));
        var source = new ProductStorage(new Ovh(100, 100));

        // Act
        WarehouseTransferResult result = warehouse.Receive(source, batch);

        // Assert
        WarehouseTransferResult.SourceFailure failure = Assert.IsType<WarehouseTransferResult.SourceFailure>(result);
        Assert.IsType<ProductStorageRemoveResult.InsufficientQuantity>(failure.Reason);
        Assert.Empty(source.Items);
        Assert.Empty(warehouse.Stock);
    }

    [Fact]
    public void ReceiveWithoutWarehouseCapacityReturnsDestinationFailureWithoutChangingState()
    {
        // Arrange
        var sku = new Sku(new SkuId(1), "TV", new Ovh(10, 10));
        ProductBatch batch = TestDataFactory.CreateProductBatch(sku, 6);
        Warehouse warehouse = CreateWarehouse(new Ovh(50, 50));
        var source = new ProductStorage(new Ovh(100, 100));
        source.Add(batch);

        // Act
        WarehouseTransferResult result = warehouse.Receive(source, batch);

        // Assert
        WarehouseTransferResult.DestinationFailure failure = Assert.IsType<WarehouseTransferResult.DestinationFailure>(result);
        Assert.IsType<ProductStorageAddResult.InsufficientCapacity>(failure.Reason);
        Assert.Single(source.Items);
        Assert.Empty(warehouse.Stock);
    }

    [Fact]
    public void ReceiveUncarryableProductReturnsFailureWithoutChangingState()
    {
        // Arrange
        var sku = new Sku(new SkuId(1), "TV", new Ovh(21, 21));
        ProductBatch batch = TestDataFactory.CreateProductBatch(sku, 1);
        Warehouse warehouse = CreateWarehouse(new Ovh(100, 100));
        var source = new ProductStorage(new Ovh(100, 100));
        source.Add(batch);

        // Act
        WarehouseTransferResult result = warehouse.Receive(source, batch);

        // Assert
        WarehouseTransferResult.UncarryableProduct failure = Assert.IsType<WarehouseTransferResult.UncarryableProduct>(result);
        Assert.Same(sku, failure.Sku);
        Assert.Equal(new Ovh(20, 20), Assert.Single(failure.EmployeeCapacities));
        Assert.Single(source.Items);
        Assert.Empty(warehouse.Stock);
    }

    [Fact]
    public void ShipValidBatchTransfersProductsAndReturnsDuration()
    {
        // Arrange
        var sku = new Sku(new SkuId(1), "TV", new Ovh(5, 5));
        ProductBatch batch = TestDataFactory.CreateProductBatch(sku, 5);
        Warehouse warehouse = CreateWarehouse(new Ovh(100, 100));
        StockWarehouse(warehouse, batch);
        var destination = new ProductStorage(new Ovh(100, 100));

        // Act
        WarehouseTransferResult result = warehouse.Ship(destination, batch);

        // Assert
        WarehouseTransferResult.Success success = Assert.IsType<WarehouseTransferResult.Success>(result);
        Assert.Equal(TimeSpan.FromMinutes(10), success.Duration.Value);
        Assert.Equal(2, success.ApproachCount.Value);
        Assert.Empty(warehouse.Stock);
        Assert.Equal(5, TestDataFactory.GetProductQuantity(destination.Items, sku));
    }

    [Fact]
    public void ShipMissingWarehouseStockReturnsSourceFailureWithoutChangingState()
    {
        // Arrange
        var sku = new Sku(new SkuId(1), "TV", new Ovh(5, 5));
        ProductBatch batch = TestDataFactory.CreateProductBatch(sku, 1);
        Warehouse warehouse = CreateWarehouse(new Ovh(100, 100));
        var destination = new ProductStorage(new Ovh(100, 100));

        // Act
        WarehouseTransferResult result = warehouse.Ship(destination, batch);

        // Assert
        WarehouseTransferResult.SourceFailure failure = Assert.IsType<WarehouseTransferResult.SourceFailure>(result);
        Assert.IsType<ProductStorageRemoveResult.InsufficientQuantity>(failure.Reason);
        Assert.Empty(warehouse.Stock);
        Assert.Empty(destination.Items);
    }

    [Fact]
    public void ShipWithoutDestinationCapacityReturnsFailureWithoutChangingState()
    {
        // Arrange
        var sku = new Sku(new SkuId(1), "TV", new Ovh(10, 10));
        ProductBatch batch = TestDataFactory.CreateProductBatch(sku, 6);
        Warehouse warehouse = CreateWarehouse(new Ovh(100, 100));
        StockWarehouse(warehouse, batch);
        var destination = new ProductStorage(new Ovh(50, 50));

        // Act
        WarehouseTransferResult result = warehouse.Ship(destination, batch);

        // Assert
        WarehouseTransferResult.DestinationFailure failure = Assert.IsType<WarehouseTransferResult.DestinationFailure>(result);
        Assert.IsType<ProductStorageAddResult.InsufficientCapacity>(failure.Reason);
        Assert.Single(warehouse.Stock);
        Assert.Empty(destination.Items);
    }

    private static Warehouse CreateWarehouse(Ovh capacity)
    {
        return new Warehouse(
            new WarehouseId("warehouse-1"),
            capacity,
            new Coordinates(0, 0),
            CreateStaff());
    }

    private static Employee[] CreateStaff()
    {
        return new Employee[]
        {
            new Employee(new Ovh(20, 20), new Duration(TimeSpan.FromMinutes(5))),
        };
    }

    private static void StockWarehouse(Warehouse warehouse, ProductBatch batch)
    {
        var source = new ProductStorage(new Ovh(1000, 1000));
        source.Add(batch);
        WarehouseTransferResult result = warehouse.Receive(source, batch);

        Assert.IsType<WarehouseTransferResult.Success>(result);
    }
}

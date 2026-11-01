using WarehouseLogistics.Geography;
using WarehouseLogistics.Products;
using WarehouseLogistics.Products.Results;
using WarehouseLogistics.Time;
using WarehouseLogistics.Transport;
using Xunit;

namespace WarehouseLogistics.Tests.Transport;

public class TruckTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ConstructorWithNonPositiveSpeedThrows(double speed)
    {
        // Arrange
        var position = new Coordinates(0, 0);

        // Act
        Action act = () =>
        {
            new Truck(new Ovh(100, 100), new Speed(speed), position);
        };

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void ConstructorWithZeroCapacityThrows()
    {
        // Arrange
        var position = new Coordinates(0, 0);

        // Act
        Action act = () =>
        {
            new Truck(new Ovh(0, 0), new Speed(10), position);
        };

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void LoadWithinCapacityUpdatesCargo()
    {
        // Arrange
        var sku = new Sku(new SkuId(1), "TV", new Ovh(10, 10));
        ProductBatch batch = TestDataFactory.CreateProductBatch(sku, 3);
        var truck = new Truck(new Ovh(50, 50), new Speed(10), new Coordinates(0, 0));

        // Act
        ProductStorageAddResult result = truck.Load(batch);

        // Assert
        Assert.IsType<ProductStorageAddResult.Success>(result);
        Assert.Equal(3, TestDataFactory.GetProductQuantity(truck.Cargo, sku));
        Assert.Equal(new Ovh(30, 30), truck.LoadedOvh);
    }

    [Fact]
    public void RepeatedLoadAddsQuantityForExistingSku()
    {
        // Arrange
        var sku = new Sku(new SkuId(1), "TV", new Ovh(10, 10));
        ProductBatch batch = TestDataFactory.CreateProductBatch(sku, 2);
        var truck = new Truck(new Ovh(100, 100), new Speed(10), new Coordinates(0, 0));

        // Act
        truck.Load(batch);
        truck.Load(batch);

        // Assert
        Assert.Single(truck.Cargo);
        Assert.Equal(4, TestDataFactory.GetProductQuantity(truck.Cargo, sku));
        Assert.Equal(new Ovh(40, 40), truck.LoadedOvh);
    }

    [Fact]
    public void LoadExceedingCapacityDoesNotChangeCargo()
    {
        // Arrange
        var sku = new Sku(new SkuId(1), "TV", new Ovh(10, 10));
        ProductBatch batch = TestDataFactory.CreateProductBatch(sku, 6);
        var truck = new Truck(new Ovh(50, 50), new Speed(10), new Coordinates(0, 0));

        // Act
        ProductStorageAddResult result = truck.Load(batch);

        // Assert
        Assert.IsType<ProductStorageAddResult.InsufficientCapacity>(result);
        Assert.Empty(truck.Cargo);
        Assert.Equal(new Ovh(0, 0), truck.LoadedOvh);
    }

    [Fact]
    public void LoadExceedingRemainingCapacityDoesNotChangeCargo()
    {
        // Arrange
        var firstSku = new Sku(new SkuId(1), "TV", new Ovh(10, 10));
        var secondSku = new Sku(new SkuId(2), "Phone", new Ovh(6, 6));
        ProductBatch firstBatch = TestDataFactory.CreateProductBatch(firstSku, 4);
        ProductBatch secondBatch = TestDataFactory.CreateProductBatch(secondSku, 2);
        var truck = new Truck(new Ovh(50, 50), new Speed(10), new Coordinates(0, 0));
        truck.Load(firstBatch);
        bool HasSecondSku(ProductQuantity item)
        {
            return item.Sku.Equals(secondSku);
        }

        // Act
        ProductStorageAddResult result = truck.Load(secondBatch);

        // Assert
        Assert.IsType<ProductStorageAddResult.InsufficientCapacity>(result);
        Assert.Single(truck.Cargo);
        Assert.Equal(4, TestDataFactory.GetProductQuantity(truck.Cargo, firstSku));
        Assert.DoesNotContain(truck.Cargo, HasSecondSku);
        Assert.Equal(new Ovh(40, 40), truck.LoadedOvh);
    }

    [Fact]
    public void UnloadAvailableBatchReducesCargo()
    {
        // Arrange
        var sku = new Sku(new SkuId(1), "TV", new Ovh(10, 10));
        var truck = new Truck(new Ovh(100, 100), new Speed(10), new Coordinates(0, 0));
        truck.Load(TestDataFactory.CreateProductBatch(sku, 5));

        // Act
        ProductStorageRemoveResult result = truck.Unload(TestDataFactory.CreateProductBatch(sku, 2));

        // Assert
        Assert.IsType<ProductStorageRemoveResult.Success>(result);
        Assert.Equal(3, TestDataFactory.GetProductQuantity(truck.Cargo, sku));
        Assert.Equal(new Ovh(30, 30), truck.LoadedOvh);
    }

    [Fact]
    public void UnloadAllOfSkuRemovesItFromCargo()
    {
        // Arrange
        var sku = new Sku(new SkuId(1), "TV", new Ovh(10, 10));
        ProductBatch batch = TestDataFactory.CreateProductBatch(sku, 2);
        var truck = new Truck(new Ovh(100, 100), new Speed(10), new Coordinates(0, 0));
        truck.Load(batch);

        // Act
        truck.Unload(batch);

        // Assert
        Assert.Empty(truck.Cargo);
        Assert.Equal(new Ovh(0, 0), truck.LoadedOvh);
    }

    [Fact]
    public void UnloadExceedingQuantityDoesNotChangeCargo()
    {
        // Arrange
        var sku = new Sku(new SkuId(1), "TV", new Ovh(10, 10));
        var truck = new Truck(new Ovh(100, 100), new Speed(10), new Coordinates(0, 0));
        truck.Load(TestDataFactory.CreateProductBatch(sku, 2));

        // Act
        ProductStorageRemoveResult result = truck.Unload(TestDataFactory.CreateProductBatch(sku, 3));

        // Assert
        ProductStorageRemoveResult.InsufficientQuantity error = Assert.IsType<ProductStorageRemoveResult.InsufficientQuantity>(result);
        Assert.Equal(new Quantity(2), error.AvailableQuantity);
        Assert.Equal(new Quantity(3), error.RequestedQuantity);
        Assert.Equal(2, TestDataFactory.GetProductQuantity(truck.Cargo, sku));
        Assert.Equal(new Ovh(20, 20), truck.LoadedOvh);
    }

    [Fact]
    public void UnloadMissingSkuDoesNotChangeCargo()
    {
        // Arrange
        var loadedSku = new Sku(new SkuId(1), "TV", new Ovh(10, 10));
        var missingSku = new Sku(new SkuId(2), "Phone", new Ovh(5, 5));
        var truck = new Truck(new Ovh(100, 100), new Speed(10), new Coordinates(0, 0));
        truck.Load(TestDataFactory.CreateProductBatch(loadedSku, 2));

        // Act
        ProductStorageRemoveResult result = truck.Unload(TestDataFactory.CreateProductBatch(missingSku, 1));

        // Assert
        ProductStorageRemoveResult.InsufficientQuantity error = Assert.IsType<ProductStorageRemoveResult.InsufficientQuantity>(result);
        Assert.Null(error.AvailableQuantity);
        Assert.Equal(new Quantity(1), error.RequestedQuantity);
        Assert.Single(truck.Cargo);
        Assert.Equal(2, TestDataFactory.GetProductQuantity(truck.Cargo, loadedSku));
        Assert.Equal(new Ovh(20, 20), truck.LoadedOvh);
    }

    [Fact]
    public void MoveToUpdatesPositionAndReturnsTravelTime()
    {
        // Arrange
        var destination = new Coordinates(0, 1);
        var truck = new Truck(new Ovh(100, 100), new Speed(10), new Coordinates(0, 0));

        // Act
        Duration travelTime = truck.MoveTo(destination);

        // Assert
        Assert.Equal(destination, truck.Position);
        Assert.InRange(travelTime.Value.TotalSeconds, 11_100, 11_130);
    }
}

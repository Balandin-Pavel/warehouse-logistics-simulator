using WarehouseLogistics.Products;
using WarehouseLogistics.Products.Results;
using Xunit;

namespace WarehouseLogistics.Tests.Products;

public class ProductStorageTests
{
    [Fact]
    public void ConstructorWithZeroCapacityThrows()
    {
        // Arrange

        // Act
        Action act = static () =>
        {
            new ProductStorage(new Ovh(0, 0));
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
            new ProductStorage(new Ovh(volume, weight));
        };

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void GetItemsReturnsSnapshot()
    {
        // Arrange
        var sku = new Sku(new SkuId(1), "TV", new Ovh(10, 10));
        var storage = new ProductStorage(new Ovh(100, 100));
        IReadOnlyCollection<ProductQuantity> snapshot = storage.Items;

        // Act
        storage.Add(TestDataFactory.CreateProductBatch(sku, 1));

        // Assert
        Assert.Empty(snapshot);
        Assert.Single(storage.Items);
    }

    [Fact]
    public void AddWithinCapacityReturnsSuccessAndUpdatesStorage()
    {
        // Arrange
        var sku = new Sku(new SkuId(1), "TV", new Ovh(10, 10));
        var storage = new ProductStorage(new Ovh(100, 100));

        // Act
        ProductStorageAddResult result = storage.Add(TestDataFactory.CreateProductBatch(sku, 2));

        // Assert
        Assert.IsType<ProductStorageAddResult.Success>(result);
        Assert.Equal(2, TestDataFactory.GetProductQuantity(storage.Items, sku));
        Assert.Equal(new Ovh(20, 20), storage.StoredOvh);
    }

    [Fact]
    public void AddExceedingCapacityReturnsDetailsAndDoesNotChangeStorage()
    {
        // Arrange
        var sku = new Sku(new SkuId(1), "TV", new Ovh(30, 30));
        var storage = new ProductStorage(new Ovh(50, 50));
        storage.Add(TestDataFactory.CreateProductBatch(sku, 1));

        // Act
        ProductStorageAddResult result = storage.Add(TestDataFactory.CreateProductBatch(sku, 1));

        // Assert
        ProductStorageAddResult.InsufficientCapacity error = Assert.IsType<ProductStorageAddResult.InsufficientCapacity>(result);
        Assert.Equal(new Ovh(50, 50), error.Capacity);
        Assert.Equal(new Ovh(30, 30), error.StoredOvh);
        Assert.Equal(new Ovh(30, 30), error.RequestedOvh);
        Assert.Equal(1, TestDataFactory.GetProductQuantity(storage.Items, sku));
        Assert.Equal(new Ovh(30, 30), storage.StoredOvh);
    }

    [Fact]
    public void AddSkuWithSameIdAndDifferentNameReturnsConflictAndDoesNotChangeStorage()
    {
        // Arrange
        var storedSku = new Sku(new SkuId(1), "TV", new Ovh(10, 10));
        var incompatibleSku = new Sku(new SkuId(1), "Phone", new Ovh(10, 10));
        var storage = new ProductStorage(new Ovh(100, 100));
        storage.Add(TestDataFactory.CreateProductBatch(storedSku, 2));

        // Act
        ProductStorageAddResult result = storage.Add(TestDataFactory.CreateProductBatch(incompatibleSku, 1));

        // Assert
        ProductStorageAddResult.ConflictingSkuDefinition error = Assert.IsType<ProductStorageAddResult.ConflictingSkuDefinition>(result);
        Assert.Same(storedSku, error.ExistingSku);
        Assert.Same(incompatibleSku, error.IncomingSku);
        Assert.Equal(2, TestDataFactory.GetProductQuantity(storage.Items, storedSku));
        Assert.Equal(new Ovh(20, 20), storage.StoredOvh);
    }

    [Fact]
    public void AddSkuWithSameIdAndDifferentOvhReturnsConflictAndDoesNotChangeStorage()
    {
        // Arrange
        var storedSku = new Sku(new SkuId(1), "TV", new Ovh(10, 10));
        var incompatibleSku = new Sku(new SkuId(1), "TV", new Ovh(20, 20));
        var storage = new ProductStorage(new Ovh(100, 100));
        storage.Add(TestDataFactory.CreateProductBatch(storedSku, 2));

        // Act
        ProductStorageAddResult result = storage.Add(TestDataFactory.CreateProductBatch(incompatibleSku, 1));

        // Assert
        ProductStorageAddResult.ConflictingSkuDefinition error = Assert.IsType<ProductStorageAddResult.ConflictingSkuDefinition>(result);
        Assert.Same(storedSku, error.ExistingSku);
        Assert.Same(incompatibleSku, error.IncomingSku);
        Assert.Equal(2, TestDataFactory.GetProductQuantity(storage.Items, storedSku));
        Assert.Equal(new Ovh(20, 20), storage.StoredOvh);
    }

    [Fact]
    public void AddEquivalentSkuCombinesQuantity()
    {
        // Arrange
        var firstSku = new Sku(new SkuId(1), "TV", new Ovh(10, 10));
        var secondSku = new Sku(new SkuId(1), "TV", new Ovh(10, 10));
        var storage = new ProductStorage(new Ovh(100, 100));
        storage.Add(TestDataFactory.CreateProductBatch(firstSku, 2));

        // Act
        ProductStorageAddResult result = storage.Add(TestDataFactory.CreateProductBatch(secondSku, 1));

        // Assert
        Assert.IsType<ProductStorageAddResult.Success>(result);
        Assert.Single(storage.Items);
        Assert.Equal(3, TestDataFactory.GetProductQuantity(storage.Items, firstSku));
        Assert.Equal(new Ovh(30, 30), storage.StoredOvh);
    }

    [Fact]
    public void RemoveAvailableQuantityReturnsSuccessAndUpdatesStorage()
    {
        // Arrange
        var sku = new Sku(new SkuId(1), "TV", new Ovh(10, 10));
        var storage = new ProductStorage(new Ovh(100, 100));
        storage.Add(TestDataFactory.CreateProductBatch(sku, 3));

        // Act
        ProductStorageRemoveResult result = storage.Remove(TestDataFactory.CreateProductBatch(sku, 2));

        // Assert
        Assert.IsType<ProductStorageRemoveResult.Success>(result);
        Assert.Equal(1, TestDataFactory.GetProductQuantity(storage.Items, sku));
        Assert.Equal(new Ovh(10, 10), storage.StoredOvh);
    }

    [Fact]
    public void RemoveMissingSkuReturnsQuantityDetailsAndDoesNotChangeStorage()
    {
        // Arrange
        var storedSku = new Sku(new SkuId(1), "TV", new Ovh(10, 10));
        var missingSku = new Sku(new SkuId(2), "Phone", new Ovh(5, 5));
        var storage = new ProductStorage(new Ovh(100, 100));
        storage.Add(TestDataFactory.CreateProductBatch(storedSku, 2));

        // Act
        ProductStorageRemoveResult result = storage.Remove(TestDataFactory.CreateProductBatch(missingSku, 1));

        // Assert
        ProductStorageRemoveResult.InsufficientQuantity error = Assert.IsType<ProductStorageRemoveResult.InsufficientQuantity>(result);
        Assert.Same(missingSku, error.Sku);
        Assert.Null(error.AvailableQuantity);
        Assert.Equal(new Quantity(1), error.RequestedQuantity);
        Assert.Equal(2, TestDataFactory.GetProductQuantity(storage.Items, storedSku));
        Assert.Equal(new Ovh(20, 20), storage.StoredOvh);
    }

    [Fact]
    public void RemoveExceedingQuantityReturnsQuantityDetailsAndDoesNotChangeStorage()
    {
        // Arrange
        var sku = new Sku(new SkuId(1), "TV", new Ovh(10, 10));
        var storage = new ProductStorage(new Ovh(100, 100));
        storage.Add(TestDataFactory.CreateProductBatch(sku, 2));

        // Act
        ProductStorageRemoveResult result = storage.Remove(TestDataFactory.CreateProductBatch(sku, 3));

        // Assert
        ProductStorageRemoveResult.InsufficientQuantity error = Assert.IsType<ProductStorageRemoveResult.InsufficientQuantity>(result);
        Assert.Same(sku, error.Sku);
        Assert.Equal(new Quantity(2), error.AvailableQuantity);
        Assert.Equal(new Quantity(3), error.RequestedQuantity);
        Assert.Equal(2, TestDataFactory.GetProductQuantity(storage.Items, sku));
        Assert.Equal(new Ovh(20, 20), storage.StoredOvh);
    }

    [Fact]
    public void RemoveConflictingSkuReturnsConflictAndDoesNotChangeStorage()
    {
        // Arrange
        var storedSku = new Sku(new SkuId(1), "TV", new Ovh(10, 10));
        var requestedSku = new Sku(new SkuId(1), "Phone", new Ovh(10, 10));
        var storage = new ProductStorage(new Ovh(100, 100));
        storage.Add(TestDataFactory.CreateProductBatch(storedSku, 2));

        // Act
        ProductStorageRemoveResult result = storage.Remove(TestDataFactory.CreateProductBatch(requestedSku, 1));

        // Assert
        ProductStorageRemoveResult.ConflictingSkuDefinition error = Assert.IsType<ProductStorageRemoveResult.ConflictingSkuDefinition>(result);
        Assert.Same(storedSku, error.ExistingSku);
        Assert.Same(requestedSku, error.RequestedSku);
        Assert.Equal(2, TestDataFactory.GetProductQuantity(storage.Items, storedSku));
        Assert.Equal(new Ovh(20, 20), storage.StoredOvh);
    }

    [Theory]
    [InlineData(51, 1)]
    [InlineData(1, 51)]
    public void AddExceedingAnyCapacityComponentReturnsError(double volume, double weight)
    {
        // Arrange
        var sku = new Sku(new SkuId(1), "TV", new Ovh(volume, weight));
        var storage = new ProductStorage(new Ovh(50, 50));

        // Act
        ProductStorageAddResult result = storage.Add(TestDataFactory.CreateProductBatch(sku, 1));

        // Assert
        Assert.IsType<ProductStorageAddResult.InsufficientCapacity>(result);
        Assert.Empty(storage.Items);
        Assert.Equal(new Ovh(0, 0), storage.StoredOvh);
    }

    [Fact]
    public void AddCausingQuantityOverflowReturnsErrorAndDoesNotChangeStorage()
    {
        // Arrange
        var sku = new Sku(new SkuId(1), "Pin", new Ovh(double.Epsilon, double.Epsilon));
        var storage = new ProductStorage(new Ovh(1, 1));
        ProductBatch storedBatch = TestDataFactory.CreateProductBatch(sku, int.MaxValue);
        ProductBatch addedBatch = TestDataFactory.CreateProductBatch(sku, 1);
        storage.Add(storedBatch);

        // Act
        ProductStorageAddResult result = storage.Add(addedBatch);

        // Assert
        ProductStorageAddResult.QuantityOverflow error = Assert.IsType<ProductStorageAddResult.QuantityOverflow>(result);
        ProductQuantity storedProduct = Assert.Single(storage.Items);
        Assert.Same(sku, error.Sku);
        Assert.Equal(int.MaxValue, error.StoredQuantity.Value);
        Assert.Equal(1, error.AddedQuantity.Value);
        Assert.Equal(int.MaxValue, storedProduct.Quantity.Value);
        Assert.Equal(storedBatch.TotalOvh, storage.StoredOvh);
    }

    [Fact]
    public void TransferToWithValidBatchMovesProductsBetweenStorages()
    {
        // Arrange
        var sku = new Sku(new SkuId(1), "TV", new Ovh(10, 10));
        var source = new ProductStorage(new Ovh(100, 100));
        var destination = new ProductStorage(new Ovh(100, 100));
        ProductBatch batch = TestDataFactory.CreateProductBatch(sku, 2);
        source.Add(batch);

        // Act
        ProductStorageTransferResult result = source.TransferTo(destination, batch);

        // Assert
        Assert.IsType<ProductStorageTransferResult.Success>(result);
        Assert.Empty(source.Items);
        Assert.Equal(new Ovh(0, 0), source.StoredOvh);
        Assert.Equal(2, TestDataFactory.GetProductQuantity(destination.Items, sku));
        Assert.Equal(batch.TotalOvh, destination.StoredOvh);
    }

    [Fact]
    public void TransferToWithMissingSourceProductDoesNotChangeStorages()
    {
        // Arrange
        var sku = new Sku(new SkuId(1), "TV", new Ovh(10, 10));
        var source = new ProductStorage(new Ovh(100, 100));
        var destination = new ProductStorage(new Ovh(100, 100));
        ProductBatch batch = TestDataFactory.CreateProductBatch(sku, 1);

        // Act
        ProductStorageTransferResult result = source.TransferTo(destination, batch);

        // Assert
        ProductStorageTransferResult.RemoveFailure failure = Assert.IsType<ProductStorageTransferResult.RemoveFailure>(result);
        Assert.IsType<ProductStorageRemoveResult.InsufficientQuantity>(failure.Reason);
        Assert.Empty(source.Items);
        Assert.Empty(destination.Items);
        Assert.Equal(new Ovh(0, 0), source.StoredOvh);
        Assert.Equal(new Ovh(0, 0), destination.StoredOvh);
    }

    [Fact]
    public void TransferToWithoutDestinationCapacityDoesNotChangeStorages()
    {
        // Arrange
        var sku = new Sku(new SkuId(1), "TV", new Ovh(30, 30));
        var source = new ProductStorage(new Ovh(100, 100));
        var destination = new ProductStorage(new Ovh(50, 50));
        ProductBatch batch = TestDataFactory.CreateProductBatch(sku, 2);
        source.Add(batch);

        // Act
        ProductStorageTransferResult result = source.TransferTo(destination, batch);

        // Assert
        ProductStorageTransferResult.AddFailure failure = Assert.IsType<ProductStorageTransferResult.AddFailure>(result);
        Assert.IsType<ProductStorageAddResult.InsufficientCapacity>(failure.Reason);
        Assert.Equal(2, TestDataFactory.GetProductQuantity(source.Items, sku));
        Assert.Equal(batch.TotalOvh, source.StoredOvh);
        Assert.Empty(destination.Items);
        Assert.Equal(new Ovh(0, 0), destination.StoredOvh);
    }
}

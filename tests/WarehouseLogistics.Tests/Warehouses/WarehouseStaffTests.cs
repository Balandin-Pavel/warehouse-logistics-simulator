using WarehouseLogistics.Products;
using WarehouseLogistics.Time;
using WarehouseLogistics.Warehouses;
using WarehouseLogistics.Warehouses.Results;
using Xunit;

namespace WarehouseLogistics.Tests.Warehouses;

public class WarehouseStaffTests
{
    [Fact]
    public void ConstructorWithEmptyEmployeesThrows()
    {
        // Arrange
        var employees = new List<Employee>();

        // Act
        Action act = () =>
        {
            new WarehouseStaff(employees);
        };

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void ConstructorCopiesEmployeesCollection()
    {
        // Arrange
        var firstEmployee = new Employee(new Ovh(20, 20), new Duration(TimeSpan.FromMinutes(5)));
        var employees = new List<Employee>
        {
            firstEmployee,
        };
        var staff = new WarehouseStaff(employees);

        // Act
        employees.Add(new Employee(new Ovh(30, 30), new Duration(TimeSpan.FromMinutes(8))));

        // Assert
        Assert.Single(staff.Employees);
        Assert.Same(firstEmployee, staff.Employees[0]);
    }

    [Fact]
    public void EmployeesReturnsDefensiveCopy()
    {
        // Arrange
        var firstEmployee = new Employee(new Ovh(20, 20), new Duration(TimeSpan.FromMinutes(5)));
        var secondEmployee = new Employee(new Ovh(30, 30), new Duration(TimeSpan.FromMinutes(8)));
        var employees = new List<Employee>
        {
            firstEmployee,
        };
        var staff = new WarehouseStaff(employees);

        // Act
        IList<Employee> returnedEmployees = Assert.IsAssignableFrom<IList<Employee>>(staff.Employees);
        returnedEmployees[0] = secondEmployee;

        // Assert
        Assert.Same(firstEmployee, staff.Employees[0]);
    }

    [Fact]
    public void CalculateShippingTimeIncludesLastApproach()
    {
        // Arrange
        WarehouseStaff staff = CreateStaff();
        var sku = new Sku(new SkuId(1), "TV", new Ovh(5, 5));
        ProductBatch batch = TestDataFactory.CreateProductBatch(sku, 5);

        // Act
        WarehouseTransferResult result = staff.CalculateShippingTime(batch);

        // Assert
        WarehouseTransferResult.Success success = Assert.IsType<WarehouseTransferResult.Success>(result);
        Assert.Equal(TimeSpan.FromMinutes(10), success.Duration.Value);
        Assert.Equal(2, success.ApproachCount.Value);
    }

    [Fact]
    public void CalculateReceivingTimeExcludesLastApproach()
    {
        // Arrange
        WarehouseStaff staff = CreateStaff();
        var sku = new Sku(new SkuId(1), "TV", new Ovh(5, 5));
        ProductBatch batch = TestDataFactory.CreateProductBatch(sku, 5);

        // Act
        WarehouseTransferResult result = staff.CalculateReceivingTime(batch);

        // Assert
        WarehouseTransferResult.Success success = Assert.IsType<WarehouseTransferResult.Success>(result);
        Assert.Equal(TimeSpan.FromMinutes(5), success.Duration.Value);
        Assert.Equal(2, success.ApproachCount.Value);
    }

    [Fact]
    public void CalculateShippingTimeWithUncarryableProductReturnsDetails()
    {
        // Arrange
        WarehouseStaff staff = CreateStaff();
        var sku = new Sku(new SkuId(1), "TV", new Ovh(21, 21));
        ProductBatch batch = TestDataFactory.CreateProductBatch(sku, 1);

        // Act
        WarehouseTransferResult result = staff.CalculateShippingTime(batch);

        // Assert
        WarehouseTransferResult.UncarryableProduct error = Assert.IsType<WarehouseTransferResult.UncarryableProduct>(result);
        Assert.Same(sku, error.Sku);
        Assert.Equal(new Ovh(20, 20), Assert.Single(error.EmployeeCapacities));
    }

    [Fact]
    public void CalculateReceivingTimeWithUncarryableProductReturnsDetails()
    {
        // Arrange
        WarehouseStaff staff = CreateStaff();
        var sku = new Sku(new SkuId(1), "TV", new Ovh(21, 21));
        ProductBatch batch = TestDataFactory.CreateProductBatch(sku, 1);

        // Act
        WarehouseTransferResult result = staff.CalculateReceivingTime(batch);

        // Assert
        WarehouseTransferResult.UncarryableProduct error = Assert.IsType<WarehouseTransferResult.UncarryableProduct>(result);
        Assert.Same(sku, error.Sku);
        Assert.Equal(new Ovh(20, 20), Assert.Single(error.EmployeeCapacities));
    }

    [Fact]
    public void DifferentEmployeeLimitsCannotBeCombinedForOneProduct()
    {
        // Arrange
        var employees = new List<Employee>
        {
            new Employee(new Ovh(10, 1), new Duration(TimeSpan.FromMinutes(5))),
            new Employee(new Ovh(1, 10), new Duration(TimeSpan.FromMinutes(5))),
        };
        var staff = new WarehouseStaff(employees);
        var sku = new Sku(new SkuId(1), "TV", new Ovh(10, 10));
        ProductBatch batch = TestDataFactory.CreateProductBatch(sku, 1);

        // Act
        WarehouseTransferResult result = staff.CalculateShippingTime(batch);

        // Assert
        WarehouseTransferResult.UncarryableProduct error = Assert.IsType<WarehouseTransferResult.UncarryableProduct>(result);
        Assert.Same(sku, error.Sku);
        Assert.Equal(2, error.EmployeeCapacities.Count);
    }

    [Fact]
    public void DifferentSkuCanShareOneApproach()
    {
        // Arrange
        WarehouseStaff staff = CreateStaff();
        var firstSku = new Sku(new SkuId(1), "Phone", new Ovh(6, 4));
        var secondSku = new Sku(new SkuId(2), "TV", new Ovh(4, 6));
        var products = new ProductQuantity[]
        {
            new ProductQuantity(firstSku, new Quantity(2)),
            new ProductQuantity(secondSku, new Quantity(2)),
        };
        var batch = new ProductBatch(products);

        // Act
        WarehouseTransferResult result = staff.CalculateShippingTime(batch);

        // Assert
        WarehouseTransferResult.Success success = Assert.IsType<WarehouseTransferResult.Success>(result);
        Assert.Equal(TimeSpan.FromMinutes(5), success.Duration.Value);
        Assert.Equal(1, success.ApproachCount.Value);
    }

    private static WarehouseStaff CreateStaff()
    {
        var employees = new List<Employee>
        {
            new Employee(new Ovh(20, 20), new Duration(TimeSpan.FromMinutes(5))),
        };

        return new WarehouseStaff(employees);
    }
}

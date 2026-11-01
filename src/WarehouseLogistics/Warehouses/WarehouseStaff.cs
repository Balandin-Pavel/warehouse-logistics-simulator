using WarehouseLogistics.Products;
using WarehouseLogistics.Time;
using WarehouseLogistics.Warehouses.Results;

namespace WarehouseLogistics.Warehouses;

public sealed class WarehouseStaff
{
    private readonly IReadOnlyCollection<Employee> _employees;

    public WarehouseStaff(IReadOnlyCollection<Employee> employees)
    {
        if (employees.Count == 0)
        {
            throw new ArgumentException("Warehouse staff must not be empty", nameof(employees));
        }

        _employees = employees switch
        {
            Employee[] employeeArray => employeeArray.ToArray(),
            _ => employees.ToList(),
        };
    }

    public IReadOnlyList<Employee> Employees => _employees.ToList();

    public WarehouseTransferResult CalculateShippingTime(ProductBatch batch)
    {
        ProductQuantity? uncarryableProduct = FindUncarryableProduct(batch);

        if (uncarryableProduct is not null)
        {
            return new WarehouseTransferResult.UncarryableProduct(uncarryableProduct.Sku, GetEmployeeCapacities());
        }

        int[] approachesByEmployee = DistributeProducts(batch);
        Duration duration = CalculateShippingDuration(approachesByEmployee);
        var approachCount = new ApproachCount(approachesByEmployee.Sum());

        return new WarehouseTransferResult.Success(duration, approachCount);
    }

    public WarehouseTransferResult CalculateReceivingTime(ProductBatch batch)
    {
        ProductQuantity? uncarryableProduct = FindUncarryableProduct(batch);

        if (uncarryableProduct is not null)
        {
            return new WarehouseTransferResult.UncarryableProduct(uncarryableProduct.Sku, GetEmployeeCapacities());
        }

        int[] approachesByEmployee = DistributeProducts(batch);
        Duration duration = CalculateReceivingDuration(approachesByEmployee);
        var approachCount = new ApproachCount(approachesByEmployee.Sum());

        return new WarehouseTransferResult.Success(duration, approachCount);
    }

    private static bool CannotBeCarriedByAnyEmployee(IReadOnlyCollection<Employee> employees, ProductQuantity item)
    {
        return employees.Any(employee => employee.CanCarry(item.Sku.Ovh)) is false;
    }

    private int[] DistributeProducts(ProductBatch batch)
    {
        var remainingCapacityByEmployee = new List<Ovh>[_employees.Count];

        for (int employeeIndex = 0; employeeIndex < _employees.Count; employeeIndex++)
        {
            remainingCapacityByEmployee[employeeIndex] = new List<Ovh>();
        }

        foreach (ProductQuantity product in batch.Items)
        {
            for (int productIndex = 0; productIndex < product.Quantity.Value; productIndex++)
            {
                AssignProduct(product.Sku.Ovh, remainingCapacityByEmployee);
            }
        }

        return remainingCapacityByEmployee.Select(SelectApproachesCount).ToArray();
    }

    private void AssignProduct(Ovh productOvh, List<Ovh>[] remainingCapacityByEmployee)
    {
        int selectedEmployeeIndex = -1;
        int selectedApproachIndex = -1;
        var selectedCompletionTime = new Duration(TimeSpan.MaxValue);
        double selectedRemainingCapacityScore = double.MaxValue;

        for (int employeeIndex = 0; employeeIndex < _employees.Count; employeeIndex++)
        {
            Employee employee = _employees.ElementAt(employeeIndex);

            if (employee.CanCarry(productOvh) is false)
            {
                continue;
            }

            List<Ovh> approaches = remainingCapacityByEmployee[employeeIndex];
            int approachIndex = FindTightestFittingApproach(approaches, productOvh, employee.MaxCarryOvh);
            int approachesCount = approaches.Count;
            Ovh remainingCapacity;

            if (approachIndex >= 0)
            {
                remainingCapacity = approaches[approachIndex] - productOvh;
            }
            else
            {
                approachesCount++;
                remainingCapacity = employee.MaxCarryOvh - productOvh;
            }

            Duration completionTime = employee.DeliveryTime * approachesCount;
            double remainingCapacityScore = CalculateRemainingCapacityScore(remainingCapacity, employee.MaxCarryOvh);

            if (completionTime.Value < selectedCompletionTime.Value
                || (completionTime.Equals(selectedCompletionTime)
                    && remainingCapacityScore < selectedRemainingCapacityScore))
            {
                selectedEmployeeIndex = employeeIndex;
                selectedApproachIndex = approachIndex;
                selectedCompletionTime = completionTime;
                selectedRemainingCapacityScore = remainingCapacityScore;
            }
        }

        if (selectedEmployeeIndex < 0)
        {
            throw new InvalidOperationException("Product distribution invariant was violated");
        }

        List<Ovh> selectedApproaches = remainingCapacityByEmployee[selectedEmployeeIndex];

        if (selectedApproachIndex >= 0)
        {
            selectedApproaches[selectedApproachIndex] = selectedApproaches[selectedApproachIndex] - productOvh;
        }
        else
        {
            Ovh remainingCapacity = _employees.ElementAt(selectedEmployeeIndex).MaxCarryOvh - productOvh;
            selectedApproaches.Add(remainingCapacity);
        }
    }

    private int FindTightestFittingApproach(List<Ovh> approaches, Ovh productOvh, Ovh employeeCapacity)
    {
        int selectedApproachIndex = -1;
        double selectedRemainingCapacityScore = double.MaxValue;

        for (int approachIndex = 0; approachIndex < approaches.Count; approachIndex++)
        {
            Ovh remainingCapacity = approaches[approachIndex];

            if (productOvh <= remainingCapacity is false)
            {
                continue;
            }

            Ovh capacityAfterProduct = remainingCapacity - productOvh;
            double remainingCapacityScore = CalculateRemainingCapacityScore(capacityAfterProduct, employeeCapacity);

            if (remainingCapacityScore < selectedRemainingCapacityScore)
            {
                selectedApproachIndex = approachIndex;
                selectedRemainingCapacityScore = remainingCapacityScore;
            }
        }

        return selectedApproachIndex;
    }

    private double CalculateRemainingCapacityScore(Ovh remainingCapacity, Ovh totalCapacity)
    {
        return (remainingCapacity.Volume / totalCapacity.Volume) + (remainingCapacity.Weight / totalCapacity.Weight);
    }

    private Duration CalculateShippingDuration(int[] approachesByEmployee)
    {
        Duration duration = Duration.Zero;

        for (int employeeIndex = 0; employeeIndex < _employees.Count; employeeIndex++)
        {
            Duration employeeDuration = _employees.ElementAt(employeeIndex).DeliveryTime * approachesByEmployee[employeeIndex];

            if (employeeDuration.Value > duration.Value)
            {
                duration = employeeDuration;
            }
        }

        return duration;
    }

    private Duration CalculateReceivingDuration(int[] approachesByEmployee)
    {
        Duration duration = Duration.Zero;

        for (int employeeIndex = 0; employeeIndex < _employees.Count; employeeIndex++)
        {
            int completedApproaches = Math.Max(approachesByEmployee[employeeIndex] - 1, 0);
            Duration employeeDuration = _employees.ElementAt(employeeIndex).DeliveryTime * completedApproaches;

            if (employeeDuration.Value > duration.Value)
            {
                duration = employeeDuration;
            }
        }

        return duration;
    }

    private ProductQuantity? FindUncarryableProduct(ProductBatch batch)
    {
        return batch.Items.FirstOrDefault(item => CannotBeCarriedByAnyEmployee(_employees, item));
    }

    private IReadOnlyCollection<Ovh> GetEmployeeCapacities()
    {
        return _employees switch
        {
            Employee[] => _employees
                .Select(SelectEmployeeCapacity)
                .ToArray(),

            _ => _employees
                .Select(SelectEmployeeCapacity)
                .ToList(),
        };
    }

    private int SelectApproachesCount(IReadOnlyCollection<Ovh> approaches)
    {
        return approaches.Count;
    }

    private Ovh SelectEmployeeCapacity(Employee employee)
    {
        return employee.MaxCarryOvh;
    }
}

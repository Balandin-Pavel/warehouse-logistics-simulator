using WarehouseLogistics.Geography;
using WarehouseLogistics.Products;
using WarehouseLogistics.Routing.Results;
using WarehouseLogistics.Routing.Stages.Shipping.Errors;
using WarehouseLogistics.Transport;
using WarehouseLogistics.Warehouses;
using WarehouseLogistics.Warehouses.Results;

namespace WarehouseLogistics.Routing.Stages.Shipping;

public sealed class ShippingStage : IRouteStage
{
    private readonly Warehouse _warehouse;
    private readonly ProductBatch _manifest;

    public ShippingStage(Warehouse warehouse, ProductBatch manifest)
    {
        _warehouse = warehouse;
        _manifest = manifest;
    }

    public RouteStageExecutionResult Execute(Truck truck)
    {
        Distance distance = truck.Position.CalculateDistanceTo(_warehouse.Position);

        if (distance.Meters > RouteStageConstants.MaximumWarehouseDistance.Meters)
        {
            var error = new TruckTooFarFromWarehouseError(distance);

            return new RouteStageExecutionResult.Failure(error);
        }

        WarehouseTransferResult transferResult = _warehouse.Ship(truck.CargoStorage, _manifest);

        if (transferResult is WarehouseTransferResult.SourceFailure sourceFailure)
        {
            return new RouteStageExecutionResult.Failure(new WarehouseShipFailedError(sourceFailure.Reason));
        }

        if (transferResult is WarehouseTransferResult.DestinationFailure destinationFailure)
        {
            return new RouteStageExecutionResult.Failure(new TruckLoadFailedError(destinationFailure.Reason));
        }

        if (transferResult is WarehouseTransferResult.UncarryableProduct uncarryableProduct)
        {
            return new RouteStageExecutionResult.Failure(new ProductTransferFailedError(uncarryableProduct));
        }

        if (transferResult is not WarehouseTransferResult.Success transferSuccess)
        {
            throw new InvalidOperationException("Warehouse staff returned an unknown shipping result");
        }

        return new RouteStageExecutionResult.Success(transferSuccess.Duration);
    }
}

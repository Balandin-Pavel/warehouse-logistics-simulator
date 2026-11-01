using WarehouseLogistics.Geography;
using WarehouseLogistics.Routing.Results;
using WarehouseLogistics.Time;
using WarehouseLogistics.Transport;

namespace WarehouseLogistics.Routing.Stages.Movement;

public sealed class MovementStage : IRouteStage
{
    public MovementStage(Coordinates coordinates)
    {
        Destination = coordinates;
    }

    public Coordinates Destination { get; }

    public RouteStageExecutionResult Execute(Truck truck)
    {
        Duration travelTime = truck.MoveTo(Destination);
        return new RouteStageExecutionResult.Success(travelTime);
    }
}

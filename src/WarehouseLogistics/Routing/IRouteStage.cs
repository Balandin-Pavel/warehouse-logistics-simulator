using WarehouseLogistics.Routing.Results;
using WarehouseLogistics.Transport;

namespace WarehouseLogistics.Routing;

public interface IRouteStage
{
    RouteStageExecutionResult Execute(Truck truck);
}

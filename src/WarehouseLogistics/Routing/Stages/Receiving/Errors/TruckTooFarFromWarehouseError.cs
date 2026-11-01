using WarehouseLogistics.Geography;
using WarehouseLogistics.Routing.Results;

namespace WarehouseLogistics.Routing.Stages.Receiving.Errors;

public sealed record TruckTooFarFromWarehouseError(Distance Distance) : RouteStageError;

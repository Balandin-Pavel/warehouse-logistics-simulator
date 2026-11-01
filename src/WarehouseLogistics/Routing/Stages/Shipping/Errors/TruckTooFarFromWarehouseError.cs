using WarehouseLogistics.Geography;
using WarehouseLogistics.Routing.Results;

namespace WarehouseLogistics.Routing.Stages.Shipping.Errors;

public sealed record TruckTooFarFromWarehouseError(Distance Distance) : RouteStageError;

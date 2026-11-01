using WarehouseLogistics.Products.Results;
using WarehouseLogistics.Routing.Results;

namespace WarehouseLogistics.Routing.Stages.Receiving.Errors;

public sealed record TruckUnloadFailedError(ProductStorageRemoveResult.Failure Reason) : RouteStageError;

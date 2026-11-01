using WarehouseLogistics.Products.Results;
using WarehouseLogistics.Routing.Results;

namespace WarehouseLogistics.Routing.Stages.Receiving.Errors;

public sealed record WarehouseReceiveFailedError(ProductStorageAddResult.Failure Reason) : RouteStageError;

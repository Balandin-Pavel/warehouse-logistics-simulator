using WarehouseLogistics.Products.Results;
using WarehouseLogistics.Routing.Results;

namespace WarehouseLogistics.Routing.Stages.Shipping.Errors;

public sealed record TruckLoadFailedError(ProductStorageAddResult.Failure Reason) : RouteStageError;

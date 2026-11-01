using WarehouseLogistics.Routing.Results;
using WarehouseLogistics.Warehouses.Results;

namespace WarehouseLogistics.Routing.Stages.Shipping.Errors;

public sealed record ProductTransferFailedError(WarehouseTransferResult.UncarryableProduct Reason) : RouteStageError;

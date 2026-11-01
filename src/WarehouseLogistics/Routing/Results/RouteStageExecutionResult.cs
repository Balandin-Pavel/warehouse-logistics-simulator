using WarehouseLogistics.Time;

namespace WarehouseLogistics.Routing.Results;

public abstract record RouteStageExecutionResult
{
    public sealed record Success(Duration Duration) : RouteStageExecutionResult;

    public sealed record Failure(RouteStageError Error) : RouteStageExecutionResult;
}

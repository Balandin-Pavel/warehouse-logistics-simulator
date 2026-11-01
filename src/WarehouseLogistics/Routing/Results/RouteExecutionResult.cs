using WarehouseLogistics.Time;

namespace WarehouseLogistics.Routing.Results;

public abstract record RouteExecutionResult
{
    public sealed record Success(Duration Duration) : RouteExecutionResult;

    public sealed record Failure(
        RouteStageError Error,
        int FailedStageIndex,
        Duration ElapsedDuration) : RouteExecutionResult;
}

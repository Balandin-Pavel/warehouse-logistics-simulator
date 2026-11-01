using WarehouseLogistics.Routing.Results;
using WarehouseLogistics.Time;
using WarehouseLogistics.Transport;

namespace WarehouseLogistics.Routing;

public sealed class RouteSheet
{
    private readonly IRouteStage[] _stages;

    public RouteSheet(IReadOnlyCollection<IRouteStage> stages)
    {
        if (stages.Count == 0)
        {
            throw new ArgumentException("Route sheet must contain at least one stage", nameof(stages));
        }

        _stages = stages.ToArray();
    }

    public IReadOnlyList<IRouteStage> Stages => _stages.ToList();

    public RouteExecutionResult Execute(Truck truck)
    {
        Duration totalDuration = Duration.Zero;
        int stageIndex = 0;
        foreach (IRouteStage stage in _stages)
        {
            RouteStageExecutionResult stageResult = stage.Execute(truck);

            switch (stageResult)
            {
                case RouteStageExecutionResult.Success success:
                    totalDuration = totalDuration + success.Duration;
                    break;

                case RouteStageExecutionResult.Failure failure:
                    return new RouteExecutionResult.Failure(failure.Error, stageIndex, totalDuration);

                default:
                    throw new InvalidOperationException("Route stage returned an unknown result type");
            }

            stageIndex++;
        }

        return new RouteExecutionResult.Success(totalDuration);
    }
}

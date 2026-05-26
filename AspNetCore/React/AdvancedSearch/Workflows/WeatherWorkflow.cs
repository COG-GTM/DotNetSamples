using Temporalio.Workflows;

namespace EqDemo.Workflows;

[Workflow]
public class WeatherWorkflow
{
    [WorkflowRun]
    public async Task<List<WeatherActivities.WeatherForecast>> RunAsync()
    {
        return await Workflow.ExecuteActivityAsync(
            (WeatherActivities act) => act.GetWeatherForecasts(),
            new ActivityOptions { StartToCloseTimeout = TimeSpan.FromSeconds(30) }
        );
    }
}

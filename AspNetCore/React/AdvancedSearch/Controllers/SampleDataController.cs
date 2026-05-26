using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Temporalio.Client;
using EqDemo.Workflows;

namespace EqDemo.Controllers
{
    [Route("api/[controller]")]
    public class SampleDataController : Controller
    {
        private readonly ITemporalClient _temporalClient;

        public SampleDataController(ITemporalClient temporalClient)
        {
            _temporalClient = temporalClient;
        }

        [HttpGet("[action]")]
        public async Task<IEnumerable<WeatherActivities.WeatherForecast>> WeatherForecasts()
        {
            var handle = await _temporalClient.StartWorkflowAsync(
                (WeatherWorkflow wf) => wf.RunAsync(),
                new WorkflowOptions
                {
                    Id = $"weather-forecast-{Guid.NewGuid()}",
                    TaskQueue = "advanced-search-task-queue"
                });

            return await handle.GetResultAsync();
        }
    }
}

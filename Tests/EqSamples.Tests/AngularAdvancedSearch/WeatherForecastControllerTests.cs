extern alias AngularAdvSearch;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using AngularWeatherController = AngularAdvSearch::EqDemo.Controllers.WeatherForecastController;

namespace EqSamples.Tests.AngularAdvancedSearch;

public class WeatherForecastControllerTests
{
    [Fact]
    public void Get_ReturnsFiveForecasts()
    {
        var logger = new Mock<ILogger<AngularWeatherController>>();
        var controller = new AngularWeatherController(logger.Object);

        var result = controller.Get();

        result.Should().HaveCount(5);
    }

    [Fact]
    public void Get_ForecastsHaveValidTemperatureRange()
    {
        var logger = new Mock<ILogger<AngularWeatherController>>();
        var controller = new AngularWeatherController(logger.Object);

        var result = controller.Get().ToList();

        foreach (var forecast in result)
        {
            forecast.TemperatureC.Should().BeInRange(-20, 54);
        }
    }

    [Fact]
    public void Get_ForecastsHaveFutureDates()
    {
        var logger = new Mock<ILogger<AngularWeatherController>>();
        var controller = new AngularWeatherController(logger.Object);

        var result = controller.Get().ToList();

        foreach (var forecast in result)
        {
            forecast.Date.Should().BeAfter(DateTime.Now);
        }
    }

    [Fact]
    public void Get_ForecastsHaveNonNullSummary()
    {
        var logger = new Mock<ILogger<AngularWeatherController>>();
        var controller = new AngularWeatherController(logger.Object);

        var result = controller.Get().ToList();

        foreach (var forecast in result)
        {
            forecast.Summary.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void Get_ForecastsHaveCorrectTemperatureF()
    {
        var logger = new Mock<ILogger<AngularWeatherController>>();
        var controller = new AngularWeatherController(logger.Object);

        var result = controller.Get().ToList();

        foreach (var forecast in result)
        {
            var expectedF = 32 + (int)(forecast.TemperatureC / 0.5556);
            forecast.TemperatureF.Should().Be(expectedF);
        }
    }
}

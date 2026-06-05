extern alias RazorAdvSearch;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RazorIndexModel = RazorAdvSearch::EqDemo.Pages.IndexModel;
using RazorPrivacyModel = RazorAdvSearch::EqDemo.Pages.PrivacyModel;
using RazorAboutModel = RazorAdvSearch::EqDemo.Pages.AboutModel;

namespace EqSamples.Tests.RazorAdvancedSearch;

public class PageModelsTests
{
    [Fact]
    public void IndexModel_OnGet_DoesNotThrow()
    {
        var logger = new Mock<ILogger<RazorIndexModel>>();
        var model = new RazorIndexModel(logger.Object);

        var exception = Record.Exception(() => model.OnGet());
        exception.Should().BeNull();
    }

    [Fact]
    public void PrivacyModel_OnGet_DoesNotThrow()
    {
        var logger = new Mock<ILogger<RazorPrivacyModel>>();
        var model = new RazorPrivacyModel(logger.Object);

        var exception = Record.Exception(() => model.OnGet());
        exception.Should().BeNull();
    }

    [Fact]
    public void AboutModel_OnGet_DoesNotThrow()
    {
        var logger = new Mock<ILogger<RazorAboutModel>>();
        var model = new RazorAboutModel(logger.Object);

        var exception = Record.Exception(() => model.OnGet());
        exception.Should().BeNull();
    }
}

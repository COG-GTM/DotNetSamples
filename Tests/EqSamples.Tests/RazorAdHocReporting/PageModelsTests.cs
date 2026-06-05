extern alias RazorAdHoc;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RazorIndexModel = RazorAdHoc::EqDemo.Pages.IndexModel;
using RazorPrivacyModel = RazorAdHoc::EqDemo.Pages.PrivacyModel;

namespace EqSamples.Tests.RazorAdHocReporting;

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
}

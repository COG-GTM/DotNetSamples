extern alias RazorDataFilter;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RazorIndexModel = RazorDataFilter::EqDemo.Pages.IndexModel;
using RazorPrivacyModel = RazorDataFilter::EqDemo.Pages.PrivacyModel;
using RazorErrorViewModel = RazorDataFilter::EqDemo.Models.ErrorViewModel;

namespace EqSamples.Tests.RazorDataFiltering;

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
    public void ErrorViewModel_ShowRequestId_ReturnsTrue_WhenIdSet()
    {
        var model = new RazorErrorViewModel { RequestId = "req-123" };
        model.ShowRequestId.Should().BeTrue();
    }

    [Fact]
    public void ErrorViewModel_ShowRequestId_ReturnsFalse_WhenIdNull()
    {
        var model = new RazorErrorViewModel { RequestId = null };
        model.ShowRequestId.Should().BeFalse();
    }

    [Fact]
    public void ErrorViewModel_ShowRequestId_ReturnsFalse_WhenIdEmpty()
    {
        var model = new RazorErrorViewModel { RequestId = "" };
        model.ShowRequestId.Should().BeFalse();
    }
}

extern alias RazorAdHoc;

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using RazorErrorModel = RazorAdHoc::EqDemo.Pages.ErrorModel;

namespace EqSamples.Tests.RazorAdHocReporting;

public class ErrorModelTests
{
    [Fact]
    public void ShowRequestId_ReturnsTrue_WhenRequestIdIsSet()
    {
        var logger = new Mock<ILogger<RazorErrorModel>>();
        var model = new RazorErrorModel(logger.Object);
        model.RequestId = "abc123";

        model.ShowRequestId.Should().BeTrue();
    }

    [Fact]
    public void ShowRequestId_ReturnsFalse_WhenRequestIdIsNull()
    {
        var logger = new Mock<ILogger<RazorErrorModel>>();
        var model = new RazorErrorModel(logger.Object);
        model.RequestId = null;

        model.ShowRequestId.Should().BeFalse();
    }

    [Fact]
    public void ShowRequestId_ReturnsFalse_WhenRequestIdIsEmpty()
    {
        var logger = new Mock<ILogger<RazorErrorModel>>();
        var model = new RazorErrorModel(logger.Object);
        model.RequestId = "";

        model.ShowRequestId.Should().BeFalse();
    }

    [Fact]
    public void OnGet_SetsRequestId()
    {
        var logger = new Mock<ILogger<RazorErrorModel>>();
        var model = new RazorErrorModel(logger.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.TraceIdentifier = "test-trace-id";
        model.PageContext = new PageContext { HttpContext = httpContext };

        model.OnGet();

        model.RequestId.Should().NotBeNullOrEmpty();
    }
}

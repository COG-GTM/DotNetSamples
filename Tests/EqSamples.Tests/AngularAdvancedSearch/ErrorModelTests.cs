extern alias AngularAdvSearch;

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using AngularErrorModel = AngularAdvSearch::EqDemo.Pages.ErrorModel;

namespace EqSamples.Tests.AngularAdvancedSearch;

public class ErrorModelTests
{
    [Fact]
    public void ShowRequestId_ReturnsTrue_WhenSet()
    {
        var logger = new Mock<ILogger<AngularErrorModel>>();
        var model = new AngularErrorModel(logger.Object);
        model.RequestId = "abc";
        model.ShowRequestId.Should().BeTrue();
    }

    [Fact]
    public void ShowRequestId_ReturnsFalse_WhenNull()
    {
        var logger = new Mock<ILogger<AngularErrorModel>>();
        var model = new AngularErrorModel(logger.Object);
        model.RequestId = null;
        model.ShowRequestId.Should().BeFalse();
    }

    [Fact]
    public void OnGet_SetsRequestId()
    {
        var logger = new Mock<ILogger<AngularErrorModel>>();
        var model = new AngularErrorModel(logger.Object);
        var httpContext = new DefaultHttpContext();
        httpContext.TraceIdentifier = "trace-3";
        model.PageContext = new PageContext { HttpContext = httpContext };

        model.OnGet();

        model.RequestId.Should().NotBeNullOrEmpty();
    }
}

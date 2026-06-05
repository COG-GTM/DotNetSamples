extern alias MvcDataFiltering;

using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using MvcHomeController = MvcDataFiltering::EqDemo.Controllers.HomeController;

namespace EqSamples.Tests.MvcDataFiltering;

public class HomeControllerTests
{
    [Fact]
    public void Index_ReturnsRedirectToOrderIndex()
    {
        var controller = new MvcHomeController();

        var result = controller.Index();

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Index");
        redirect.ControllerName.Should().Be("Order");
    }

    [Fact]
    public void Privacy_ReturnsViewResult()
    {
        var controller = new MvcHomeController();

        var result = controller.Privacy();

        result.Should().BeOfType<ViewResult>();
    }
}

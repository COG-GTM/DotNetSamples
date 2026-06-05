extern alias MvcDataFiltering;

using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using MvcOrder = MvcDataFiltering::EqDemo.Models.Order;
using MvcCustomer = MvcDataFiltering::EqDemo.Models.Customer;
using MvcEmployee = MvcDataFiltering::EqDemo.Models.Employee;
using MvcAppDbContext = MvcDataFiltering::EqDemo.AppDbContext;
using MvcOrderController = MvcDataFiltering::EqDemo.Controllers.OrderController;

namespace EqSamples.Tests.MvcDataFiltering;

public class OrderControllerTests
{
    private MvcAppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<MvcAppDbContext>()
            .UseInMemoryDatabase(databaseName: $"MvcDataFiltering_{Guid.NewGuid()}")
            .Options;
        return new MvcAppDbContext(options);
    }

    [Fact]
    public void Index_ReturnsViewResult_WithOrdersViewName()
    {
        using var context = CreateInMemoryContext();
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(sp => sp.GetService(It.IsAny<Type>())).Returns(null!);

        var controller = new MvcOrderController(serviceProvider.Object, context);

        var result = controller.Index();

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("Orders");
    }
}

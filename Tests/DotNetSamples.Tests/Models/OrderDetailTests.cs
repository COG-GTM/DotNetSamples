using EqDemo.Models;
using FluentAssertions;

namespace DotNetSamples.Tests.Models;

public class OrderDetailTests
{
    [Fact]
    public void Properties_ShouldRoundTrip()
    {
        var detail = new OrderDetail
        {
            OrderID = 10248,
            ProductID = 11,
            UnitPrice = 14.00m,
            Quantity = 12,
            Discount = 0.1f
        };

        detail.OrderID.Should().Be(10248);
        detail.ProductID.Should().Be(11);
        detail.UnitPrice.Should().Be(14.00m);
        detail.Quantity.Should().Be(12);
        detail.Discount.Should().BeApproximately(0.1f, 0.001f);
    }

    [Fact]
    public void DefaultValues_ShouldBeZero()
    {
        var detail = new OrderDetail();

        detail.OrderID.Should().Be(0);
        detail.ProductID.Should().Be(0);
        detail.UnitPrice.Should().Be(0);
        detail.Quantity.Should().Be(0);
        detail.Discount.Should().Be(0);
    }

    [Fact]
    public void NavigationProperties_ShouldBeSettable()
    {
        var order = new Order { Id = 1 };
        var product = new Product { Id = 10, Name = "Chai" };

        var detail = new OrderDetail
        {
            OrderID = 1,
            ProductID = 10,
            Order = order,
            Product = product
        };

        detail.Order.Should().NotBeNull();
        detail.Order!.Id.Should().Be(1);
        detail.Product.Should().NotBeNull();
        detail.Product!.Name.Should().Be("Chai");
    }
}

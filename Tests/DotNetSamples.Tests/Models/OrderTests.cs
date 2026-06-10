using EqDemo.Models;
using FluentAssertions;

namespace DotNetSamples.Tests.Models;

public class OrderTests
{
    [Fact]
    public void Name_ShouldFormatIdAndDate()
    {
        var order = new Order
        {
            Id = 42,
            OrderDate = new DateTime(2024, 3, 15)
        };

        order.Name.Should().Be("0042-2024-03-15");
    }

    [Fact]
    public void Name_ShouldHandleNullOrderDate()
    {
        var order = new Order
        {
            Id = 1,
            OrderDate = null
        };

        // string.Format with null DateTime? produces empty string for the date part
        order.Name.Should().Be("0001-");
    }

    [Fact]
    public void Name_ShouldPadIdToFourDigits()
    {
        var order = new Order
        {
            Id = 7,
            OrderDate = new DateTime(2023, 1, 1)
        };

        order.Name.Should().StartWith("0007-");
    }

    [Fact]
    public void Name_ShouldHandleLargeId()
    {
        var order = new Order
        {
            Id = 12345,
            OrderDate = new DateTime(2024, 6, 1)
        };

        order.Name.Should().Be("12345-2024-06-01");
    }

    [Fact]
    public void DefaultValues_ShouldBeNull()
    {
        var order = new Order();

        order.CustomerID.Should().BeNull();
        order.Customer.Should().BeNull();
        order.EmployeeID.Should().BeNull();
        order.Employee.Should().BeNull();
        order.Freight.Should().BeNull();
        order.ShippedDate.Should().BeNull();
        order.RequiredDate.Should().BeNull();
        order.ShipName.Should().BeNull();
        order.ShipAddress.Should().BeNull();
        order.ShipCity.Should().BeNull();
        order.ShipRegion.Should().BeNull();
        order.ShipPostalCode.Should().BeNull();
        order.ShipCountry.Should().BeNull();
    }

    [Fact]
    public void Properties_ShouldRoundTrip()
    {
        var order = new Order
        {
            Id = 100,
            OrderDate = new DateTime(2024, 1, 1),
            RequiredDate = new DateTime(2024, 1, 15),
            ShippedDate = new DateTime(2024, 1, 10),
            Freight = 25.50m,
            CustomerID = "CUST1",
            EmployeeID = 5,
            ShipVia = 2,
            ShipName = "Test Ship",
            ShipAddress = "123 Main St",
            ShipCity = "Seattle",
            ShipRegion = "WA",
            ShipPostalCode = "98101",
            ShipCountry = "USA"
        };

        order.Id.Should().Be(100);
        order.Freight.Should().Be(25.50m);
        order.CustomerID.Should().Be("CUST1");
        order.EmployeeID.Should().Be(5);
        order.ShipVia.Should().Be(2);
        order.ShipName.Should().Be("Test Ship");
        order.ShipAddress.Should().Be("123 Main St");
        order.ShipCity.Should().Be("Seattle");
        order.ShipRegion.Should().Be("WA");
        order.ShipPostalCode.Should().Be("98101");
        order.ShipCountry.Should().Be("USA");
    }

    [Fact]
    public void Items_ShouldBeSettable()
    {
        var order = new Order
        {
            Id = 1,
            Items = new List<OrderDetail>
            {
                new OrderDetail { OrderID = 1, ProductID = 10, UnitPrice = 5.0m, Quantity = 2 },
                new OrderDetail { OrderID = 1, ProductID = 20, UnitPrice = 10.0m, Quantity = 1 }
            }
        };

        order.Items.Should().HaveCount(2);
    }
}

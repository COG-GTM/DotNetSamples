extern alias RazorAdvSearch;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RazorAppDbContext = RazorAdvSearch::EqDemo.AppDbContext;
using RazorOrder = RazorAdvSearch::EqDemo.Models.Order;
using RazorOrderDetail = RazorAdvSearch::EqDemo.Models.OrderDetail;

namespace EqSamples.Tests.RazorAdvancedSearch;

public class AppDbContextTests
{
    private RazorAppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RazorAppDbContext>()
            .UseInMemoryDatabase($"RazorAdvSearch_{Guid.NewGuid()}")
            .Options;
        return new RazorAppDbContext(options);
    }

    [Fact]
    public void DbContext_HasAllDbSets()
    {
        using var context = CreateContext();

        context.Categories.Should().NotBeNull();
        context.Customers.Should().NotBeNull();
        context.Employees.Should().NotBeNull();
        context.Orders.Should().NotBeNull();
        context.Products.Should().NotBeNull();
        context.OrderDetails.Should().NotBeNull();
        context.Shippers.Should().NotBeNull();
        context.Suppliers.Should().NotBeNull();
    }

    [Fact]
    public async Task DbContext_OrderDetail_HasCompositeKey()
    {
        using var context = CreateContext();

        context.OrderDetails.Add(new RazorOrderDetail
        {
            OrderID = 10,
            ProductID = 20,
            UnitPrice = 5m,
            Quantity = 3,
            Discount = 0
        });
        await context.SaveChangesAsync();

        var found = await context.OrderDetails.FindAsync(10, 20);
        found.Should().NotBeNull();
    }

    [Fact]
    public async Task DbContext_CanAddAndQueryOrders()
    {
        using var context = CreateContext();

        context.Orders.Add(new RazorOrder { Id = 1, ShipCity = "NYC" });
        context.Orders.Add(new RazorOrder { Id = 2, ShipCity = "LA" });
        await context.SaveChangesAsync();

        var nycOrders = await context.Orders.Where(o => o.ShipCity == "NYC").ToListAsync();
        nycOrders.Should().HaveCount(1);
    }
}

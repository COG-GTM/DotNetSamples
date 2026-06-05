extern alias RazorDataFilter;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RazorAppDbContext = RazorDataFilter::EqDemo.AppDbContext;
using RazorOrder = RazorDataFilter::EqDemo.Models.Order;
using RazorCustomer = RazorDataFilter::EqDemo.Models.Customer;
using RazorEmployee = RazorDataFilter::EqDemo.Models.Employee;
using RazorOrderDetail = RazorDataFilter::EqDemo.Models.OrderDetail;

namespace EqSamples.Tests.RazorDataFiltering;

public class AppDbContextTests
{
    private RazorAppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RazorAppDbContext>()
            .UseInMemoryDatabase($"RazorDataFilter_{Guid.NewGuid()}")
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
    public async Task DbContext_CanCRUD_Order()
    {
        using var context = CreateContext();

        var order = new RazorOrder { Id = 1, ShipName = "Test" };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var retrieved = await context.Orders.FindAsync(1);
        retrieved.Should().NotBeNull();

        retrieved!.ShipName = "Updated";
        context.Update(retrieved);
        await context.SaveChangesAsync();

        var updated = await context.Orders.FindAsync(1);
        updated!.ShipName.Should().Be("Updated");

        context.Orders.Remove(updated);
        await context.SaveChangesAsync();
        (await context.Orders.FindAsync(1)).Should().BeNull();
    }

    [Fact]
    public async Task DbContext_OrderDetail_CompositeKey_Works()
    {
        using var context = CreateContext();

        context.OrderDetails.Add(new RazorOrderDetail { OrderID = 1, ProductID = 1, UnitPrice = 10, Quantity = 1, Discount = 0 });
        context.OrderDetails.Add(new RazorOrderDetail { OrderID = 1, ProductID = 2, UnitPrice = 20, Quantity = 2, Discount = 0 });
        await context.SaveChangesAsync();

        (await context.OrderDetails.CountAsync()).Should().Be(2);
        (await context.OrderDetails.FindAsync(1, 2)).Should().NotBeNull();
    }

    [Fact]
    public async Task DbContext_Models_NavigationProperties()
    {
        using var context = CreateContext();

        var customer = new RazorCustomer { Id = "C1", CompanyName = "TestCo" };
        var employee = new RazorEmployee { Id = 1, FirstName = "John", LastName = "Doe" };
        var order = new RazorOrder { Id = 1, CustomerID = "C1", EmployeeID = 1 };

        context.Customers.Add(customer);
        context.Employees.Add(employee);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var loaded = await context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Employee)
            .FirstAsync(o => o.Id == 1);

        loaded.Customer.Should().NotBeNull();
        loaded.Customer!.CompanyName.Should().Be("TestCo");
        loaded.Employee.Should().NotBeNull();
        loaded.Employee!.FullName.Should().Be("John Doe");
    }
}

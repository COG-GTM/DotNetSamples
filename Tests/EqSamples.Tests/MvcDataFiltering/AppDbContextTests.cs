extern alias MvcDataFiltering;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MvcAppDbContext = MvcDataFiltering::EqDemo.AppDbContext;
using MvcOrder = MvcDataFiltering::EqDemo.Models.Order;
using MvcCustomer = MvcDataFiltering::EqDemo.Models.Customer;
using MvcEmployee = MvcDataFiltering::EqDemo.Models.Employee;
using MvcProduct = MvcDataFiltering::EqDemo.Models.Product;
using MvcCategory = MvcDataFiltering::EqDemo.Models.Category;
using MvcOrderDetail = MvcDataFiltering::EqDemo.Models.OrderDetail;
using MvcShipper = MvcDataFiltering::EqDemo.Models.Shipper;
using MvcSupplier = MvcDataFiltering::EqDemo.Models.Supplier;

namespace EqSamples.Tests.MvcDataFiltering;

public class AppDbContextTests
{
    private MvcAppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MvcAppDbContext>()
            .UseInMemoryDatabase($"MvcDbContext_{Guid.NewGuid()}")
            .Options;
        return new MvcAppDbContext(options);
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
    public async Task DbContext_CanAddAndRetrieveOrder()
    {
        using var context = CreateContext();

        var order = new MvcOrder
        {
            Id = 1,
            OrderDate = DateTime.Now,
            ShipName = "Test"
        };

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var retrieved = await context.Orders.FindAsync(1);
        retrieved.Should().NotBeNull();
        retrieved!.ShipName.Should().Be("Test");
    }

    [Fact]
    public async Task DbContext_OrderDetail_HasCompositeKey()
    {
        using var context = CreateContext();

        var detail = new MvcOrderDetail
        {
            OrderID = 1,
            ProductID = 1,
            UnitPrice = 10m,
            Quantity = 5,
            Discount = 0
        };

        context.OrderDetails.Add(detail);
        await context.SaveChangesAsync();

        var retrieved = await context.OrderDetails.FindAsync(1, 1);
        retrieved.Should().NotBeNull();
        retrieved!.UnitPrice.Should().Be(10m);
    }

    [Fact]
    public async Task DbContext_CanAddAndRetrieveCustomer()
    {
        using var context = CreateContext();

        var customer = new MvcCustomer
        {
            Id = "TEST1",
            CompanyName = "Test Company",
            ContactName = "John Doe",
            Country = "US"
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var retrieved = await context.Customers.FindAsync("TEST1");
        retrieved.Should().NotBeNull();
        retrieved!.CompanyName.Should().Be("Test Company");
    }

    [Fact]
    public async Task DbContext_CanAddAndRetrieveEmployee()
    {
        using var context = CreateContext();

        var employee = new MvcEmployee
        {
            Id = 1,
            FirstName = "Jane",
            LastName = "Doe"
        };

        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        var retrieved = await context.Employees.FindAsync(1);
        retrieved.Should().NotBeNull();
        retrieved!.FullName.Should().Be("Jane Doe");
    }

    [Fact]
    public async Task DbContext_CanAddAndRetrieveProduct()
    {
        using var context = CreateContext();

        var product = new MvcProduct
        {
            Id = 1,
            Name = "Widget"
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        var retrieved = await context.Products.FindAsync(1);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Widget");
    }

    [Fact]
    public async Task DbContext_CanAddAndRetrieveCategory()
    {
        using var context = CreateContext();

        var cat = new MvcCategory
        {
            Id = 1,
            CategoryName = "Beverages"
        };

        context.Categories.Add(cat);
        await context.SaveChangesAsync();

        var retrieved = await context.Categories.FindAsync(1);
        retrieved.Should().NotBeNull();
        retrieved!.CategoryName.Should().Be("Beverages");
    }

    [Fact]
    public async Task DbContext_CanAddAndRetrieveShipper()
    {
        using var context = CreateContext();

        var shipper = new MvcShipper
        {
            Id = 1,
            CompanyName = "Express Shipping"
        };

        context.Shippers.Add(shipper);
        await context.SaveChangesAsync();

        var retrieved = await context.Shippers.FindAsync(1);
        retrieved.Should().NotBeNull();
        retrieved!.CompanyName.Should().Be("Express Shipping");
    }

    [Fact]
    public async Task DbContext_CanAddAndRetrieveSupplier()
    {
        using var context = CreateContext();

        var supplier = new MvcSupplier
        {
            Id = 1,
            CompanyName = "Acme Supplies"
        };

        context.Suppliers.Add(supplier);
        await context.SaveChangesAsync();

        var retrieved = await context.Suppliers.FindAsync(1);
        retrieved.Should().NotBeNull();
        retrieved!.CompanyName.Should().Be("Acme Supplies");
    }
}

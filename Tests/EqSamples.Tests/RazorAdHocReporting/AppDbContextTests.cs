extern alias RazorAdHoc;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RazorAppDbContext = RazorAdHoc::EqDemo.AppDbContext;
using RazorReport = RazorAdHoc::EqDemo.Models.Report;
using RazorOrder = RazorAdHoc::EqDemo.Models.Order;
using RazorCustomer = RazorAdHoc::EqDemo.Models.Customer;
using RazorEmployee = RazorAdHoc::EqDemo.Models.Employee;
using RazorProduct = RazorAdHoc::EqDemo.Models.Product;
using RazorCategory = RazorAdHoc::EqDemo.Models.Category;
using RazorOrderDetail = RazorAdHoc::EqDemo.Models.OrderDetail;
using RazorShipper = RazorAdHoc::EqDemo.Models.Shipper;
using RazorSupplier = RazorAdHoc::EqDemo.Models.Supplier;

namespace EqSamples.Tests.RazorAdHocReporting;

public class AppDbContextTests
{
    private RazorAppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RazorAppDbContext>()
            .UseInMemoryDatabase($"RazorAdHoc_{Guid.NewGuid()}")
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
        context.Reports.Should().NotBeNull();
    }

    [Fact]
    public async Task DbContext_CanAddAndRetrieveReport()
    {
        using var context = CreateContext();

        var report = new RazorReport
        {
            Id = "r1",
            Name = "Test Report",
            Description = "A test",
            ModelId = "model1",
            QueryJson = "{}",
            OwnerId = "user1"
        };

        context.Reports.Add(report);
        await context.SaveChangesAsync();

        var retrieved = await context.Reports.FindAsync("r1");
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Test Report");
    }

    [Fact]
    public async Task DbContext_CanQueryReportsByOwner()
    {
        using var context = CreateContext();

        context.Reports.AddRange(
            new RazorReport { Id = "r1", OwnerId = "user1", Name = "Report 1", ModelId = "m1" },
            new RazorReport { Id = "r2", OwnerId = "user2", Name = "Report 2", ModelId = "m1" },
            new RazorReport { Id = "r3", OwnerId = "user1", Name = "Report 3", ModelId = "m1" }
        );
        await context.SaveChangesAsync();

        var user1Reports = await context.Reports.Where(r => r.OwnerId == "user1").ToListAsync();
        user1Reports.Should().HaveCount(2);
    }

    [Fact]
    public async Task DbContext_OrderDetail_HasCompositeKey()
    {
        using var context = CreateContext();

        var detail = new RazorOrderDetail
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
    }

    [Fact]
    public async Task DbContext_CanAddMultipleEntities()
    {
        using var context = CreateContext();

        context.Categories.Add(new RazorCategory { Id = 1, CategoryName = "Beverages" });
        context.Customers.Add(new RazorCustomer { Id = "C1", CompanyName = "Co1" });
        context.Employees.Add(new RazorEmployee { Id = 1, FirstName = "A", LastName = "B" });
        context.Shippers.Add(new RazorShipper { Id = 1, CompanyName = "Ship1" });
        context.Suppliers.Add(new RazorSupplier { Id = 1, CompanyName = "Sup1" });
        context.Products.Add(new RazorProduct { Id = 1, Name = "P1" });
        context.Orders.Add(new RazorOrder { Id = 1 });

        await context.SaveChangesAsync();

        (await context.Categories.CountAsync()).Should().Be(1);
        (await context.Customers.CountAsync()).Should().Be(1);
        (await context.Employees.CountAsync()).Should().Be(1);
        (await context.Shippers.CountAsync()).Should().Be(1);
        (await context.Suppliers.CountAsync()).Should().Be(1);
        (await context.Products.CountAsync()).Should().Be(1);
        (await context.Orders.CountAsync()).Should().Be(1);
    }
}

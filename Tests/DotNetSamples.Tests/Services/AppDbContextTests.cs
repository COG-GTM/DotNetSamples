using EqDemo;
using EqDemo.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DotNetSamples.Tests.Services;

public class AppDbContextTests
{
    private AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public void CanCreateDatabase()
    {
        using var context = CreateInMemoryContext();
        context.Database.EnsureCreated().Should().BeTrue();
    }

    [Fact]
    public async Task CanAddAndRetrieveCustomer()
    {
        using var context = CreateInMemoryContext();
        context.Database.EnsureCreated();

        var customer = new Customer
        {
            Id = "TEST1",
            CompanyName = "Test Company",
            ContactName = "Test Contact",
            ContactTitle = "Owner",
            Address = "123 Main St",
            City = "Seattle",
            Region = "WA",
            PostalCode = "98101",
            Country = "USA",
            Phone = "555-1234",
            Fax = "555-5678"
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var retrieved = await context.Customers.FindAsync("TEST1");
        retrieved.Should().NotBeNull();
        retrieved!.CompanyName.Should().Be("Test Company");
    }

    [Fact]
    public async Task CanAddAndRetrieveProduct()
    {
        using var context = CreateInMemoryContext();
        context.Database.EnsureCreated();

        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            QuantityPerUnit = "10 boxes",
            UnitPrice = 19.99m,
            UnitsInStock = 50,
            Discontinued = false
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        var retrieved = await context.Products.FindAsync(1);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Test Product");
        retrieved.UnitPrice.Should().Be(19.99m);
    }

    [Fact]
    public async Task CanAddAndRetrieveOrder()
    {
        using var context = CreateInMemoryContext();
        context.Database.EnsureCreated();

        var order = new Order
        {
            Id = 1,
            OrderDate = new DateTime(2024, 1, 1),
            Freight = 15.50m,
            ShipName = "Test Shipment",
            ShipAddress = "123 Main",
            ShipCity = "Seattle",
            ShipRegion = "WA",
            ShipPostalCode = "98101",
            ShipCountry = "USA",
            CustomerID = ""
        };

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var retrieved = await context.Orders.FindAsync(1);
        retrieved.Should().NotBeNull();
        retrieved!.Freight.Should().Be(15.50m);
        retrieved.ShipCity.Should().Be("Seattle");
    }

    [Fact]
    public async Task CanAddAndRetrieveEmployee()
    {
        using var context = CreateInMemoryContext();
        context.Database.EnsureCreated();

        var employee = new Employee
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Title = "Sales Rep",
            TitleOfCourtesy = "Mr.",
            Address = "123 Oak",
            City = "Portland",
            Region = "OR",
            PostalCode = "97201",
            Country = "USA",
            HomePhone = "555-0001",
            Extension = "1234",
            PhotoPath = "/photos/john.jpg",
            Notes = "Test employee",
            Photo = Array.Empty<byte>()
        };

        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        var retrieved = await context.Employees.FindAsync(1);
        retrieved.Should().NotBeNull();
        retrieved!.FullName.Should().Be("John Doe");
    }

    [Fact]
    public async Task CanAddAndRetrieveReport()
    {
        using var context = CreateInMemoryContext();
        context.Database.EnsureCreated();

        var report = new Report
        {
            Id = "rpt-1",
            Name = "Test Report",
            Description = "A test report",
            ModelId = "adhoc-reporting",
            QueryJson = "{}",
            OwnerId = "user-1"
        };

        context.Reports.Add(report);
        await context.SaveChangesAsync();

        var retrieved = await context.Reports.FindAsync("rpt-1");
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Test Report");
        retrieved.ModelId.Should().Be("adhoc-reporting");
    }

    [Fact]
    public async Task CanAddAndRetrieveCategory()
    {
        using var context = CreateInMemoryContext();
        context.Database.EnsureCreated();

        var category = new Category
        {
            Id = 1,
            CategoryName = "Beverages",
            Description = "Soft drinks, coffees, teas",
            Picture = Array.Empty<byte>()
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var retrieved = await context.Categories.FindAsync(1);
        retrieved.Should().NotBeNull();
        retrieved!.CategoryName.Should().Be("Beverages");
    }

    [Fact]
    public async Task CanAddAndRetrieveSupplier()
    {
        using var context = CreateInMemoryContext();
        context.Database.EnsureCreated();

        var supplier = new Supplier
        {
            Id = 1,
            CompanyName = "Acme Corp",
            ContactName = "John Smith",
            ContactTitle = "CEO",
            Address = "100 Industry Blvd",
            City = "Chicago",
            Region = "IL",
            PostalCode = "60601",
            Country = "USA",
            Phone = "312-555-0001",
            Fax = "312-555-0002",
            HomePage = "https://acme.example.com"
        };

        context.Suppliers.Add(supplier);
        await context.SaveChangesAsync();

        var retrieved = await context.Suppliers.FindAsync(1);
        retrieved.Should().NotBeNull();
        retrieved!.CompanyName.Should().Be("Acme Corp");
    }

    [Fact]
    public async Task CanAddAndRetrieveShipper()
    {
        using var context = CreateInMemoryContext();
        context.Database.EnsureCreated();

        var shipper = new Shipper
        {
            Id = 1,
            CompanyName = "Speedy Express",
            Phone = "(503) 555-9831"
        };

        context.Shippers.Add(shipper);
        await context.SaveChangesAsync();

        var retrieved = await context.Shippers.FindAsync(1);
        retrieved.Should().NotBeNull();
        retrieved!.CompanyName.Should().Be("Speedy Express");
    }

    [Fact]
    public async Task CanQueryOrdersWithCustomerInclude()
    {
        using var context = CreateInMemoryContext();
        context.Database.EnsureCreated();

        var customer = new Customer
        {
            Id = "CUST1", CompanyName = "Test Corp",
            ContactName = "Jane", ContactTitle = "Mgr",
            Address = "1 St", City = "NYC", Region = "NY",
            PostalCode = "10001", Country = "USA",
            Phone = "555-0001", Fax = "555-0002"
        };
        var order = new Order
        {
            Id = 1, CustomerID = "CUST1", Customer = customer,
            ShipName = "Ship1", ShipAddress = "2 St",
            ShipCity = "NYC", ShipRegion = "NY",
            ShipPostalCode = "10001", ShipCountry = "USA"
        };

        context.Customers.Add(customer);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var retrieved = await context.Orders
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == 1);

        retrieved.Should().NotBeNull();
        retrieved!.Customer.Should().NotBeNull();
        retrieved.Customer!.CompanyName.Should().Be("Test Corp");
    }

    [Fact]
    public async Task CanQueryMultipleReportsForUser()
    {
        using var context = CreateInMemoryContext();
        context.Database.EnsureCreated();

        context.Reports.AddRange(
            new Report { Id = "r1", Name = "Report A", Description = "D", ModelId = "m1", OwnerId = "user-1", QueryJson = "{}" },
            new Report { Id = "r2", Name = "Report B", Description = "D", ModelId = "m1", OwnerId = "user-1", QueryJson = "{}" },
            new Report { Id = "r3", Name = "Report C", Description = "D", ModelId = "m1", OwnerId = "user-2", QueryJson = "{}" }
        );
        await context.SaveChangesAsync();

        var userReports = await context.Reports
            .Where(r => r.OwnerId == "user-1")
            .OrderBy(r => r.Name)
            .ToListAsync();

        userReports.Should().HaveCount(2);
        userReports[0].Name.Should().Be("Report A");
        userReports[1].Name.Should().Be("Report B");
    }

    [Fact]
    public async Task CanDeleteReport()
    {
        using var context = CreateInMemoryContext();
        context.Database.EnsureCreated();

        var report = new Report { Id = "r1", Name = "To Delete", Description = "D", ModelId = "m1", OwnerId = "u1", QueryJson = "{}" };
        context.Reports.Add(report);
        await context.SaveChangesAsync();

        context.Reports.Remove(report);
        await context.SaveChangesAsync();

        var retrieved = await context.Reports.FindAsync("r1");
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task CanUpdateReport()
    {
        using var context = CreateInMemoryContext();
        context.Database.EnsureCreated();

        var report = new Report { Id = "r1", Name = "Original", Description = "D", ModelId = "m1", OwnerId = "u1", QueryJson = "{}" };
        context.Reports.Add(report);
        await context.SaveChangesAsync();

        report.Name = "Updated";
        context.Reports.Update(report);
        await context.SaveChangesAsync();

        var retrieved = await context.Reports.FindAsync("r1");
        retrieved!.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task CanFilterProductsByCategory()
    {
        using var context = CreateInMemoryContext();
        context.Database.EnsureCreated();

        var cat1 = new Category { Id = 1, CategoryName = "Beverages", Description = "Drinks", Picture = Array.Empty<byte>() };
        var cat2 = new Category { Id = 2, CategoryName = "Condiments", Description = "Sauces", Picture = Array.Empty<byte>() };
        context.Categories.AddRange(cat1, cat2);

        context.Products.AddRange(
            new Product { Id = 1, Name = "Chai", CategoryID = 1, Category = cat1, QuantityPerUnit = "10" },
            new Product { Id = 2, Name = "Chang", CategoryID = 1, Category = cat1, QuantityPerUnit = "24" },
            new Product { Id = 3, Name = "Mustard", CategoryID = 2, Category = cat2, QuantityPerUnit = "12" }
        );
        await context.SaveChangesAsync();

        var beverages = await context.Products
            .Where(p => p.CategoryID == 1)
            .ToListAsync();

        beverages.Should().HaveCount(2);
    }

    [Fact]
    public async Task CanFilterOrdersByDateRange()
    {
        using var context = CreateInMemoryContext();
        context.Database.EnsureCreated();

        context.Orders.AddRange(
            new Order { Id = 1, OrderDate = new DateTime(2024, 1, 1), ShipName = "S1", ShipAddress = "A1", ShipCity = "C1", ShipRegion = "R1", ShipPostalCode = "P1", ShipCountry = "US", CustomerID = "" },
            new Order { Id = 2, OrderDate = new DateTime(2024, 3, 15), ShipName = "S2", ShipAddress = "A2", ShipCity = "C2", ShipRegion = "R2", ShipPostalCode = "P2", ShipCountry = "US", CustomerID = "" },
            new Order { Id = 3, OrderDate = new DateTime(2024, 6, 1), ShipName = "S3", ShipAddress = "A3", ShipCity = "C3", ShipRegion = "R3", ShipPostalCode = "P3", ShipCountry = "US", CustomerID = "" }
        );
        await context.SaveChangesAsync();

        var q1Orders = await context.Orders
            .Where(o => o.OrderDate >= new DateTime(2024, 1, 1) && o.OrderDate < new DateTime(2024, 4, 1))
            .ToListAsync();

        q1Orders.Should().HaveCount(2);
    }

    [Fact]
    public async Task OrderDetail_ShouldHaveCompositeKey()
    {
        using var context = CreateInMemoryContext();
        context.Database.EnsureCreated();

        var detail = new OrderDetail
        {
            OrderID = 1,
            ProductID = 10,
            UnitPrice = 14.00m,
            Quantity = 12,
            Discount = 0
        };

        context.OrderDetails.Add(detail);
        await context.SaveChangesAsync();

        var retrieved = await context.OrderDetails.FindAsync(1, 10);
        retrieved.Should().NotBeNull();
        retrieved!.UnitPrice.Should().Be(14.00m);
    }
}

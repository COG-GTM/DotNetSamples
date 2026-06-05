extern alias AngularAdvSearch;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using AngularAppDbContext = AngularAdvSearch::EqDemo.AppDbContext;
using AngularOrder = AngularAdvSearch::EqDemo.Models.Order;
using AngularCustomer = AngularAdvSearch::EqDemo.Models.Customer;
using AngularEmployee = AngularAdvSearch::EqDemo.Models.Employee;
using AngularProduct = AngularAdvSearch::EqDemo.Models.Product;
using AngularCategory = AngularAdvSearch::EqDemo.Models.Category;
using AngularOrderDetail = AngularAdvSearch::EqDemo.Models.OrderDetail;
using AngularShipper = AngularAdvSearch::EqDemo.Models.Shipper;
using AngularSupplier = AngularAdvSearch::EqDemo.Models.Supplier;
using AngularWeatherForecast = AngularAdvSearch::EqDemo.WeatherForecast;

namespace EqSamples.Tests.AngularAdvancedSearch;

public class ModelsAndDbContextTests
{
    private AngularAppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AngularAppDbContext>()
            .UseInMemoryDatabase($"AngularAdvSearch_{Guid.NewGuid()}")
            .Options;
        return new AngularAppDbContext(options);
    }

    [Fact]
    public void WeatherForecast_TemperatureF_ConvertsCorrectly()
    {
        var forecast = new AngularWeatherForecast { TemperatureC = 0 };
        forecast.TemperatureF.Should().Be(32);

        forecast.TemperatureC = 100;
        forecast.TemperatureF.Should().Be(32 + (int)(100 / 0.5556));

        forecast.TemperatureC = -20;
        forecast.TemperatureF.Should().Be(32 + (int)(-20 / 0.5556));
    }

    [Fact]
    public void WeatherForecast_Properties_SetAndGet()
    {
        var date = DateTime.Now;
        var forecast = new AngularWeatherForecast
        {
            Date = date,
            TemperatureC = 25,
            Summary = "Warm"
        };

        forecast.Date.Should().Be(date);
        forecast.TemperatureC.Should().Be(25);
        forecast.Summary.Should().Be("Warm");
    }

    [Fact]
    public void Order_Name_FormatsCorrectly()
    {
        var order = new AngularOrder
        {
            Id = 99,
            OrderDate = new DateTime(2024, 12, 25)
        };

        order.Name.Should().Be("0099-2024-12-25");
    }

    [Fact]
    public void Employee_FullName_CombinesNames()
    {
        var emp = new AngularEmployee { FirstName = "Alice", LastName = "Johnson" };
        emp.FullName.Should().Be("Alice Johnson");
    }

    [Fact]
    public void Employee_FullName_OnlyFirstName()
    {
        var emp = new AngularEmployee { FirstName = "Alice", LastName = null };
        emp.FullName.Should().Contain("Alice");
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
    public async Task DbContext_CRUD_Operations()
    {
        using var context = CreateContext();

        var cat = new AngularCategory { Id = 1, CategoryName = "Beverages" };
        context.Categories.Add(cat);
        await context.SaveChangesAsync();

        var found = await context.Categories.FindAsync(1);
        found.Should().NotBeNull();
        found!.CategoryName.Should().Be("Beverages");
    }

    [Fact]
    public async Task DbContext_OrderDetail_CompositeKey()
    {
        using var context = CreateContext();

        context.OrderDetails.Add(new AngularOrderDetail
        {
            OrderID = 5,
            ProductID = 10,
            UnitPrice = 15m,
            Quantity = 2,
            Discount = 0
        });
        await context.SaveChangesAsync();

        var found = await context.OrderDetails.FindAsync(5, 10);
        found.Should().NotBeNull();
        found!.UnitPrice.Should().Be(15m);
    }

    [Fact]
    public void AllModelProperties_Customer()
    {
        var c = new AngularCustomer
        {
            Id = "X",
            CompanyName = "C",
            Address = "A",
            City = "Ci",
            Region = "R",
            PostalCode = "P",
            Country = "Co",
            ContactName = "CN",
            ContactTitle = "CT",
            Phone = "Ph",
            Fax = "F"
        };

        c.Id.Should().Be("X");
        c.CompanyName.Should().Be("C");
        c.Address.Should().Be("A");
        c.City.Should().Be("Ci");
        c.Region.Should().Be("R");
        c.PostalCode.Should().Be("P");
        c.Country.Should().Be("Co");
        c.ContactName.Should().Be("CN");
        c.ContactTitle.Should().Be("CT");
        c.Phone.Should().Be("Ph");
        c.Fax.Should().Be("F");
    }

    [Fact]
    public void AllModelProperties_Product()
    {
        var cat = new AngularCategory { Id = 1 };
        var sup = new AngularSupplier { Id = 1 };
        var p = new AngularProduct
        {
            Id = 1,
            Name = "P",
            QuantityPerUnit = "Q",
            UnitPrice = 10m,
            UnitsInStock = 5,
            UnitsOnOrder = 2,
            ReorderLevel = 1,
            Discontinued = true,
            CategoryID = 1,
            Category = cat,
            SupplierID = 1,
            Supplier = sup
        };

        p.Discontinued.Should().BeTrue();
        p.ReorderLevel.Should().Be(1);
        p.UnitsOnOrder.Should().Be(2);
        p.UnitsInStock.Should().Be(5);
        p.Name.Should().Be("P");
        p.Category.Should().BeSameAs(cat);
        p.Supplier.Should().BeSameAs(sup);
    }

    [Fact]
    public void AllModelProperties_Order()
    {
        var c = new AngularCustomer { Id = "C1" };
        var e = new AngularEmployee { Id = 1 };
        var o = new AngularOrder
        {
            Id = 1, OrderDate = DateTime.Now, RequiredDate = DateTime.Now,
            ShippedDate = DateTime.Now, Freight = 10m, CustomerID = "C1",
            Customer = c, EmployeeID = 1, Employee = e,
            Items = new List<AngularOrderDetail>(),
            ShipVia = 1, ShipName = "S", ShipAddress = "A",
            ShipCity = "C", ShipRegion = "R", ShipPostalCode = "P",
            ShipCountry = "Co"
        };
        o.ShipName.Should().Be("S");
        o.Customer.Should().BeSameAs(c);
        o.Employee.Should().BeSameAs(e);
        o.Items.Should().BeEmpty();
        o.ShipVia.Should().Be(1);
        o.Freight.Should().Be(10m);
    }

    [Fact]
    public void AllModelProperties_Employee()
    {
        var mgr = new AngularEmployee { Id = 2 };
        var emp = new AngularEmployee
        {
            Id = 1, FirstName = "J", LastName = "D", Title = "T",
            TitleOfCourtesy = "Mr.", BirthDate = DateTime.Now,
            HireDate = DateTime.Now, Address = "A", City = "C",
            Region = "R", PostalCode = "P", Country = "Co",
            HomePhone = "H", Extension = "E",
            Photo = new byte[] { 1 }, PhotoPath = "/p",
            Notes = "N", ReportsTo = 2, Manager = mgr,
            Orders = new List<AngularOrder>()
        };
        emp.Title.Should().Be("T");
        emp.TitleOfCourtesy.Should().Be("Mr.");
        emp.Manager.Should().BeSameAs(mgr);
        emp.Orders.Should().BeEmpty();
    }

    [Fact]
    public void AllModelProperties_Category()
    {
        var c = new AngularCategory
        {
            Id = 1, CategoryName = "B", Description = "D",
            Picture = new byte[] { 1 }
        };
        c.CategoryName.Should().Be("B");
        c.Description.Should().Be("D");
        c.Picture.Should().HaveCount(1);
    }

    [Fact]
    public void AllModelProperties_OrderDetail()
    {
        var o = new AngularOrder { Id = 1 };
        var p = new AngularProduct { Id = 1 };
        var d = new AngularOrderDetail
        {
            OrderID = 1, Order = o, ProductID = 1, Product = p,
            UnitPrice = 10m, Quantity = 5, Discount = 0.1f
        };
        d.Order.Should().BeSameAs(o);
        d.Product.Should().BeSameAs(p);
    }

    [Fact]
    public void Employee_FullName_EmptyFirst()
    {
        var emp = new AngularEmployee { FirstName = "", LastName = "B" };
        emp.FullName.Should().Be("B");
    }

    [Fact]
    public void Employee_FullName_NullFirst()
    {
        var emp = new AngularEmployee { FirstName = null, LastName = "B" };
        emp.FullName.Should().Be("B");
    }

    [Fact]
    public void AllModelProperties_Shipper()
    {
        var s = new AngularShipper { Id = 1, CompanyName = "S", Phone = "P" };
        s.Id.Should().Be(1);
        s.CompanyName.Should().Be("S");
        s.Phone.Should().Be("P");
    }

    [Fact]
    public void AllModelProperties_Supplier()
    {
        var s = new AngularSupplier
        {
            Id = 1,
            CompanyName = "S",
            ContactName = "C",
            ContactTitle = "T",
            Address = "A",
            City = "Ci",
            Region = "R",
            PostalCode = "P",
            Country = "Co",
            Phone = "Ph",
            Fax = "F",
            HomePage = "H"
        };

        s.CompanyName.Should().Be("S");
        s.HomePage.Should().Be("H");
    }
}

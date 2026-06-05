extern alias RazorAdvSearch;

using FluentAssertions;
using RazorOrder = RazorAdvSearch::EqDemo.Models.Order;
using RazorCustomer = RazorAdvSearch::EqDemo.Models.Customer;
using RazorEmployee = RazorAdvSearch::EqDemo.Models.Employee;
using RazorProduct = RazorAdvSearch::EqDemo.Models.Product;
using RazorCategory = RazorAdvSearch::EqDemo.Models.Category;
using RazorOrderDetail = RazorAdvSearch::EqDemo.Models.OrderDetail;
using RazorShipper = RazorAdvSearch::EqDemo.Models.Shipper;
using RazorSupplier = RazorAdvSearch::EqDemo.Models.Supplier;

namespace EqSamples.Tests.RazorAdvancedSearch;

public class ModelsTests
{
    [Fact]
    public void Order_Name_FormatsCorrectly()
    {
        var order = new RazorOrder { Id = 99, OrderDate = new DateTime(2024, 12, 25) };
        order.Name.Should().Be("0099-2024-12-25");
    }

    [Fact]
    public void Order_AllProperties()
    {
        var c = new RazorCustomer { Id = "C1" };
        var e = new RazorEmployee { Id = 1 };
        var o = new RazorOrder
        {
            Id = 1, OrderDate = DateTime.Now, RequiredDate = DateTime.Now,
            ShippedDate = DateTime.Now, Freight = 10m, CustomerID = "C1",
            Customer = c, EmployeeID = 1, Employee = e,
            Items = new List<RazorOrderDetail>(),
            ShipVia = 1, ShipName = "S", ShipAddress = "A",
            ShipCity = "C", ShipRegion = "R", ShipPostalCode = "P",
            ShipCountry = "Co"
        };
        o.ShipName.Should().Be("S");
        o.Customer.Should().BeSameAs(c);
        o.Employee.Should().BeSameAs(e);
    }

    [Fact]
    public void Employee_FullName_CombinesNames()
    {
        var emp = new RazorEmployee { FirstName = "A", LastName = "B" };
        emp.FullName.Should().Be("A B");
    }

    [Fact]
    public void Employee_FullName_EmptyFirst()
    {
        var emp = new RazorEmployee { FirstName = "", LastName = "B" };
        emp.FullName.Should().Be("B");
    }

    [Fact]
    public void Employee_FullName_NullFirst()
    {
        var emp = new RazorEmployee { FirstName = null, LastName = "B" };
        emp.FullName.Should().Be("B");
    }

    [Fact]
    public void Employee_AllProperties()
    {
        var mgr = new RazorEmployee { Id = 2 };
        var emp = new RazorEmployee
        {
            Id = 1, FirstName = "J", LastName = "D", Title = "T",
            TitleOfCourtesy = "Mr.", BirthDate = DateTime.Now,
            HireDate = DateTime.Now, Address = "A", City = "C",
            Region = "R", PostalCode = "P", Country = "Co",
            HomePhone = "H", Extension = "E",
            Photo = new byte[] { 1 }, PhotoPath = "/p",
            Notes = "N", ReportsTo = 2, Manager = mgr,
            Orders = new List<RazorOrder>()
        };
        emp.Title.Should().Be("T");
        emp.Manager.Should().BeSameAs(mgr);
        emp.Orders.Should().BeEmpty();
    }

    [Fact]
    public void Customer_AllProperties()
    {
        var c = new RazorCustomer
        {
            Id = "X", CompanyName = "C", Address = "A", City = "Ci",
            Region = "R", PostalCode = "P", Country = "Co",
            ContactName = "CN", ContactTitle = "CT", Phone = "Ph", Fax = "F"
        };
        c.CompanyName.Should().Be("C");
        c.Fax.Should().Be("F");
    }

    [Fact]
    public void Product_AllProperties()
    {
        var cat = new RazorCategory { Id = 1 };
        var sup = new RazorSupplier { Id = 1 };
        var p = new RazorProduct
        {
            Id = 1, Name = "P", QuantityPerUnit = "Q", UnitPrice = 10m,
            UnitsInStock = 5, UnitsOnOrder = 2, ReorderLevel = 1,
            Discontinued = true, CategoryID = 1, Category = cat,
            SupplierID = 1, Supplier = sup
        };
        p.Name.Should().Be("P");
        p.Discontinued.Should().BeTrue();
    }

    [Fact]
    public void Category_AllProperties()
    {
        var c = new RazorCategory
        {
            Id = 1, CategoryName = "B", Description = "D",
            Picture = new byte[] { 1 }
        };
        c.CategoryName.Should().Be("B");
    }

    [Fact]
    public void OrderDetail_AllProperties()
    {
        var o = new RazorOrder { Id = 1 };
        var p = new RazorProduct { Id = 1 };
        var d = new RazorOrderDetail
        {
            OrderID = 1, Order = o, ProductID = 1, Product = p,
            UnitPrice = 10m, Quantity = 5, Discount = 0.1f
        };
        d.UnitPrice.Should().Be(10m);
        d.Order.Should().BeSameAs(o);
        d.Product.Should().BeSameAs(p);
    }

    [Fact]
    public void Shipper_AllProperties()
    {
        var s = new RazorShipper { Id = 1, CompanyName = "S", Phone = "P" };
        s.CompanyName.Should().Be("S");
    }

    [Fact]
    public void Supplier_AllProperties()
    {
        var s = new RazorSupplier
        {
            Id = 1, CompanyName = "S", ContactName = "C", ContactTitle = "T",
            Address = "A", City = "Ci", Region = "R", PostalCode = "P",
            Country = "Co", Phone = "Ph", Fax = "F", HomePage = "H"
        };
        s.HomePage.Should().Be("H");
    }
}

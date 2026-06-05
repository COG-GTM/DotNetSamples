extern alias RazorAdHoc;

using FluentAssertions;
using RazorOrder = RazorAdHoc::EqDemo.Models.Order;
using RazorCustomer = RazorAdHoc::EqDemo.Models.Customer;
using RazorEmployee = RazorAdHoc::EqDemo.Models.Employee;
using RazorProduct = RazorAdHoc::EqDemo.Models.Product;
using RazorCategory = RazorAdHoc::EqDemo.Models.Category;
using RazorOrderDetail = RazorAdHoc::EqDemo.Models.OrderDetail;
using RazorShipper = RazorAdHoc::EqDemo.Models.Shipper;
using RazorSupplier = RazorAdHoc::EqDemo.Models.Supplier;

namespace EqSamples.Tests.RazorAdHocReporting;

public class ModelsTests
{
    [Fact]
    public void Order_Name_FormatsCorrectly()
    {
        var order = new RazorOrder { Id = 42, OrderDate = new DateTime(2024, 3, 15) };
        order.Name.Should().Be("0042-2024-03-15");
    }

    [Fact]
    public void Order_Name_HandlesNullOrderDate()
    {
        var order = new RazorOrder { Id = 1, OrderDate = null };
        order.Name.Should().NotBeNullOrEmpty();
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

        o.Id.Should().Be(1);
        o.CustomerID.Should().Be("C1");
        o.Customer.Should().BeSameAs(c);
        o.EmployeeID.Should().Be(1);
        o.Employee.Should().BeSameAs(e);
        o.Items.Should().BeEmpty();
        o.ShipVia.Should().Be(1);
        o.ShipName.Should().Be("S");
        o.ShipAddress.Should().Be("A");
        o.ShipCity.Should().Be("C");
        o.ShipRegion.Should().Be("R");
        o.ShipPostalCode.Should().Be("P");
        o.ShipCountry.Should().Be("Co");
        o.Freight.Should().Be(10m);
        o.RequiredDate.Should().NotBeNull();
        o.ShippedDate.Should().NotBeNull();
    }

    [Fact]
    public void Employee_FullName_CombinesFirstAndLast()
    {
        var emp = new RazorEmployee { FirstName = "John", LastName = "Doe" };
        emp.FullName.Should().Be("John Doe");
    }

    [Fact]
    public void Employee_FullName_EmptyFirstName()
    {
        var emp = new RazorEmployee { FirstName = "", LastName = "Doe" };
        emp.FullName.Should().Be("Doe");
    }

    [Fact]
    public void Employee_FullName_NullFirstName()
    {
        var emp = new RazorEmployee { FirstName = null, LastName = "Doe" };
        emp.FullName.Should().Be("Doe");
    }

    [Fact]
    public void Employee_FullName_NullLastName()
    {
        var emp = new RazorEmployee { FirstName = "John", LastName = null };
        emp.FullName.Should().Contain("John");
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
        emp.TitleOfCourtesy.Should().Be("Mr.");
        emp.BirthDate.Should().NotBeNull();
        emp.HireDate.Should().NotBeNull();
        emp.Address.Should().Be("A");
        emp.City.Should().Be("C");
        emp.Region.Should().Be("R");
        emp.PostalCode.Should().Be("P");
        emp.Country.Should().Be("Co");
        emp.HomePhone.Should().Be("H");
        emp.Extension.Should().Be("E");
        emp.Photo.Should().HaveCount(1);
        emp.PhotoPath.Should().Be("/p");
        emp.Notes.Should().Be("N");
        emp.ReportsTo.Should().Be(2);
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
        p.Category.Should().BeSameAs(cat);
        p.Supplier.Should().BeSameAs(sup);
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
        c.Description.Should().Be("D");
        c.Picture.Should().HaveCount(1);
    }

    [Fact]
    public void OrderDetail_AllProperties()
    {
        var order = new RazorOrder { Id = 1 };
        var product = new RazorProduct { Id = 1 };
        var d = new RazorOrderDetail
        {
            OrderID = 1, Order = order, ProductID = 1, Product = product,
            UnitPrice = 10m, Quantity = 5, Discount = 0.1f
        };
        d.OrderID.Should().Be(1);
        d.Order.Should().BeSameAs(order);
        d.ProductID.Should().Be(1);
        d.Product.Should().BeSameAs(product);
        d.UnitPrice.Should().Be(10m);
        d.Quantity.Should().Be(5);
        d.Discount.Should().BeApproximately(0.1f, 0.001f);
    }

    [Fact]
    public void Shipper_AllProperties()
    {
        var s = new RazorShipper { Id = 1, CompanyName = "S", Phone = "P" };
        s.Id.Should().Be(1);
        s.CompanyName.Should().Be("S");
        s.Phone.Should().Be("P");
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
        s.CompanyName.Should().Be("S");
        s.ContactName.Should().Be("C");
        s.HomePage.Should().Be("H");
    }
}

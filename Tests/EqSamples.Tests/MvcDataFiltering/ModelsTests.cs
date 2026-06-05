extern alias MvcDataFiltering;

using FluentAssertions;
using MvcOrder = MvcDataFiltering::EqDemo.Models.Order;
using MvcCustomer = MvcDataFiltering::EqDemo.Models.Customer;
using MvcEmployee = MvcDataFiltering::EqDemo.Models.Employee;
using MvcProduct = MvcDataFiltering::EqDemo.Models.Product;
using MvcCategory = MvcDataFiltering::EqDemo.Models.Category;
using MvcOrderDetail = MvcDataFiltering::EqDemo.Models.OrderDetail;
using MvcShipper = MvcDataFiltering::EqDemo.Models.Shipper;
using MvcSupplier = MvcDataFiltering::EqDemo.Models.Supplier;
using MvcErrorViewModel = MvcDataFiltering::EqDemo.Models.ErrorViewModel;

namespace EqSamples.Tests.MvcDataFiltering;

public class ModelsTests
{
    [Fact]
    public void Order_Name_FormatsCorrectly()
    {
        var order = new MvcOrder
        {
            Id = 42,
            OrderDate = new DateTime(2024, 3, 15)
        };

        order.Name.Should().Be("0042-2024-03-15");
    }

    [Fact]
    public void Order_Name_HandlesNullOrderDate()
    {
        var order = new MvcOrder { Id = 1, OrderDate = null };
        order.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Order_Properties_SetAndGetCorrectly()
    {
        var customer = new MvcCustomer { Id = "CUST1" };
        var employee = new MvcEmployee { Id = 1 };
        var order = new MvcOrder
        {
            Id = 100,
            OrderDate = new DateTime(2024, 1, 1),
            RequiredDate = new DateTime(2024, 1, 15),
            ShippedDate = new DateTime(2024, 1, 10),
            Freight = 25.50m,
            CustomerID = "CUST1",
            Customer = customer,
            EmployeeID = 1,
            Employee = employee,
            Items = new List<MvcOrderDetail>(),
            ShipVia = 2,
            ShipName = "Test Ship",
            ShipAddress = "123 Test St",
            ShipCity = "TestCity",
            ShipRegion = "TR",
            ShipPostalCode = "12345",
            ShipCountry = "US"
        };

        order.Id.Should().Be(100);
        order.OrderDate.Should().Be(new DateTime(2024, 1, 1));
        order.RequiredDate.Should().Be(new DateTime(2024, 1, 15));
        order.ShippedDate.Should().Be(new DateTime(2024, 1, 10));
        order.Freight.Should().Be(25.50m);
        order.CustomerID.Should().Be("CUST1");
        order.Customer.Should().BeSameAs(customer);
        order.EmployeeID.Should().Be(1);
        order.Employee.Should().BeSameAs(employee);
        order.Items.Should().BeEmpty();
        order.ShipVia.Should().Be(2);
        order.ShipName.Should().Be("Test Ship");
        order.ShipAddress.Should().Be("123 Test St");
        order.ShipCity.Should().Be("TestCity");
        order.ShipRegion.Should().Be("TR");
        order.ShipPostalCode.Should().Be("12345");
        order.ShipCountry.Should().Be("US");
    }

    [Fact]
    public void Employee_FullName_CombinesFirstAndLastName()
    {
        var emp = new MvcEmployee { FirstName = "John", LastName = "Doe" };
        emp.FullName.Should().Be("John Doe");
    }

    [Fact]
    public void Employee_FullName_HandlesFirstNameOnly()
    {
        var emp = new MvcEmployee { FirstName = "John", LastName = null };
        emp.FullName.Should().Be("John ");
    }

    [Fact]
    public void Employee_FullName_HandlesEmptyFirstName()
    {
        var emp = new MvcEmployee { FirstName = "", LastName = "Doe" };
        emp.FullName.Should().Be("Doe");
    }

    [Fact]
    public void Employee_FullName_HandlesNullFirstName()
    {
        var emp = new MvcEmployee { FirstName = null, LastName = "Doe" };
        emp.FullName.Should().Be("Doe");
    }

    [Fact]
    public void Employee_Properties_SetAndGetCorrectly()
    {
        var manager = new MvcEmployee { Id = 2 };
        var emp = new MvcEmployee
        {
            Id = 1,
            FirstName = "Jane",
            LastName = "Smith",
            Title = "Sales Rep",
            TitleOfCourtesy = "Ms.",
            BirthDate = new DateTime(1990, 6, 15),
            HireDate = new DateTime(2020, 1, 1),
            Address = "456 Elm St",
            City = "Portland",
            Region = "OR",
            PostalCode = "97201",
            Country = "USA",
            HomePhone = "555-0100",
            Extension = "1234",
            Photo = new byte[] { 1, 2, 3 },
            PhotoPath = "/photos/jane.jpg",
            Notes = "Some notes",
            ReportsTo = 2,
            Manager = manager,
            Orders = new List<MvcOrder>()
        };

        emp.Id.Should().Be(1);
        emp.FirstName.Should().Be("Jane");
        emp.LastName.Should().Be("Smith");
        emp.Title.Should().Be("Sales Rep");
        emp.TitleOfCourtesy.Should().Be("Ms.");
        emp.BirthDate.Should().Be(new DateTime(1990, 6, 15));
        emp.HireDate.Should().Be(new DateTime(2020, 1, 1));
        emp.Address.Should().Be("456 Elm St");
        emp.City.Should().Be("Portland");
        emp.Region.Should().Be("OR");
        emp.PostalCode.Should().Be("97201");
        emp.Country.Should().Be("USA");
        emp.HomePhone.Should().Be("555-0100");
        emp.Extension.Should().Be("1234");
        emp.Photo.Should().Equal(new byte[] { 1, 2, 3 });
        emp.PhotoPath.Should().Be("/photos/jane.jpg");
        emp.Notes.Should().Be("Some notes");
        emp.ReportsTo.Should().Be(2);
        emp.Manager.Should().BeSameAs(manager);
        emp.Orders.Should().BeEmpty();
    }

    [Fact]
    public void Customer_Properties_SetAndGetCorrectly()
    {
        var cust = new MvcCustomer
        {
            Id = "ALFKI",
            CompanyName = "Alfreds Futterkiste",
            Address = "Obere Str. 57",
            City = "Berlin",
            Region = null,
            PostalCode = "12209",
            Country = "Germany",
            ContactName = "Maria Anders",
            ContactTitle = "Sales Representative",
            Phone = "030-0074321",
            Fax = "030-0076545"
        };

        cust.Id.Should().Be("ALFKI");
        cust.CompanyName.Should().Be("Alfreds Futterkiste");
        cust.Address.Should().Be("Obere Str. 57");
        cust.City.Should().Be("Berlin");
        cust.Region.Should().BeNull();
        cust.PostalCode.Should().Be("12209");
        cust.Country.Should().Be("Germany");
        cust.ContactName.Should().Be("Maria Anders");
        cust.ContactTitle.Should().Be("Sales Representative");
        cust.Phone.Should().Be("030-0074321");
        cust.Fax.Should().Be("030-0076545");
    }

    [Fact]
    public void Product_Properties_SetAndGetCorrectly()
    {
        var cat = new MvcCategory { Id = 1 };
        var sup = new MvcSupplier { Id = 1 };
        var product = new MvcProduct
        {
            Id = 10,
            Name = "Chai",
            QuantityPerUnit = "10 boxes x 20 bags",
            UnitPrice = 18.00m,
            UnitsInStock = 39,
            UnitsOnOrder = 0,
            ReorderLevel = 10,
            Discontinued = false,
            CategoryID = 1,
            Category = cat,
            SupplierID = 1,
            Supplier = sup
        };

        product.Id.Should().Be(10);
        product.Name.Should().Be("Chai");
        product.QuantityPerUnit.Should().Be("10 boxes x 20 bags");
        product.UnitPrice.Should().Be(18.00m);
        product.UnitsInStock.Should().Be(39);
        product.UnitsOnOrder.Should().Be(0);
        product.ReorderLevel.Should().Be(10);
        product.Discontinued.Should().BeFalse();
        product.CategoryID.Should().Be(1);
        product.Category.Should().BeSameAs(cat);
        product.SupplierID.Should().Be(1);
        product.Supplier.Should().BeSameAs(sup);
    }

    [Fact]
    public void Category_Properties_SetAndGetCorrectly()
    {
        var category = new MvcCategory
        {
            Id = 1,
            CategoryName = "Beverages",
            Description = "Soft drinks, coffees, teas",
            Picture = new byte[] { 0xFF, 0xD8 }
        };

        category.Id.Should().Be(1);
        category.CategoryName.Should().Be("Beverages");
        category.Description.Should().Be("Soft drinks, coffees, teas");
        category.Picture.Should().Equal(new byte[] { 0xFF, 0xD8 });
    }

    [Fact]
    public void OrderDetail_Properties_SetAndGetCorrectly()
    {
        var product = new MvcProduct { Id = 1 };
        var order = new MvcOrder { Id = 100 };
        var detail = new MvcOrderDetail
        {
            OrderID = 100,
            Order = order,
            ProductID = 1,
            Product = product,
            UnitPrice = 14.00m,
            Quantity = 12,
            Discount = 0.1f
        };

        detail.OrderID.Should().Be(100);
        detail.Order.Should().BeSameAs(order);
        detail.ProductID.Should().Be(1);
        detail.Product.Should().BeSameAs(product);
        detail.UnitPrice.Should().Be(14.00m);
        detail.Quantity.Should().Be(12);
        detail.Discount.Should().BeApproximately(0.1f, 0.001f);
    }

    [Fact]
    public void Shipper_Properties_SetAndGetCorrectly()
    {
        var shipper = new MvcShipper
        {
            Id = 1,
            CompanyName = "Speedy Express",
            Phone = "(503) 555-9831"
        };

        shipper.Id.Should().Be(1);
        shipper.CompanyName.Should().Be("Speedy Express");
        shipper.Phone.Should().Be("(503) 555-9831");
    }

    [Fact]
    public void Supplier_Properties_SetAndGetCorrectly()
    {
        var supplier = new MvcSupplier
        {
            Id = 1,
            CompanyName = "Exotic Liquids",
            ContactName = "Charlotte Cooper",
            ContactTitle = "Purchasing Manager",
            Address = "49 Gilbert St.",
            City = "London",
            Region = null,
            PostalCode = "EC1 4SD",
            Country = "UK",
            Phone = "(171) 555-2222",
            Fax = null,
            HomePage = null
        };

        supplier.Id.Should().Be(1);
        supplier.CompanyName.Should().Be("Exotic Liquids");
        supplier.ContactName.Should().Be("Charlotte Cooper");
        supplier.ContactTitle.Should().Be("Purchasing Manager");
        supplier.Address.Should().Be("49 Gilbert St.");
        supplier.City.Should().Be("London");
        supplier.Region.Should().BeNull();
        supplier.PostalCode.Should().Be("EC1 4SD");
        supplier.Country.Should().Be("UK");
        supplier.Phone.Should().Be("(171) 555-2222");
        supplier.Fax.Should().BeNull();
        supplier.HomePage.Should().BeNull();
    }

    [Fact]
    public void ErrorViewModel_ShowRequestId_ReturnsTrueWhenRequestIdIsSet()
    {
        var model = new MvcErrorViewModel { RequestId = "abc123" };
        model.ShowRequestId.Should().BeTrue();
    }

    [Fact]
    public void ErrorViewModel_ShowRequestId_ReturnsFalseWhenRequestIdIsNull()
    {
        var model = new MvcErrorViewModel { RequestId = null };
        model.ShowRequestId.Should().BeFalse();
    }

    [Fact]
    public void ErrorViewModel_ShowRequestId_ReturnsFalseWhenRequestIdIsEmpty()
    {
        var model = new MvcErrorViewModel { RequestId = string.Empty };
        model.ShowRequestId.Should().BeFalse();
    }
}

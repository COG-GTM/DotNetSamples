using EqDemo.Models;
using FluentAssertions;

namespace DotNetSamples.Tests.Models;

public class ProductTests
{
    [Fact]
    public void Properties_ShouldRoundTrip()
    {
        var product = new Product
        {
            Id = 1,
            Name = "Widget",
            SupplierID = 5,
            CategoryID = 3,
            QuantityPerUnit = "10 boxes x 20 bags",
            UnitPrice = 18.50m,
            UnitsInStock = 39,
            UnitsOnOrder = 10,
            ReorderLevel = 5,
            Discontinued = false
        };

        product.Id.Should().Be(1);
        product.Name.Should().Be("Widget");
        product.SupplierID.Should().Be(5);
        product.CategoryID.Should().Be(3);
        product.QuantityPerUnit.Should().Be("10 boxes x 20 bags");
        product.UnitPrice.Should().Be(18.50m);
        product.UnitsInStock.Should().Be(39);
        product.UnitsOnOrder.Should().Be(10);
        product.ReorderLevel.Should().Be(5);
        product.Discontinued.Should().BeFalse();
    }

    [Fact]
    public void DefaultValues_ShouldBeExpected()
    {
        var product = new Product();

        product.Id.Should().Be(0);
        product.Name.Should().BeNull();
        product.UnitPrice.Should().BeNull();
        product.UnitsInStock.Should().BeNull();
        product.UnitsOnOrder.Should().BeNull();
        product.ReorderLevel.Should().BeNull();
        product.Discontinued.Should().BeFalse();
    }

    [Fact]
    public void Supplier_ShouldBeSettable()
    {
        var supplier = new Supplier { Id = 1, CompanyName = "Acme Corp" };
        var product = new Product
        {
            Id = 1,
            SupplierID = 1,
            Supplier = supplier
        };

        product.Supplier.Should().NotBeNull();
        product.Supplier!.CompanyName.Should().Be("Acme Corp");
    }

    [Fact]
    public void Category_ShouldBeSettable()
    {
        var category = new Category { Id = 1, CategoryName = "Beverages" };
        var product = new Product
        {
            Id = 1,
            CategoryID = 1,
            Category = category
        };

        product.Category.Should().NotBeNull();
        product.Category!.CategoryName.Should().Be("Beverages");
    }

    [Fact]
    public void Discontinued_ShouldBeSettableToTrue()
    {
        var product = new Product { Discontinued = true };
        product.Discontinued.Should().BeTrue();
    }
}

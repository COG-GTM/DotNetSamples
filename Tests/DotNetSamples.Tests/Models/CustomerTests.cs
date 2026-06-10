using EqDemo.Models;
using FluentAssertions;

namespace DotNetSamples.Tests.Models;

public class CustomerTests
{
    [Fact]
    public void Properties_ShouldRoundTrip()
    {
        var customer = new Customer
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

        customer.Id.Should().Be("ALFKI");
        customer.CompanyName.Should().Be("Alfreds Futterkiste");
        customer.Address.Should().Be("Obere Str. 57");
        customer.City.Should().Be("Berlin");
        customer.Region.Should().BeNull();
        customer.PostalCode.Should().Be("12209");
        customer.Country.Should().Be("Germany");
        customer.ContactName.Should().Be("Maria Anders");
        customer.ContactTitle.Should().Be("Sales Representative");
        customer.Phone.Should().Be("030-0074321");
        customer.Fax.Should().Be("030-0076545");
    }

    [Fact]
    public void DefaultValues_ShouldBeNull()
    {
        var customer = new Customer();

        customer.Id.Should().BeNull();
        customer.CompanyName.Should().BeNull();
        customer.Address.Should().BeNull();
        customer.City.Should().BeNull();
        customer.Country.Should().BeNull();
        customer.ContactName.Should().BeNull();
        customer.ContactTitle.Should().BeNull();
        customer.Phone.Should().BeNull();
        customer.Fax.Should().BeNull();
    }
}

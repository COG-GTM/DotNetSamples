using EqDemo.Models;
using FluentAssertions;

namespace DotNetSamples.Tests.Models;

public class EmployeeTests
{
    [Fact]
    public void FullName_ShouldCombineFirstAndLastName()
    {
        var employee = new Employee
        {
            FirstName = "John",
            LastName = "Doe"
        };

        employee.FullName.Should().Be("John Doe");
    }

    [Fact]
    public void FullName_ShouldHandleOnlyFirstName()
    {
        var employee = new Employee
        {
            FirstName = "John",
            LastName = null
        };

        employee.FullName.Should().Be("John ");
    }

    [Fact]
    public void FullName_ShouldHandleEmptyFirstName()
    {
        var employee = new Employee
        {
            FirstName = "",
            LastName = "Doe"
        };

        employee.FullName.Should().Be("Doe");
    }

    [Fact]
    public void FullName_ShouldHandleNullFirstName()
    {
        var employee = new Employee
        {
            FirstName = null,
            LastName = "Doe"
        };

        employee.FullName.Should().Be("Doe");
    }

    [Fact]
    public void FullName_ShouldHandleBothNull()
    {
        var employee = new Employee
        {
            FirstName = null,
            LastName = null
        };

        employee.FullName.Should().BeNull();
    }

    [Fact]
    public void FullName_ShouldHandleBothEmpty()
    {
        var employee = new Employee
        {
            FirstName = "",
            LastName = ""
        };

        employee.FullName.Should().Be("");
    }

    [Fact]
    public void Properties_ShouldRoundTrip()
    {
        var hireDate = new DateTime(2020, 6, 15);
        var birthDate = new DateTime(1990, 3, 20);

        var employee = new Employee
        {
            Id = 1,
            FirstName = "Jane",
            LastName = "Smith",
            Title = "Sales Representative",
            TitleOfCourtesy = "Ms.",
            BirthDate = birthDate,
            HireDate = hireDate,
            Address = "456 Oak Ave",
            City = "Portland",
            Region = "OR",
            PostalCode = "97201",
            Country = "USA",
            HomePhone = "(503) 555-1234",
            Extension = "1234",
            PhotoPath = "/photos/jane.jpg",
            Notes = "Experienced sales rep",
            ReportsTo = 2
        };

        employee.Id.Should().Be(1);
        employee.Title.Should().Be("Sales Representative");
        employee.TitleOfCourtesy.Should().Be("Ms.");
        employee.BirthDate.Should().Be(birthDate);
        employee.HireDate.Should().Be(hireDate);
        employee.Address.Should().Be("456 Oak Ave");
        employee.City.Should().Be("Portland");
        employee.Region.Should().Be("OR");
        employee.PostalCode.Should().Be("97201");
        employee.Country.Should().Be("USA");
        employee.HomePhone.Should().Be("(503) 555-1234");
        employee.Extension.Should().Be("1234");
        employee.PhotoPath.Should().Be("/photos/jane.jpg");
        employee.Notes.Should().Be("Experienced sales rep");
        employee.ReportsTo.Should().Be(2);
    }

    [Fact]
    public void Manager_ShouldBeSettable()
    {
        var manager = new Employee { Id = 1, FirstName = "Boss", LastName = "Man" };
        var employee = new Employee
        {
            Id = 2,
            FirstName = "Worker",
            LastName = "Bee",
            ReportsTo = 1,
            Manager = manager
        };

        employee.Manager.Should().NotBeNull();
        employee.Manager!.FullName.Should().Be("Boss Man");
    }

    [Fact]
    public void Orders_ShouldBeSettable()
    {
        var employee = new Employee
        {
            Id = 1,
            FirstName = "Test",
            LastName = "User",
            Orders = new List<Order>
            {
                new Order { Id = 100 },
                new Order { Id = 101 }
            }
        };

        employee.Orders.Should().HaveCount(2);
    }
}

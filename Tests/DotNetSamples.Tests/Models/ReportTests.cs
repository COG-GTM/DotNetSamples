using EqDemo.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;

namespace DotNetSamples.Tests.Models;

public class ReportTests
{
    [Fact]
    public void Properties_ShouldRoundTrip()
    {
        var report = new Report
        {
            Id = "rpt-001",
            Name = "Monthly Sales",
            Description = "Monthly sales report for Q1",
            ModelId = "adhoc-reporting",
            QueryJson = "{\"columns\":[\"OrderDate\",\"Total\"]}",
            OwnerId = "user-123"
        };

        report.Id.Should().Be("rpt-001");
        report.Name.Should().Be("Monthly Sales");
        report.Description.Should().Be("Monthly sales report for Q1");
        report.ModelId.Should().Be("adhoc-reporting");
        report.QueryJson.Should().Contain("OrderDate");
        report.OwnerId.Should().Be("user-123");
    }

    [Fact]
    public void DefaultValues_ShouldBeNull()
    {
        var report = new Report();

        report.Id.Should().BeNull();
        report.Name.Should().BeNull();
        report.Description.Should().BeNull();
        report.ModelId.Should().BeNull();
        report.QueryJson.Should().BeNull();
        report.OwnerId.Should().BeNull();
        report.Owner.Should().BeNull();
    }

    [Fact]
    public void Owner_ShouldBeSettable()
    {
        var user = new IdentityUser { Id = "user-123", UserName = "test@test.com" };
        var report = new Report
        {
            Id = "rpt-002",
            OwnerId = user.Id,
            Owner = user
        };

        report.Owner.Should().NotBeNull();
        report.Owner!.UserName.Should().Be("test@test.com");
    }
}

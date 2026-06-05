extern alias RazorAdHoc;

using FluentAssertions;
using RazorReport = RazorAdHoc::EqDemo.Models.Report;

namespace EqSamples.Tests.RazorAdHocReporting;

public class ReportModelTests
{
    [Fact]
    public void Report_Properties_SetAndGetCorrectly()
    {
        var report = new RazorReport
        {
            Id = "report-001",
            Name = "Sales Report",
            Description = "Monthly sales overview",
            ModelId = "adhoc-reporting",
            QueryJson = "{\"columns\":[]}",
            OwnerId = "user-123"
        };

        report.Id.Should().Be("report-001");
        report.Name.Should().Be("Sales Report");
        report.Description.Should().Be("Monthly sales overview");
        report.ModelId.Should().Be("adhoc-reporting");
        report.QueryJson.Should().Be("{\"columns\":[]}");
        report.OwnerId.Should().Be("user-123");
        report.Owner.Should().BeNull();
    }

    [Fact]
    public void Report_DefaultValues_AreNull()
    {
        var report = new RazorReport();

        report.Id.Should().BeNull();
        report.Name.Should().BeNull();
        report.Description.Should().BeNull();
        report.ModelId.Should().BeNull();
        report.QueryJson.Should().BeNull();
        report.OwnerId.Should().BeNull();
        report.Owner.Should().BeNull();
    }
}

using EqDemo;
using EqDemo.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DotNetSamples.Tests.Services;

public class ReportCrudTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var ctx = new AppDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    private static Report MakeReport(string id, string name, string modelId, string ownerId)
    {
        return new Report
        {
            Id = id,
            Name = name,
            Description = "Test description",
            ModelId = modelId,
            QueryJson = "{}",
            OwnerId = ownerId
        };
    }

    [Fact]
    public async Task AddAndRetrieve()
    {
        using var context = CreateContext();

        var report = MakeReport("r1", "Sales Report", "adhoc-reporting", "user-1");
        context.Reports.Add(report);
        await context.SaveChangesAsync();

        var retrieved = await context.Reports.FindAsync("r1");
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Sales Report");
        retrieved.OwnerId.Should().Be("user-1");
    }

    [Fact]
    public async Task UpdateReport()
    {
        using var context = CreateContext();

        var report = MakeReport("r1", "Original", "m1", "user-1");
        context.Reports.Add(report);
        await context.SaveChangesAsync();

        report.Name = "Updated Report";
        report.QueryJson = "{\"updated\":true}";
        context.Reports.Update(report);
        await context.SaveChangesAsync();

        var retrieved = await context.Reports.FindAsync("r1");
        retrieved!.Name.Should().Be("Updated Report");
        retrieved.QueryJson.Should().Contain("updated");
    }

    [Fact]
    public async Task DeleteReport()
    {
        using var context = CreateContext();

        var report = MakeReport("r1", "To Delete", "m1", "user-1");
        context.Reports.Add(report);
        await context.SaveChangesAsync();

        context.Reports.Remove(report);
        await context.SaveChangesAsync();

        var retrieved = await context.Reports.FindAsync("r1");
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task FilterByOwner()
    {
        using var context = CreateContext();

        context.Reports.AddRange(
            MakeReport("r1", "Report A", "m1", "user-1"),
            MakeReport("r2", "Report B", "m1", "user-1"),
            MakeReport("r3", "Report C", "m1", "user-2")
        );
        await context.SaveChangesAsync();

        var user1Reports = await context.Reports
            .Where(r => r.OwnerId == "user-1")
            .ToListAsync();

        user1Reports.Should().HaveCount(2);
        user1Reports.Should().AllSatisfy(r => r.OwnerId.Should().Be("user-1"));
    }

    [Fact]
    public async Task FilterByModelId()
    {
        using var context = CreateContext();

        context.Reports.AddRange(
            MakeReport("r1", "Report A", "model-1", "user-1"),
            MakeReport("r2", "Report B", "model-2", "user-1"),
            MakeReport("r3", "Report C", "model-1", "user-1")
        );
        await context.SaveChangesAsync();

        var model1Reports = await context.Reports
            .Where(r => r.ModelId == "model-1")
            .OrderBy(r => r.Name)
            .ToListAsync();

        model1Reports.Should().HaveCount(2);
        model1Reports[0].Name.Should().Be("Report A");
    }

    [Fact]
    public async Task FilterByOwnerAndModel()
    {
        using var context = CreateContext();

        context.Reports.AddRange(
            MakeReport("r1", "A", "m1", "user-1"),
            MakeReport("r2", "B", "m2", "user-1"),
            MakeReport("r3", "C", "m1", "user-2"),
            MakeReport("r4", "D", "m1", "user-1")
        );
        await context.SaveChangesAsync();

        var results = await context.Reports
            .Where(r => r.OwnerId == "user-1" && r.ModelId == "m1")
            .OrderBy(r => r.Name)
            .ToListAsync();

        results.Should().HaveCount(2);
        results.Select(r => r.Name).Should().ContainInOrder("A", "D");
    }
}

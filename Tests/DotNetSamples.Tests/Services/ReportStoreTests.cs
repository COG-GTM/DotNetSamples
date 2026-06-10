using EqDemo;
using EqDemo.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Security.Claims;

namespace DotNetSamples.Tests.Services;

public class ReportStoreTests
{
    private static AppDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
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

    private static (IServiceProvider, AppDbContext) CreateServices(string userId, string dbName)
    {
        var context = CreateContext(dbName);
        context.Database.EnsureCreated();

        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "TestAuth"));

        var httpContext = new DefaultHttpContext { User = claims };
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var services = new ServiceCollection();
        services.AddSingleton(httpContextAccessor.Object);
        services.AddSingleton(context);
        var provider = services.BuildServiceProvider();

        return (provider, context);
    }

    [Fact]
    public async Task ReportsCrudOperations_AddAndRetrieve()
    {
        var dbName = Guid.NewGuid().ToString();
        var (_, context) = CreateServices("user-1", dbName);

        var report = MakeReport("r1", "Sales Report", "adhoc-reporting", "user-1");
        context.Reports.Add(report);
        await context.SaveChangesAsync();

        var retrieved = await context.Reports.FindAsync("r1");
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Sales Report");
        retrieved.OwnerId.Should().Be("user-1");
    }

    [Fact]
    public async Task ReportsCrudOperations_UpdateReport()
    {
        var dbName = Guid.NewGuid().ToString();
        var (_, context) = CreateServices("user-1", dbName);

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
    public async Task ReportsCrudOperations_DeleteReport()
    {
        var dbName = Guid.NewGuid().ToString();
        var (_, context) = CreateServices("user-1", dbName);

        var report = MakeReport("r1", "To Delete", "m1", "user-1");
        context.Reports.Add(report);
        await context.SaveChangesAsync();

        context.Reports.Remove(report);
        await context.SaveChangesAsync();

        var retrieved = await context.Reports.FindAsync("r1");
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task ReportsFiltering_ByOwner()
    {
        var dbName = Guid.NewGuid().ToString();
        var (_, context) = CreateServices("user-1", dbName);

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
    public async Task ReportsFiltering_ByModelId()
    {
        var dbName = Guid.NewGuid().ToString();
        var (_, context) = CreateServices("user-1", dbName);

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
    public async Task ReportsFiltering_ByOwnerAndModel()
    {
        var dbName = Guid.NewGuid().ToString();
        var (_, context) = CreateServices("user-1", dbName);

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

    [Fact]
    public void HttpContextAccessor_ShouldResolveFromServices()
    {
        var (provider, _) = CreateServices("test-user", Guid.NewGuid().ToString());

        var accessor = provider.GetService<IHttpContextAccessor>();
        accessor.Should().NotBeNull();
        accessor!.HttpContext.Should().NotBeNull();
        accessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be("test-user");
    }
}

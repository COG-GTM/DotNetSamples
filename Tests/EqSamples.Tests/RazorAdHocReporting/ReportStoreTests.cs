extern alias RazorAdHoc;

using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RazorAppDbContext = RazorAdHoc::EqDemo.AppDbContext;
using RazorReport = RazorAdHoc::EqDemo.Models.Report;
using RazorReportStore = RazorAdHoc::EqDemo.ReportStore;

namespace EqSamples.Tests.RazorAdHocReporting;

public class ReportStoreTests
{
    private RazorAppDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<RazorAppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new RazorAppDbContext(options);
    }

    private (RazorReportStore store, RazorAppDbContext context) CreateStoreWithUser(string userId, string? dbName = null)
    {
        dbName ??= $"ReportStore_{Guid.NewGuid()}";
        var context = CreateContext(dbName);

        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(a => a.HttpContext).Returns(httpContext);

        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(sp => sp.GetService(typeof(IHttpContextAccessor)))
            .Returns(httpContextAccessor.Object);
        serviceProvider.Setup(sp => sp.GetService(typeof(RazorAppDbContext)))
            .Returns(context);

        var store = new RazorReportStore(serviceProvider.Object);
        return (store, context);
    }

    [Fact]
    public async Task GetAllQueriesAsync_ReturnsOnlyUserReports()
    {
        var dbName = $"ReportStore_{Guid.NewGuid()}";

        using var setupContext = CreateContext(dbName);
        setupContext.Reports.AddRange(
            new RazorReport { Id = "r1", OwnerId = "user1", ModelId = "m1", Name = "R1" },
            new RazorReport { Id = "r2", OwnerId = "user2", ModelId = "m1", Name = "R2" },
            new RazorReport { Id = "r3", OwnerId = "user1", ModelId = "m1", Name = "R3" }
        );
        await setupContext.SaveChangesAsync();

        var (store, context) = CreateStoreWithUser("user1", dbName);
        using (context)
        {
            var result = await store.GetAllQueriesAsync("m1");
            result.Should().HaveCount(2);
        }
    }

    [Fact]
    public async Task GetAllQueriesAsync_ReturnsEmptyForNoMatches()
    {
        var (store, context) = CreateStoreWithUser("user1");
        using (context)
        {
            var result = await store.GetAllQueriesAsync("nonexistent");
            result.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task RemoveQueryAsync_RemovesExistingReport()
    {
        var dbName = $"ReportStore_{Guid.NewGuid()}";

        using var setupContext = CreateContext(dbName);
        setupContext.Reports.Add(new RazorReport { Id = "r1", OwnerId = "user1", ModelId = "m1", Name = "R1" });
        await setupContext.SaveChangesAsync();

        var (store, context) = CreateStoreWithUser("user1", dbName);
        using (context)
        {
            var removed = await store.RemoveQueryAsync("m1", "r1");
            removed.Should().BeTrue();

            (await context.Reports.FindAsync("r1")).Should().BeNull();
        }
    }

    [Fact]
    public async Task RemoveQueryAsync_ReturnsFalseForNonexistentReport()
    {
        var (store, context) = CreateStoreWithUser("user1");
        using (context)
        {
            var removed = await store.RemoveQueryAsync("m1", "nonexistent");
            removed.Should().BeFalse();
        }
    }

    [Fact]
    public async Task RemoveQueryAsync_ReturnsFalseForOtherUsersReport()
    {
        var dbName = $"ReportStore_{Guid.NewGuid()}";

        using var setupContext = CreateContext(dbName);
        setupContext.Reports.Add(new RazorReport { Id = "r1", OwnerId = "user2", ModelId = "m1", Name = "R1" });
        await setupContext.SaveChangesAsync();

        var (store, context) = CreateStoreWithUser("user1", dbName);
        using (context)
        {
            var removed = await store.RemoveQueryAsync("m1", "r1");
            removed.Should().BeFalse();
        }
    }

    [Fact]
    public void Constructor_ThrowsWhenServiceMissing()
    {
        var sp = new Mock<IServiceProvider>();
        sp.Setup(s => s.GetService(typeof(IHttpContextAccessor))).Returns(null!);

        Action act = () => new RazorReportStore(sp.Object);

        act.Should().Throw<InvalidOperationException>();
    }
}

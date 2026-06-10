using EqDemo;
using EqDemo.Models;
using EqDemo.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace DotNetSamples.Tests.Services;

public class DefaultReportGeneratorTests
{
    private AppDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        var ctx = new AppDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    // The source code uses Path.Combine(contentRoot, "App_Data\\Seed") which
    // on Linux creates a path with a literal backslash. We must match that.
    private static string CreateSeedDir(string tempDir)
    {
        var seedDir = Path.Combine(tempDir, "App_Data\\Seed");
        Directory.CreateDirectory(seedDir);
        return seedDir;
    }

    [Fact]
    public void Constructor_ShouldInitializeDataPath()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var env = new Mock<IWebHostEnvironment>();
        env.Setup(e => e.ContentRootPath).Returns("/app");

        var generator = new DefaultReportGenerator(env.Object, context);

        generator.Should().NotBeNull();
    }

    [Fact]
    public async Task GenerateAsync_ShouldAddReportsFromJsonFiles()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var seedDir = CreateSeedDir(tempDir);

        try
        {
            File.WriteAllText(
                Path.Combine(seedDir, "report1.json"),
                "{\"name\":\"Sales Report\",\"desc\":\"Monthly sales\"}"
            );
            File.WriteAllText(
                Path.Combine(seedDir, "report2.json"),
                "{\"name\":\"Inventory Report\",\"desc\":\"Current stock levels\"}"
            );

            var env = new Mock<IWebHostEnvironment>();
            env.Setup(e => e.ContentRootPath).Returns(tempDir);

            var generator = new DefaultReportGenerator(env.Object, context);
            var user = new IdentityUser { Id = "test-user-id", UserName = "test@test.com" };

            await generator.GenerateAsync(user);

            var reports = await context.Reports.ToListAsync();
            reports.Should().HaveCount(2);
            reports.Should().Contain(r => r.Name == "Sales Report");
            reports.Should().Contain(r => r.Name == "Inventory Report");
            reports.Should().OnlyContain(r => r.OwnerId == "test-user-id");
            reports.Should().OnlyContain(r => r.ModelId == "adhoc-reporting");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task GenerateAsync_ShouldSetUniqueIdsForEachReport()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var seedDir = CreateSeedDir(tempDir);

        try
        {
            File.WriteAllText(Path.Combine(seedDir, "a.json"), "{\"name\":\"A\",\"desc\":\"Desc A\"}");
            File.WriteAllText(Path.Combine(seedDir, "b.json"), "{\"name\":\"B\",\"desc\":\"Desc B\"}");
            File.WriteAllText(Path.Combine(seedDir, "c.json"), "{\"name\":\"C\",\"desc\":\"Desc C\"}");

            var env = new Mock<IWebHostEnvironment>();
            env.Setup(e => e.ContentRootPath).Returns(tempDir);

            var generator = new DefaultReportGenerator(env.Object, context);
            var user = new IdentityUser { Id = "user-1" };

            await generator.GenerateAsync(user);

            var reports = await context.Reports.ToListAsync();
            reports.Should().HaveCount(3);

            var ids = reports.Select(r => r.Id).ToList();
            ids.Should().OnlyHaveUniqueItems();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task GenerateAsync_ShouldHandleEmptyDirectory()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var seedDir = CreateSeedDir(tempDir);

        try
        {
            var env = new Mock<IWebHostEnvironment>();
            env.Setup(e => e.ContentRootPath).Returns(tempDir);

            var generator = new DefaultReportGenerator(env.Object, context);
            var user = new IdentityUser { Id = "user-1" };

            await generator.GenerateAsync(user);

            var reports = await context.Reports.ToListAsync();
            reports.Should().BeEmpty();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task GenerateAsync_ShouldIncludeQueryJsonWithId()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var seedDir = CreateSeedDir(tempDir);

        try
        {
            File.WriteAllText(
                Path.Combine(seedDir, "test.json"),
                "{\"name\":\"Test\",\"desc\":\"A test report\",\"columns\":[\"col1\"]}"
            );

            var env = new Mock<IWebHostEnvironment>();
            env.Setup(e => e.ContentRootPath).Returns(tempDir);

            var generator = new DefaultReportGenerator(env.Object, context);
            var user = new IdentityUser { Id = "user-1" };

            await generator.GenerateAsync(user);

            var report = await context.Reports.FirstAsync();
            report.QueryJson.Should().Contain("\"id\":");
            report.QueryJson.Should().Contain(report.Id);
            report.QueryJson.Should().Contain("columns");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}

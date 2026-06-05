extern alias RazorAdHoc;

using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using RazorAppDbContext = RazorAdHoc::EqDemo.AppDbContext;
using RazorDefaultReportGenerator = RazorAdHoc::EqDemo.Services.DefaultReportGenerator;

namespace EqSamples.Tests.RazorAdHocReporting;

public class DefaultReportGeneratorTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _seedDir;

    public DefaultReportGeneratorTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"EqTest_{Guid.NewGuid()}");
        // Production code uses: Path.Combine(env.ContentRootPath, $"App_Data\\Seed")
        // On Linux this produces a literal backslash path, so recreate that exact path
        _seedDir = Path.Combine(_tempDir, $"App_Data\\Seed");
        Directory.CreateDirectory(_seedDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private RazorAppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RazorAppDbContext>()
            .UseInMemoryDatabase($"ReportGen_{Guid.NewGuid()}")
            .Options;
        return new RazorAppDbContext(options);
    }

    [Fact]
    public async Task GenerateAsync_CreatesReportsFromJsonFiles()
    {
        File.WriteAllText(Path.Combine(_seedDir, "report1.json"),
            "{\"name\": \"Report 1\", \"desc\": \"Desc 1\"}");
        File.WriteAllText(Path.Combine(_seedDir, "report2.json"),
            "{\"name\": \"Report 2\", \"desc\": \"Desc 2\"}");

        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.ContentRootPath).Returns(_tempDir);

        using var context = CreateContext();
        var generator = new RazorDefaultReportGenerator(mockEnv.Object, context);
        var user = new IdentityUser { Id = "test-user-123" };

        await generator.GenerateAsync(user);

        var reports = await context.Reports.ToListAsync();
        reports.Should().HaveCount(2);
        reports.Should().Contain(r => r.Name == "Report 1");
        reports.Should().Contain(r => r.Name == "Report 2");
        reports.Should().OnlyContain(r => r.OwnerId == "test-user-123");
        reports.Should().OnlyContain(r => r.ModelId == "adhoc-reporting");
    }

    [Fact]
    public async Task GenerateAsync_NoJsonFiles_NoReportsCreated()
    {
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.ContentRootPath).Returns(_tempDir);

        using var context = CreateContext();
        var generator = new RazorDefaultReportGenerator(mockEnv.Object, context);
        var user = new IdentityUser { Id = "user-1" };

        await generator.GenerateAsync(user);

        (await context.Reports.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task GenerateAsync_SetsUniqueIds()
    {
        File.WriteAllText(Path.Combine(_seedDir, "a.json"), "{\"name\":\"A\"}");
        File.WriteAllText(Path.Combine(_seedDir, "b.json"), "{\"name\":\"B\"}");

        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.ContentRootPath).Returns(_tempDir);

        using var context = CreateContext();
        var generator = new RazorDefaultReportGenerator(mockEnv.Object, context);
        await generator.GenerateAsync(new IdentityUser { Id = "u1" });

        var reports = await context.Reports.ToListAsync();
        reports.Select(r => r.Id).Distinct().Should().HaveCount(2);
    }
}

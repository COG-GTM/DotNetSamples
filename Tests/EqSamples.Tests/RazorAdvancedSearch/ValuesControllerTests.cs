extern alias RazorAdvSearch;

using FluentAssertions;
using RazorValuesController = RazorAdvSearch::EqDemo.AspNetCoreRazor.AdvancedSearch.ValuesController;

namespace EqSamples.Tests.RazorAdvancedSearch;

public class ValuesControllerTests
{
    [Fact]
    public void GetList1_ReturnsThreeItems()
    {
        var controller = new RazorValuesController();

        var result = controller.GetList1();

        result.Should().HaveCount(3);
    }

    [Fact]
    public void GetList1_ItemsAreNotNull()
    {
        var controller = new RazorValuesController();

        var result = controller.GetList1().ToList();

        foreach (var item in result)
        {
            item.Should().NotBeNull();
        }
    }
}

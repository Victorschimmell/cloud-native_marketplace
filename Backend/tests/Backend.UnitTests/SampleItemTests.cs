using Backend.Domain.Entities;

namespace Backend.UnitTests;

public class SampleItemTests
{
    [Fact]
    public void CanCreateSampleItemWithName()
    {
        var item = new SampleItem
        {
            Name = "Example"
        };

        Assert.Equal("Example", item.Name);
        Assert.Null(item.Description);
    }
}

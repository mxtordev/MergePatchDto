using System.Text.Json;

namespace MergePatchDto.Tests;

public class NestedAndArraySemanticsTests
{
    [Fact]
    public void NestedObjectIsTreatedAsProvidedReplacementValue()
    {
        var target = new Event
        {
            Location = new Address { City = "Old" }
        };

        var patch = JsonSerializer.Deserialize<UpdateEventPatch>("""{ "location": { "city": "New" } }""", JsonOptions.CamelCase)!;

        patch.ApplyTo(target);

        Assert.True(patch.Has.Location);
        Assert.Equal("New", target.Location?.City);
        Assert.Equal(2, target.LocationSetCount);
    }

    [Fact]
    public void ArraysAreTreatedAsReplacementValues()
    {
        var target = new Event
        {
            Tags = ["old"]
        };

        var patch = JsonSerializer.Deserialize<UpdateEventPatch>("""{ "tags": ["new", "featured"] }""", JsonOptions.CamelCase)!;

        patch.ApplyTo(target);

        Assert.True(patch.Has.Tags);
        Assert.Equal(["new", "featured"], target.Tags);
        Assert.Equal(2, target.TagsSetCount);
    }
}

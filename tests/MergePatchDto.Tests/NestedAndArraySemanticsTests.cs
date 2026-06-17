using System.Text.Json;

namespace MergePatchDto.Tests;

public class NestedAndArraySemanticsTests
{
    [Fact]
    public void NestedObjectIsTreatedAsProvidedReplacementValue()
    {
        var target = new Document
        {
            Location = new Address { City = "Old" }
        };

        var patch = JsonSerializer.Deserialize<UpdateDocumentPatch>("""{ "location": { "city": "New" } }""", JsonOptions.CamelCase)!;

        patch.ApplyTo(target);

        Assert.True(patch.Has.Location);
        Assert.Equal("New", target.Location?.City);
        Assert.Equal(2, target.LocationSetCount);
    }

    [Fact]
    public void ArraysAreTreatedAsReplacementValues()
    {
        var target = new Document
        {
            Tags = ["old"]
        };

        var patch = JsonSerializer.Deserialize<UpdateDocumentPatch>("""{ "tags": ["new", "featured"] }""", JsonOptions.CamelCase)!;

        patch.ApplyTo(target);

        Assert.True(patch.Has.Tags);
        Assert.Equal(["new", "featured"], target.Tags);
        Assert.Equal(2, target.TagsSetCount);
    }
}

using System.Text.Json;

namespace MergePatchDto.Tests;

public class PresenceTrackingTests
{
    [Fact]
    public void EmptyObjectLeavesPropertiesUnprovided()
    {
        var patch = JsonSerializer.Deserialize<UpdateDocumentPatch>("{}")!;

        Assert.False(patch.Has.Name);
        Assert.Null(patch.Name);
    }

    [Fact]
    public void ExplicitValueMarksPropertyProvided()
    {
        var patch = JsonSerializer.Deserialize<UpdateDocumentPatch>("""{ "name": "New title" }""", JsonOptions.CamelCase)!;

        Assert.True(patch.Has.Name);
        Assert.Equal("New title", patch.Name);
    }
}

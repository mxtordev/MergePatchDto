using System.Text.Json;

namespace MergePatchDto.Tests;

public class PresenceTrackingTests
{
    [Fact]
    public void EmptyObjectLeavesPropertiesUnprovided()
    {
        var patch = JsonSerializer.Deserialize<UpdateEventPatch>("{}")!;

        Assert.False(patch.Has.Name);
        Assert.False(patch.WasProvided(nameof(UpdateEventPatch.Name)));
        Assert.Null(patch.Name);
    }

    [Fact]
    public void ExplicitValueMarksPropertyProvided()
    {
        var patch = JsonSerializer.Deserialize<UpdateEventPatch>("""{ "name": "NM 2026" }""", JsonOptions.CamelCase)!;

        Assert.True(patch.Has.Name);
        Assert.True(patch.WasProvided(nameof(UpdateEventPatch.Name)));
        Assert.Equal("NM 2026", patch.Name);
    }
}

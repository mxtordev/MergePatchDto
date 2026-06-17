using System.Reflection;
using System.Text.Json;

namespace MergePatchDto.Tests;

public class TargetAgnosticModeTests
{
    [Fact]
    public void TargetAgnosticPatchTracksProvidedProperties()
    {
        var patch = JsonSerializer.Deserialize<PresenceOnlyPatch>(
            """{ "name": null, "externalName": "public-name", "priorityDelta": 2 }""",
            JsonOptions.CamelCase)!;

        Assert.True(patch.Has.Name);
        Assert.True(patch.Has.ExternalName);
        Assert.True(patch.Has.PriorityDelta);
        Assert.Null(patch.Name);
        Assert.Equal("public-name", patch.ExternalName);
        Assert.Equal(2, patch.PriorityDelta);
    }

    [Fact]
    public void TargetAgnosticPatchLeavesOmittedPropertiesUnprovided()
    {
        var patch = JsonSerializer.Deserialize<PresenceOnlyPatch>("{}")!;

        Assert.False(patch.Has.Name);
        Assert.False(patch.Has.ExternalName);
        Assert.False(patch.Has.PriorityDelta);
    }

    [Fact]
    public void TargetAgnosticPatchDoesNotGenerateApplyTo()
    {
        var applyToMethods = typeof(PresenceOnlyPatch)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(method => method.Name == "ApplyTo")
            .ToArray();

        Assert.Empty(applyToMethods);
    }
}

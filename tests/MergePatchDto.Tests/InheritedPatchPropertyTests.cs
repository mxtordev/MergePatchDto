using System.Text.Json;
using System.Text.Json.Serialization;
using MergePatch;

namespace MergePatchDto.Tests;

public class InheritedPatchPropertyTests
{
    [Fact]
    public void InheritedPatchPropertiesAreTrackedAndApplied()
    {
        var patch = JsonSerializer.Deserialize<InheritedPersonPatch>(
            """{ "name": "Ada", "email": "ada@example.test" }""",
            JsonOptions.CamelCase)!;
        var target = new InheritedPerson();

        patch.ApplyTo(target);

        Assert.True(patch.Has.Name);
        Assert.True(patch.Has.Email);
        Assert.Equal("Ada", patch.Name);
        Assert.Equal("ada@example.test", patch.Email);
        Assert.Equal("Ada", target.Name);
        Assert.Equal("ada@example.test", target.Email);
    }

    [Fact]
    public void StrictUnknownPropertyHandlingAcceptsInheritedPatchProperties()
    {
        var patch = JsonSerializer.Deserialize<StrictInheritedPersonPatch>(
            """{ "name": "Ada" }""",
            JsonOptions.CamelCase)!;

        Assert.True(patch.Has.Name);
        Assert.Equal("Ada", patch.Name);
    }

    [Fact]
    public void InheritedPatchPropertiesKeepJsonAndMappingAttributes()
    {
        var patch = JsonSerializer.Deserialize<InheritedMappedPatch>(
            """{ "display_name": "Ada" }""",
            JsonOptions.CamelCase)!;
        var target = new InheritedMappedTarget();

        patch.ApplyTo(target);

        Assert.True(patch.Has.DisplayName);
        Assert.Equal("Ada", patch.DisplayName);
        Assert.Equal("Ada", target.Name);
    }
}

public class InheritedPersonPatchBase
{
    public string? Name { get; set; }
}

public sealed class InheritedPerson
{
    public string? Name { get; set; }

    public string? Email { get; set; }
}

[MergePatch(typeof(InheritedPerson))]
public partial class InheritedPersonPatch : InheritedPersonPatchBase
{
    public string? Email { get; set; }
}

[MergePatch(typeof(InheritedPerson), UnknownPropertyHandling = UnknownPropertyHandling.Reject)]
public partial class StrictInheritedPersonPatch : InheritedPersonPatchBase
{
}

public class InheritedMappedPatchBase
{
    [JsonPropertyName("display_name")]
    [PatchTo(nameof(InheritedMappedTarget.Name))]
    public string? DisplayName { get; set; }
}

public sealed class InheritedMappedTarget
{
    public string? Name { get; set; }
}

[MergePatch(typeof(InheritedMappedTarget))]
public partial class InheritedMappedPatch : InheritedMappedPatchBase
{
}

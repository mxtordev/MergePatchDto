using System.Text.Json;

namespace MergePatchDto.Tests;

public class NullableNullSemanticsTests
{
    [Fact]
    public void ExplicitNullForNonNullableReferenceThrows()
    {
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<NonNullableReferencePatch>("""{ "name": null }""", JsonOptions.CamelCase));
    }

    [Fact]
    public void ExplicitNullForNullableReferenceMarksPropertyProvided()
    {
        var patch = JsonSerializer.Deserialize<NullableReferencePatch>("""{ "name": null }""", JsonOptions.CamelCase)!;

        Assert.True(patch.Has.Name);
        Assert.Null(patch.Name);
    }

    [Fact]
    public void ExplicitNullForObliviousReferenceMarksPropertyProvided()
    {
        var patch = JsonSerializer.Deserialize<ObliviousReferencePatch>("""{ "name": null }""", JsonOptions.CamelCase)!;

        Assert.True(patch.Has.Name);
        Assert.Null(patch.Name);
    }

    [Fact]
    public void MissingNullablePropertyStaysUnprovidedEvenThoughValueIsNull()
    {
        var patch = JsonSerializer.Deserialize<UpdateDocumentPatch>("""{ "name": "New title" }""", JsonOptions.CamelCase)!;

        Assert.False(patch.Has.Description);
        Assert.Null(patch.Description);
    }
}

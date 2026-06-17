using System.Text.Json;

namespace MergePatchDto.Tests;

public class NullableNullSemanticsTests
{
    [Fact]
    public void ExplicitNullMarksPropertyProvided()
    {
        var patch = JsonSerializer.Deserialize<UpdateDocumentPatch>("""{ "description": null }""", JsonOptions.CamelCase)!;

        Assert.True(patch.Has.Description);
        Assert.Null(patch.Description);
    }

    [Fact]
    public void MissingNullablePropertyStaysUnprovidedEvenThoughValueIsNull()
    {
        var patch = JsonSerializer.Deserialize<UpdateDocumentPatch>("""{ "name": "New title" }""", JsonOptions.CamelCase)!;

        Assert.False(patch.Has.Description);
        Assert.Null(patch.Description);
    }
}

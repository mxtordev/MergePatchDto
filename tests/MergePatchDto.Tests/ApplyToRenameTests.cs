using System.Text.Json;

namespace MergePatchDto.Tests;

public class ApplyToRenameTests
{
    [Fact]
    public void ApplyToUsesPatchToMapping()
    {
        var target = new Document
        {
            Name = "Old",
            Summary = "old-url"
        };

        var patch = JsonSerializer.Deserialize<UpdateDocumentPatch>("""{ "description": null }""", JsonOptions.CamelCase)!;

        patch.ApplyTo(target);

        Assert.Equal("Old", target.Name);
        Assert.Null(target.Summary);
    }
}

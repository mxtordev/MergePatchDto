using System.Text.Json;

namespace MergePatchDto.Tests;

public class ApplyToIgnoreTests
{
    [Fact]
    public void ApplyToSkipsPatchIgnoredProperty()
    {
        var target = new Document
        {
            Name = "Old"
        };

        var patch = JsonSerializer.Deserialize<UpdateDocumentPatch>("""{ "requestId": "abc" }""", JsonOptions.CamelCase)!;

        patch.ApplyTo(target);

        Assert.True(patch.Has.RequestId);
        Assert.Equal("abc", patch.RequestId);
        Assert.Equal("Old", target.Name);
    }
}

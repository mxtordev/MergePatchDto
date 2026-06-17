using System.Text.Json;

namespace MergePatchDto.Tests;

public class JsonPropertyNameTests
{
    [Fact]
    public void ConverterUsesJsonPropertyNameAttribute()
    {
        var patch = JsonSerializer.Deserialize<JsonPropertyNamePatch>("""{ "summary_text": "Custom summary" }""")!;

        Assert.True(patch.Has.Description);
        Assert.Equal("Custom summary", patch.Description);
    }
}

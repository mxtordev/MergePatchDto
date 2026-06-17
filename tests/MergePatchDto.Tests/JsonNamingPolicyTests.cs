using System.Text.Json;

namespace MergePatchDto.Tests;

public class JsonNamingPolicyTests
{
    [Fact]
    public void ConverterUsesConfiguredPropertyNamingPolicy()
    {
        var patch = JsonSerializer.Deserialize<JsonNamingPatch>("""{ "displayName": "Conference" }""", JsonOptions.CamelCase)!;

        Assert.True(patch.Has.DisplayName);
        Assert.Equal("Conference", patch.DisplayName);
    }
}

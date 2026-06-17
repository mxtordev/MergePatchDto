using System.Text.Json;

namespace MergePatchDto.Tests;

public class JsonIgnoreTests
{
    [Fact]
    public void ConverterIgnoresJsonIgnoredProperties()
    {
        var patch = JsonSerializer.Deserialize<JsonIgnorePatch>("""{ "name": "Visible", "clientMutationId": "ignored" }""", JsonOptions.CamelCase)!;

        Assert.True(patch.Has.Name);
        Assert.Equal("Visible", patch.Name);
        Assert.Null(patch.ClientMutationId);
    }
}

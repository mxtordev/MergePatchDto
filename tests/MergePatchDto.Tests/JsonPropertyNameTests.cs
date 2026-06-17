using System.Text.Json;

namespace MergePatchDto.Tests;

public class JsonPropertyNameTests
{
    [Fact]
    public void ConverterUsesJsonPropertyNameAttribute()
    {
        var patch = JsonSerializer.Deserialize<JsonPropertyNamePatch>("""{ "terms_url": "https://terms.example" }""")!;

        Assert.True(patch.Has.TermsAndConditionsUrl);
        Assert.Equal("https://terms.example", patch.TermsAndConditionsUrl);
    }
}

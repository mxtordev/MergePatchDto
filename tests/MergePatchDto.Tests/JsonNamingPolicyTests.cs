using System.Text.Json;
using MergePatch;

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

    [Fact]
    public void ConverterRejectsCamelCaseJsonNameCollisions()
    {
        var readException = Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<CamelCaseJsonNameCollisionPatch>("""{ "urlValue": "abc" }""", JsonOptions.CamelCase));

        Assert.Contains("duplicate JSON property name", readException.Message);

        var patch = JsonSerializer.Deserialize<CamelCaseJsonNameCollisionPatch>(
            """{ "URLValue": "abc", "UrlValue": "def" }""")!;

        var writeException = Assert.Throws<JsonException>(() =>
            JsonSerializer.Serialize(patch, JsonOptions.CamelCase));

        Assert.Contains("duplicate JSON property name", writeException.Message);
    }

    [Fact]
    public void ConverterRejectsCaseInsensitiveJsonNameCollisions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var readException = Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<CaseInsensitiveJsonNameCollisionPatch>("""{ "name": "abc" }""", options));

        Assert.Contains("duplicate JSON property name", readException.Message);

        var patch = JsonSerializer.Deserialize<CaseInsensitiveJsonNameCollisionPatch>(
            """{ "Name": "abc", "NAME": "def" }""")!;

        var writeException = Assert.Throws<JsonException>(() =>
            JsonSerializer.Serialize(patch, options));

        Assert.Contains("duplicate JSON property name", writeException.Message);
    }
}

[MergePatch]
public partial class CamelCaseJsonNameCollisionPatch
{
    public string? URLValue { get; set; }

    public string? UrlValue { get; set; }
}

[MergePatch]
public partial class CaseInsensitiveJsonNameCollisionPatch
{
    public string? Name { get; set; }

    public string? NAME { get; set; }
}

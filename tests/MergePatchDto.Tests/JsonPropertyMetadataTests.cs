using System.Text.Json;
using System.Text.Json.Serialization;

namespace MergePatchDto.Tests;

public class JsonPropertyMetadataTests
{
    [Fact]
    public void ConverterUsesPropertyLevelJsonConverterOnRead()
    {
        var patch = JsonSerializer.Deserialize<PropertyMetadataPatch>("""{ "code": "abc" }""", JsonOptions.CamelCase)!;

        Assert.True(patch.Has.Code);
        Assert.Equal("read:abc", patch.Code);
    }

    [Fact]
    public void ConverterUsesPropertyLevelJsonConverterOnWrite()
    {
        var patch = JsonSerializer.Deserialize<PropertyMetadataPatch>("""{ "code": "abc" }""", JsonOptions.CamelCase)!;

        var json = JsonSerializer.Serialize(patch, JsonOptions.CamelCase);

        Assert.Contains(""""code":"write:read:abc"""", json);
    }

    [Fact]
    public void ConverterUsesPropertyLevelJsonNumberHandlingOnRead()
    {
        var patch = JsonSerializer.Deserialize<PropertyMetadataPatch>("""{ "count": "12" }""", JsonOptions.CamelCase)!;

        Assert.True(patch.Has.Count);
        Assert.Equal(12, patch.Count);
    }

    [Fact]
    public void ConverterUsesPropertyLevelJsonNumberHandlingOnWrite()
    {
        var patch = JsonSerializer.Deserialize<PropertyMetadataPatch>("""{ "count": "12" }""", JsonOptions.CamelCase)!;

        var json = JsonSerializer.Serialize(patch, JsonOptions.CamelCase);

        Assert.Contains(""""count":"12"""", json);
    }

    [Fact]
    public void ConverterStillUsesGlobalJsonConverters()
    {
        var options = new JsonSerializerOptions(JsonOptions.CamelCase);
        options.Converters.Add(new JsonStringEnumConverter());

        var patch = JsonSerializer.Deserialize<PropertyMetadataPatch>("""{ "status": "Published" }""", options)!;

        Assert.True(patch.Has.Status);
        Assert.Equal(PropertyMetadataStatus.Published, patch.Status);

        var json = JsonSerializer.Serialize(patch, options);

        Assert.Contains(""""status":"Published"""", json);
    }
}

using System.Reflection;
using System.Text.Json;

namespace MergePatchDto.Tests;

public class JsonIgnoreTests
{
    [Fact]
    public void ConverterIgnoresJsonIgnoredProperties()
    {
        var patch = JsonSerializer.Deserialize<JsonIgnorePatch>("""{ "name": "Visible", "requestId": "ignored" }""", JsonOptions.CamelCase)!;

        Assert.True(patch.Has.Name);
        Assert.Equal("Visible", patch.Name);
        Assert.Null(patch.RequestId);
        Assert.DoesNotContain(
            typeof(JsonIgnorePatch.ProvidedFields).GetProperties(BindingFlags.Instance | BindingFlags.Public),
            property => property.Name == nameof(JsonIgnorePatch.RequestId));
    }

    [Fact]
    public void ConverterReadsJsonIgnoreWhenWritingNullProperties()
    {
        var patch = JsonSerializer.Deserialize<ConditionalJsonIgnorePatch>("""{ "nullableText": null }""", JsonOptions.CamelCase)!;

        Assert.True(patch.Has.NullableText);
        Assert.Null(patch.NullableText);
    }

    [Fact]
    public void ConverterReadsJsonIgnoreWhenWritingDefaultProperties()
    {
        var patch = JsonSerializer.Deserialize<ConditionalJsonIgnorePatch>("""{ "count": 0 }""", JsonOptions.CamelCase)!;

        Assert.True(patch.Has.Count);
        Assert.Equal(0, patch.Count);
    }

    [Fact]
    public void ConverterReadsJsonIgnoreNeverProperties()
    {
        var patch = JsonSerializer.Deserialize<ConditionalJsonIgnorePatch>("""{ "includedText": "included" }""", JsonOptions.CamelCase)!;

        Assert.True(patch.Has.IncludedText);
        Assert.Equal("included", patch.IncludedText);
    }

    [Fact]
    public void RejectUnknownAllowsJsonIgnoreWhenWritingProperties()
    {
        var patch = JsonSerializer.Deserialize<StrictConditionalJsonIgnorePatch>("""{ "nullableText": "known" }""", JsonOptions.CamelCase)!;

        Assert.True(patch.Has.NullableText);
        Assert.Equal("known", patch.NullableText);
    }
}

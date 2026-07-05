using System.Text.Json;

namespace MergePatchDto.Tests;

public class UnknownPropertyTests
{
    [Fact]
    public void UnknownPropertiesAreIgnoredByDefault()
    {
        var patch = JsonSerializer.Deserialize<UnknownIgnoredPatch>("""{ "Name": "Known", "Other": "ignored" }""")!;

        Assert.True(patch.Has.Name);
        Assert.Equal("Known", patch.Name);
    }

    [Fact]
    public void UnknownNestedPropertiesAreSkippedByDefault()
    {
        var patch = JsonSerializer.Deserialize<UnknownIgnoredPatch>(
            """{ "Other": { "Nested": [1, { "Deep": true }] }, "Name": "Known" }""")!;

        Assert.True(patch.Has.Name);
        Assert.Equal("Known", patch.Name);
    }

    [Fact]
    public void UnknownPropertiesCanBeRejected()
    {
        var exception = Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<UnknownRejectedPatch>("""{ "Name": "Known", "Other": "rejected" }"""));

        Assert.Contains("Unknown JSON property", exception.Message);
    }

    [Fact]
    public void RejectedUnknownPropertiesFailBeforeReadingValue()
    {
        var exception = Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<UnknownRejectedPatch>("{ \"Other\": "));

        Assert.Contains("Unknown JSON property 'Other'", exception.Message);
    }
}

using System.Text.Json;
using Microsoft.CodeAnalysis;

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

    [Fact]
    public void ReportsErrorForDuplicateExplicitJsonPropertyNames()
    {
        var diagnostics = DiagnosticTestHelper.GetDiagnostics(
            """
            using MergePatch;
            using System.Text.Json.Serialization;

            [MergePatch]
            public partial class Patch
            {
                [JsonPropertyName("name")]
                public string? DisplayName { get; set; }

                [JsonPropertyName("name")]
                public string? LegalName { get; set; }
            }
            """);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "MPD017" && diagnostic.Severity == DiagnosticSeverity.Error);
    }
}

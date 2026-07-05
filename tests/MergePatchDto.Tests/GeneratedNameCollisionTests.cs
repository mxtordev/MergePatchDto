using System.Text.Json;
using System.Text.Json.Serialization;
using MergePatch;
using Microsoft.CodeAnalysis;

namespace MergePatchDto.Tests;

public class GeneratedNameCollisionTests
{
    [Fact]
    public void UserMembersWithPreviousGeneratedPresenceNamesDoNotBreakTracking()
    {
        var patch = JsonSerializer.Deserialize<PreviousGeneratedMemberNamePatch>("""{ "name": "Draft" }""", JsonOptions.CamelCase)!;

        Assert.True(patch.Has.Name);
        Assert.Equal("Draft", patch.Name);
        Assert.Equal(7, patch.ExistingProvidedValue);
        Assert.Equal(11, patch.ExistingGeneratedProvidedValue);

        var json = JsonSerializer.Serialize(patch, JsonOptions.CamelCase);

        Assert.Contains(""""name":"Draft"""", json);
    }

    [Fact]
    public void UserTypeWithPreviousGeneratedConverterNameDoesNotBreakConverterGeneration()
    {
        var patch = JsonSerializer.Deserialize<PreviousGeneratedConverterNamePatch>(
            """{ "code": "abc", "name": "Draft" }""",
            JsonOptions.CamelCase)!;

        Assert.True(patch.Has.Code);
        Assert.True(patch.Has.Name);
        Assert.Equal("read:abc", patch.Code);
        Assert.Equal("Draft", patch.Name);

        var json = JsonSerializer.Serialize(patch, JsonOptions.CamelCase);

        Assert.Contains(""""code":"write:read:abc"""", json);
        Assert.Contains(""""name":"Draft"""", json);
    }

    [Fact]
    public void ReportsErrorWhenPatchDtoDefinesGeneratedHasApi()
    {
        var diagnostics = DiagnosticTestHelper.GetDiagnostics(
            """
            using MergePatch;

            [MergePatch]
            public partial class Patch
            {
                public string? Name { get; set; }

                public bool Has => false;
            }
            """);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "MPD016" && diagnostic.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void ReportsErrorWhenPatchDtoDefinesGeneratedApplyToApi()
    {
        var diagnostics = DiagnosticTestHelper.GetDiagnostics(
            """
            using MergePatch;

            public class Target
            {
                public string? Name { get; set; }
            }

            [MergePatch(typeof(Target))]
            public partial class Patch
            {
                public string? Name { get; set; }

                public void ApplyTo(Target target)
                {
                }
            }
            """);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "MPD016" && diagnostic.Severity == DiagnosticSeverity.Error);
    }
}

[MergePatch]
public partial class PreviousGeneratedMemberNamePatch
{
    private readonly int _provided = 7;
    private readonly int __mergePatchDtoProvided = 11;

    public string? Name { get; set; }

    public int ExistingProvidedValue => _provided;

    public int ExistingGeneratedProvidedValue => __mergePatchDtoProvided;

    private bool __MergePatchWasProvided(string propertyName)
    {
        return false;
    }

    private void __MergePatchMarkProvided(string propertyName)
    {
    }

    private void __MergePatchClearProvided()
    {
    }

    private bool __MergePatchDtoWasProvided(string propertyName)
    {
        return false;
    }

    private void __MergePatchDtoMarkProvided(string propertyName)
    {
    }

    private void __MergePatchDtoClearProvided()
    {
    }
}

[MergePatch]
public partial class PreviousGeneratedConverterNamePatch
{
    [JsonConverter(typeof(PrefixStringConverter))]
    public string? Code { get; set; }

    public string? Name { get; set; }
}

public sealed class PreviousGeneratedConverterNamePatchJsonConverter
{
    private static string __MergePatchGetJsonName(
        string clrName,
        string? explicitJsonName,
        JsonSerializerOptions options)
    {
        return explicitJsonName ?? clrName;
    }

    private static bool __MergePatchJsonNameEquals(
        string actualName,
        string expectedName,
        JsonSerializerOptions options)
    {
        return actualName == expectedName;
    }

    private static JsonSerializerOptions __MergePatchGetCodeJsonOptions(JsonSerializerOptions options)
    {
        return options;
    }
}

public sealed class __MergePatchDtoPreviousGeneratedConverterNamePatchJsonConverter
{
}

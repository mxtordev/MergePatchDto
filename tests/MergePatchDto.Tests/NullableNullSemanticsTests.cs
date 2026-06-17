using System.Text.Json;

namespace MergePatchDto.Tests;

public class NullableNullSemanticsTests
{
    [Fact]
    public void ExplicitNullMarksPropertyProvided()
    {
        var patch = JsonSerializer.Deserialize<UpdateEventPatch>("""{ "termsAndConditionsUrl": null }""", JsonOptions.CamelCase)!;

        Assert.True(patch.Has.TermsAndConditionsUrl);
        Assert.Null(patch.TermsAndConditionsUrl);
    }

    [Fact]
    public void MissingNullablePropertyStaysUnprovidedEvenThoughValueIsNull()
    {
        var patch = JsonSerializer.Deserialize<UpdateEventPatch>("""{ "name": "NM 2026" }""", JsonOptions.CamelCase)!;

        Assert.False(patch.Has.TermsAndConditionsUrl);
        Assert.Null(patch.TermsAndConditionsUrl);
    }
}

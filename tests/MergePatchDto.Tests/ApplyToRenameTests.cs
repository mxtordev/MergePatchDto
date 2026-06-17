using System.Text.Json;

namespace MergePatchDto.Tests;

public class ApplyToRenameTests
{
    [Fact]
    public void ApplyToUsesPatchToMapping()
    {
        var target = new Event
        {
            Name = "Old",
            TermsUrl = "old-url"
        };

        var patch = JsonSerializer.Deserialize<UpdateEventPatch>("""{ "termsAndConditionsUrl": null }""", JsonOptions.CamelCase)!;

        patch.ApplyTo(target);

        Assert.Equal("Old", target.Name);
        Assert.Null(target.TermsUrl);
    }
}

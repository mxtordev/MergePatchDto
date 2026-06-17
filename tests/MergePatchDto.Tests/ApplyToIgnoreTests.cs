using System.Text.Json;

namespace MergePatchDto.Tests;

public class ApplyToIgnoreTests
{
    [Fact]
    public void ApplyToSkipsPatchIgnoredProperty()
    {
        var target = new Event
        {
            Name = "Old"
        };

        var patch = JsonSerializer.Deserialize<UpdateEventPatch>("""{ "clientMutationId": "abc" }""", JsonOptions.CamelCase)!;

        patch.ApplyTo(target);

        Assert.True(patch.Has.ClientMutationId);
        Assert.Equal("abc", patch.ClientMutationId);
        Assert.Equal("Old", target.Name);
    }
}

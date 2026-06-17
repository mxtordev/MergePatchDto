using System.Text.Json;

namespace MergePatchDto.Tests;

public class ApplyToConventionTests
{
    [Fact]
    public void ApplyToAssignsSameNamePropertyOnlyWhenProvided()
    {
        var target = new Event
        {
            Name = "Old",
            TermsUrl = "old-url"
        };

        var patch = JsonSerializer.Deserialize<UpdateEventPatch>("""{ "name": "New" }""", JsonOptions.CamelCase)!;

        patch.ApplyTo(target);

        Assert.Equal("New", target.Name);
        Assert.Equal("old-url", target.TermsUrl);
        Assert.Equal(2, target.NameSetCount);
        Assert.Equal(1, target.TermsUrlSetCount);
    }

    [Fact]
    public void ApplyToDoesNotCallSetterForMissingProperty()
    {
        var target = new Event
        {
            Name = "Old"
        };

        var before = target.NameSetCount;
        var patch = JsonSerializer.Deserialize<UpdateEventPatch>("{}")!;

        patch.ApplyTo(target);

        Assert.Equal("Old", target.Name);
        Assert.Equal(before, target.NameSetCount);
    }

    [Fact]
    public void PatchDtoCanGenerateMultipleTargetOverloads()
    {
        var patch = JsonSerializer.Deserialize<MultiTargetPatch>("""{ "Name": "Draft" }""")!;
        var eventTarget = new Event();
        var draftTarget = new EventDraft();

        patch.ApplyTo(eventTarget);
        patch.ApplyTo(draftTarget);

        Assert.Equal("Draft", eventTarget.Name);
        Assert.Equal("Draft", draftTarget.Name);
    }
}

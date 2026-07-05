using System.Text.Json;

namespace MergePatchDto.Tests;

public class ApplyToConventionTests
{
    [Fact]
    public void ApplyToAssignsSameNamePropertyOnlyWhenProvided()
    {
        var target = new Document
        {
            Name = "Old",
            Summary = "old-url"
        };

        var patch = JsonSerializer.Deserialize<UpdateDocumentPatch>("""{ "name": "New" }""", JsonOptions.CamelCase)!;

        patch.ApplyTo(target);

        Assert.Equal("New", target.Name);
        Assert.Equal("old-url", target.Summary);
        Assert.Equal(2, target.NameSetCount);
        Assert.Equal(1, target.SummarySetCount);
    }

    [Fact]
    public void ApplyToDoesNotCallSetterForMissingProperty()
    {
        var target = new Document
        {
            Name = "Old"
        };

        var before = target.NameSetCount;
        var patch = JsonSerializer.Deserialize<UpdateDocumentPatch>("{}")!;

        patch.ApplyTo(target);

        Assert.Equal("Old", target.Name);
        Assert.Equal(before, target.NameSetCount);
    }

    [Fact]
    public void PatchDtoCanTargetInterfaceImplementedByMultipleClasses()
    {
        var patch = JsonSerializer.Deserialize<InterfaceTargetPatch>("""{ "Name": "Shared" }""")!;
        var documentTarget = new NamedDocument();
        var draftTarget = new NamedDraft();

        patch.ApplyTo(documentTarget);
        patch.ApplyTo(draftTarget);

        Assert.Equal("Shared", documentTarget.Name);
        Assert.Equal("Shared", draftTarget.Name);
    }
}

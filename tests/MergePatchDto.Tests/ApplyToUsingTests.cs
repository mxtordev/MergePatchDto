using System.Text.Json;

namespace MergePatchDto.Tests;

public class ApplyToUsingTests
{
    [Fact]
    public void ApplyToCallsCustomStaticMethodOnlyWhenProvided()
    {
        var target = new Document();
        var patch = JsonSerializer.Deserialize<UpdateDocumentPatch>("""{ "priority": 12 }""", JsonOptions.CamelCase)!;

        patch.ApplyTo(target);

        Assert.Equal(12, target.Priority);
        Assert.Equal(1, target.PrioritySetCount);
    }

    [Fact]
    public void ApplyToDoesNotCallCustomStaticMethodWhenMissing()
    {
        var target = new Document();
        var patch = JsonSerializer.Deserialize<UpdateDocumentPatch>("{}")!;

        patch.ApplyTo(target);

        Assert.Null(target.Priority);
        Assert.Equal(0, target.PrioritySetCount);
    }

    [Fact]
    public void ApplyToSupportsPatchTargetValueCustomMethodSignature()
    {
        var target = new Document();
        var patch = JsonSerializer.Deserialize<InstanceUsingPatch>("""{ "priority": 25 }""", JsonOptions.CamelCase)!;

        patch.ApplyTo(target);

        Assert.Equal(25, target.Priority);
        Assert.Equal(1, target.PrioritySetCount);
    }
}

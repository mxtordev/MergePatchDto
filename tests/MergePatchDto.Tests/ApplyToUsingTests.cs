using System.Text.Json;

namespace MergePatchDto.Tests;

public class ApplyToUsingTests
{
    [Fact]
    public void ApplyToCallsCustomStaticMethodOnlyWhenProvided()
    {
        var target = new Event();
        var patch = JsonSerializer.Deserialize<UpdateEventPatch>("""{ "maxParticipants": 12 }""", JsonOptions.CamelCase)!;

        patch.ApplyTo(target);

        Assert.Equal(12, target.Capacity);
        Assert.Equal(1, target.CapacitySetCount);
    }

    [Fact]
    public void ApplyToDoesNotCallCustomStaticMethodWhenMissing()
    {
        var target = new Event();
        var patch = JsonSerializer.Deserialize<UpdateEventPatch>("{}")!;

        patch.ApplyTo(target);

        Assert.Null(target.Capacity);
        Assert.Equal(0, target.CapacitySetCount);
    }

    [Fact]
    public void ApplyToSupportsPatchTargetValueCustomMethodSignature()
    {
        var target = new Event();
        var patch = JsonSerializer.Deserialize<InstanceUsingPatch>("""{ "maxParticipants": 25 }""", JsonOptions.CamelCase)!;

        patch.ApplyTo(target);

        Assert.Equal(25, target.Capacity);
        Assert.Equal(1, target.CapacitySetCount);
    }
}

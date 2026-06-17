using System.Text.Json.Serialization;
using MergePatch;

namespace MergePatchDto.Tests;

public sealed class Event
{
    private string? _name;
    private string? _termsUrl;
    private int? _capacity;
    private Address? _location;
    private string[]? _tags;

    public int NameSetCount { get; private set; }

    public int TermsUrlSetCount { get; private set; }

    public int CapacitySetCount { get; private set; }

    public int LocationSetCount { get; private set; }

    public int TagsSetCount { get; private set; }

    public string? Name
    {
        get => _name;
        set
        {
            NameSetCount++;
            _name = value;
        }
    }

    public string? TermsUrl
    {
        get => _termsUrl;
        set
        {
            TermsUrlSetCount++;
            _termsUrl = value;
        }
    }

    public Address? Location
    {
        get => _location;
        set
        {
            LocationSetCount++;
            _location = value;
        }
    }

    public string[]? Tags
    {
        get => _tags;
        set
        {
            TagsSetCount++;
            _tags = value;
        }
    }

    public int? Capacity => _capacity;

    public void SetCapacity(int? value)
    {
        CapacitySetCount++;
        _capacity = value;
    }
}

public sealed class EventDraft
{
    public string? Name { get; set; }
}

public sealed class Address
{
    public string? City { get; set; }
}

[MergePatch(typeof(Event))]
public partial class UpdateEventPatch
{
    public string? Name { get; set; }

    [PatchTo(nameof(Event.TermsUrl))]
    public string? TermsAndConditionsUrl { get; set; }

    [PatchIgnore]
    public string? ClientMutationId { get; set; }

    [PatchUsing(nameof(ApplyCapacity))]
    public int? MaxParticipants { get; set; }

    public Address? Location { get; set; }

    public string[]? Tags { get; set; }

    private static void ApplyCapacity(Event target, int? value)
    {
        target.SetCapacity(value);
    }
}

[MergePatch]
[MergePatchTarget(typeof(Event))]
[MergePatchTarget(typeof(EventDraft))]
public partial class MultiTargetPatch
{
    public string? Name { get; set; }
}

[MergePatch]
public partial class PresenceOnlyPatch
{
    public string? Name { get; set; }

    [PatchTo("IgnoredBecauseNoTargetExists")]
    public string? ExternalName { get; set; }

    [PatchIgnore]
    [PatchUsing("IgnoredBecauseNoTargetExists")]
    public int? CapacityDelta { get; set; }
}

[MergePatch]
public partial class JsonNamingPatch
{
    public string? DisplayName { get; set; }
}

[MergePatch]
public partial class JsonPropertyNamePatch
{
    [JsonPropertyName("terms_url")]
    public string? TermsAndConditionsUrl { get; set; }
}

[MergePatch]
public partial class JsonIgnorePatch
{
    public string? Name { get; set; }

    [JsonIgnore]
    public string? ClientMutationId { get; set; }
}

[MergePatch]
public partial class UnknownIgnoredPatch
{
    public string? Name { get; set; }
}

[MergePatch(UnknownPropertyHandling = UnknownPropertyHandling.Reject)]
public partial class UnknownRejectedPatch
{
    public string? Name { get; set; }
}

[MergePatch(typeof(Event))]
public partial class InstanceUsingPatch
{
    [PatchUsing(nameof(ApplyCapacity))]
    public int? MaxParticipants { get; set; }

    private static void ApplyCapacity(InstanceUsingPatch patch, Event target, int? value)
    {
        if (patch.Has.MaxParticipants)
        {
            target.SetCapacity(value);
        }
    }
}

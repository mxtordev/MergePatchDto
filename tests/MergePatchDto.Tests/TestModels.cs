using System.Text.Json.Serialization;
using MergePatch;

namespace MergePatchDto.Tests;

public sealed class Document
{
    private string? _name;
    private string? _summary;
    private int? _priority;
    private Address? _location;
    private string[]? _tags;

    public int NameSetCount { get; private set; }

    public int SummarySetCount { get; private set; }

    public int PrioritySetCount { get; private set; }

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

    public string? Summary
    {
        get => _summary;
        set
        {
            SummarySetCount++;
            _summary = value;
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

    public int? Priority => _priority;

    public void SetPriority(int? value)
    {
        PrioritySetCount++;
        _priority = value;
    }
}

public sealed class DocumentDraft
{
    public string? Name { get; set; }
}

public sealed class Address
{
    public string? City { get; set; }
}

[MergePatch(typeof(Document))]
public partial class UpdateDocumentPatch
{
    public string? Name { get; set; }

    [PatchTo(nameof(Document.Summary))]
    public string? Description { get; set; }

    [PatchIgnore]
    public string? RequestId { get; set; }

    [PatchUsing(nameof(ApplyPriority))]
    public int? Priority { get; set; }

    public Address? Location { get; set; }

    public string[]? Tags { get; set; }

    private static void ApplyPriority(Document target, int? value)
    {
        target.SetPriority(value);
    }
}

[MergePatch]
[MergePatchTarget(typeof(Document))]
[MergePatchTarget(typeof(DocumentDraft))]
public partial class MultiTargetPatch
{
    public string? Name { get; set; }
}

[MergePatch]
public partial class PresenceOnlyPatch
{
    public string? Name { get; set; }

    public string? ExternalName { get; set; }

    public int? PriorityDelta { get; set; }
}

[MergePatch]
public partial class JsonNamingPatch
{
    public string? DisplayName { get; set; }
}

[MergePatch]
public partial class JsonPropertyNamePatch
{
    [JsonPropertyName("summary_text")]
    public string? Description { get; set; }
}

[MergePatch]
public partial class JsonIgnorePatch
{
    public string? Name { get; set; }

    [JsonIgnore]
    public string? RequestId { get; set; }
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

[MergePatch(typeof(Document))]
public partial class InstanceUsingPatch
{
    [PatchUsing(nameof(ApplyPriority))]
    public int? Priority { get; set; }

    private static void ApplyPriority(InstanceUsingPatch patch, Document target, int? value)
    {
        if (patch.Has.Priority)
        {
            target.SetPriority(value);
        }
    }
}

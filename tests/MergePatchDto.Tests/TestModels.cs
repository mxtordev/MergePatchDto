using System.Text.Json;
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

public interface INamedResource
{
    string? Name { get; set; }
}

public sealed class NamedDocument : INamedResource
{
    public string? Name { get; set; }
}

public sealed class NamedDraft : INamedResource
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

[MergePatch(typeof(INamedResource))]
public partial class InterfaceTargetPatch
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

public sealed class PrefixStringConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return "read:" + reader.GetString();
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue("write:" + value);
    }
}

public sealed class StatefulStringConverterAttribute : JsonConverterAttribute
{
    private readonly string _readPrefix;
    private readonly string _writePrefix;

    public StatefulStringConverterAttribute(string readPrefix, string writePrefix)
    {
        _readPrefix = readPrefix;
        _writePrefix = writePrefix;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert)
    {
        return new StatefulStringConverter(_readPrefix, _writePrefix);
    }
}

public sealed class StatefulStringConverter : JsonConverter<string?>
{
    private readonly string _readPrefix;
    private readonly string _writePrefix;

    public StatefulStringConverter(string readPrefix, string writePrefix)
    {
        _readPrefix = readPrefix;
        _writePrefix = writePrefix;
    }

    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return _readPrefix + reader.GetString();
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(_writePrefix + value);
    }
}

public enum PropertyMetadataStatus
{
    Draft,
    Published
}

[MergePatch]
public partial class PropertyMetadataPatch
{
    [JsonConverter(typeof(PrefixStringConverter))]
    public string? Code { get; set; }

    [StatefulStringConverter("state-read:", "state-write:")]
    public string? StatefulCode { get; set; }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public int Count { get; set; }

    public PropertyMetadataStatus Status { get; set; }
}

[MergePatch]
public partial class JsonIgnorePatch
{
    public string? Name { get; set; }

    [JsonIgnore]
    public string? RequestId { get; set; }
}

[MergePatch]
public partial class ConditionalJsonIgnorePatch
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? NullableText { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Count { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? IncludedText { get; set; }
}

[MergePatch(UnknownPropertyHandling = UnknownPropertyHandling.Reject)]
public partial class StrictConditionalJsonIgnorePatch
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? NullableText { get; set; }
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

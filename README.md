# MergePatchDto

MergePatchDto is a NuGet package for .NET APIs that want normal DTO-shaped PATCH request bodies, not JSON Patch operation arrays.

It gives PATCH endpoints:

- a typed allowlist of patchable fields
- explicit null vs missing tracking
- optional generated `ApplyTo` mapping

That lets a PATCH endpoint distinguish:

- missing property: leave the target unchanged
- explicit `null`: assign or clear the target value
- explicit value: assign the target value

Nested object properties are replacement values, not recursive document merges. Arrays are replacement values, not element-level merges.

## Install

```bash
dotnet add package MergePatchDto
```

## Quick Start

Define a partial patch DTO and point it at the type it updates:

```csharp
using MergePatch;

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

    private static void ApplyPriority(Document target, int? value)
    {
        target.SetPriority(value);
    }
}
```

Use it directly in an endpoint:

```csharp
public async Task<IActionResult> Patch(Guid id, UpdateDocumentPatch patch)
{
    var document = await db.Documents.FindAsync(id);
    if (document is null)
    {
        return NotFound();
    }

    patch.ApplyTo(document);

    await db.SaveChangesAsync();
    return NoContent();
}
```

`ApplyTo` only applies fields that were present in the JSON body. For example:

```json
{
  "description": null
}
```

This clears `Document.Summary` and leaves every omitted property unchanged.

## DTO-Shaped PATCH

`JsonPatchDocument<T>` is useful when clients need RFC 6902 operation documents with paths and operations. MergePatchDto is for endpoints where the public contract is a fixed DTO shape.

The patch DTO is the allowlist. Properties that are not on the patch DTO are not patchable through that endpoint, even if they exist on the target type.

```csharp
[MergePatch(typeof(Document))]
public partial class UpdateDocumentPatch
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}
```

The target type lets MergePatchDto generate typed mapping:

```csharp
patch.ApplyTo(document);
```

It does not mean every property on `Document` is patchable. The DTO remains the boundary.

## Presence Checks

The hard part of DTO-shaped PATCH is knowing what the client actually sent. After normal deserialization, a missing JSON property and an explicit `null` can both leave a CLR property as `null`.

The generated `Has` API exposes JSON presence, so validation and domain logic do not have to guess from nullable/default values:

```csharp
var has = patch.Has;

if (has.Name) entity.Name = patch.Name;
if (has.Description) entity.Summary = patch.Description;
if (has.Priority) entity.SetPriority(patch.Priority);
```

## Targetless Patches for Domain Logic

Generated `ApplyTo` is useful when patch properties map cleanly to a target type. When the update needs domain logic, omit the target type:

```csharp
using MergePatch;

[MergePatch]
public partial class UpdateDocumentPatch
{
    public string? Name { get; set; }
    public int? PriorityDelta { get; set; }
}
```

MergePatchDto still generates the JSON converter and `Has` API:

```csharp
var has = patch.Has;

if (has.Name)
    entity.Name = patch.Name;

if (has.PriorityDelta)
    entity.IncrementPriority(patch.PriorityDelta.GetValueOrDefault());
```

Add a target type when you want a typed generated `ApplyTo` method.

## Mapping Rules

By default, patch properties map to target properties with the same CLR name.

- `[PatchTo(nameof(Target.OtherName))]` maps a patch property to a differently named target property.
- `[PatchIgnore]` excludes client-only values from generated `ApplyTo`.
- `[PatchUsing(nameof(Method))]` calls custom domain logic when the patch property was provided.
- `[MergePatchTarget(typeof(OtherTarget))]` adds another typed `ApplyTo` overload.
- Targets can be interfaces. In that case, generated `ApplyTo` accepts the interface and maps only members declared on that interface.

Supported custom apply signatures are:

```csharp
private static void ApplyPriority(Document target, int? value)
private static void ApplyPriority(UpdateDocumentPatch patch, Document target, int? value)
```

Target-specific mapping attributes are invalid on targetless patch types and produce an error.

## JSON Behavior

MergePatchDto generates a `System.Text.Json` converter for each patch DTO.

The converter:

- tracks top-level JSON property presence
- stores presence by CLR property name
- respects `JsonPropertyNameAttribute`
- respects property-level `JsonConverterAttribute`
- respects property-level `JsonNumberHandlingAttribute`
- respects `JsonIgnoreAttribute` read behavior
- respects `JsonSerializerOptions.PropertyNamingPolicy`
- treats explicit `null` as provided

Unknown JSON properties are ignored by default. To reject them:

```csharp
[MergePatch(UnknownPropertyHandling = UnknownPropertyHandling.Reject)]
public partial class StrictPatch
{
    public string? Name { get; set; }
}
```

## Diagnostics

Invalid patch shapes and target-specific mappings fail during build with MergePatchDto diagnostics.

Common examples:

- patch DTO class is not `partial`
- target property does not exist
- target property setter is not accessible
- patch property type cannot be assigned to the target property type
- `[PatchUsing]` method is missing or has an unsupported signature
- a property combines conflicting mapping attributes

`MPD009` is reported when a targetless `[MergePatch]` DTO uses target-specific
mapping attributes such as `[PatchTo]`, `[PatchIgnore]`, or `[PatchUsing]`.
Targetless DTOs do not generate `ApplyTo`, so those attributes would be
misleading. Add a target type with `[MergePatch(typeof(Target))]` or
`[MergePatchTarget(typeof(Target))]`, or remove the target-specific attributes.

## Compatibility

MergePatchDto supports SDK-style projects built with the .NET 8 SDK or newer.
The runtime assembly targets `netstandard2.0`, but the source generator runs
inside the consumer's C# compiler, so the supported compiler floor is the .NET
8 SDK / Roslyn 4.8.

## Limitations

- MergePatchDto is DTO presence tracking for merge-patch-style endpoints, not a complete RFC 7396 implementation.
- Patch DTOs must be top-level, non-generic, non-abstract partial classes.
- Patch DTOs need an accessible parameterless constructor.
- `required` members are not supported.
- Presence tracking is top-level only.
- Nested object properties are replacement values and are assigned according to the property mapping.
- Arrays are replacement values, not partial merges.
- JSON Patch operation arrays, expression-tree mapping, wrapper properties, and DI-driven `ApplyTo` are not part of this package.

# MergePatchDto

MergePatchDto is a source-generator package for ergonomic merge-patch-style DTO presence tracking in ASP.NET Core.

It is not a full RFC 7396 JSON document merge engine. The package tracks which top-level DTO properties appeared in the JSON body and can generate typed assignment helpers for those properties.

It lets a PATCH endpoint distinguish:

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

## Presence Checks

The generated `Has` API stays useful when the update needs explicit domain logic:

```csharp
var has = patch.Has;

if (has.Name) entity.Name = patch.Name;
if (has.Description) entity.Summary = patch.Description;
if (has.Priority) entity.SetPriority(patch.Priority);
```

## Targetless Patches

If you only want JSON presence tracking, omit the target type:

```csharp
using MergePatch;

[MergePatch]
public partial class UpdateDocumentPatch
{
    public string? Name { get; set; }
    public int? PriorityDelta { get; set; }
}
```

MergePatchDto still generates the JSON converter and `Has` API. Add a target type when you want a typed generated `ApplyTo` method.

```csharp
var has = patch.Has;

if (has.Name)
    entity.Name = patch.Name;

if (has.PriorityDelta)
    entity.IncrementPriority(patch.PriorityDelta.GetValueOrDefault());
```

## Mapping Rules

By default, patch properties map to target properties with the same CLR name.

- `[PatchTo(nameof(Target.OtherName))]` maps a patch property to a differently named target property.
- `[PatchIgnore]` excludes client-only values from generated `ApplyTo`.
- `[PatchUsing(nameof(Method))]` calls custom domain logic when the patch property was provided.
- `[MergePatchTarget(typeof(OtherTarget))]` adds another typed `ApplyTo` overload.

Supported custom apply signatures are:

```csharp
private static void ApplyPriority(Document target, int? value)
private static void ApplyPriority(UpdateDocumentPatch patch, Document target, int? value)
```

Target-specific mapping attributes have no effect on targetless patch types and produce a warning.

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

Invalid patch shapes and mappings fail during build with MergePatchDto diagnostics.

Common examples:

- patch DTO class is not `partial`
- target property does not exist
- target property setter is not accessible
- patch property type cannot be assigned to the target property type
- `[PatchUsing]` method is missing or has an unsupported signature
- a property combines conflicting mapping attributes

## Limitations

- MergePatchDto is DTO presence tracking for merge-patch-style endpoints, not a complete RFC 7396 implementation.
- Patch DTOs must be top-level, non-generic, non-abstract partial classes.
- Patch DTOs need an accessible parameterless constructor.
- `required` members are not supported.
- Presence tracking is top-level only.
- Nested object properties are replacement values and are assigned according to the property mapping.
- Arrays are replacement values, not partial merges.
- JSON Patch operation arrays, expression-tree mapping, wrapper properties, and DI-driven `ApplyTo` are not part of this package.

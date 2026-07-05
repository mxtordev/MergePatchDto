# MergePatchDto

MergePatchDto is a NuGet package for .NET APIs that want DTO-shaped PATCH
request bodies while still knowing which JSON properties the client actually
sent.

Normal `System.Text.Json` deserialization cannot tell the difference between an
omitted nullable property and an explicit `null`. ASP.NET Core's
`Microsoft.AspNetCore.JsonPatch.JsonPatchDocument<T>` solves a different
problem with RFC 6902 operation arrays. MergePatchDto keeps the public contract
as an ordinary JSON object and adds generated presence tracking around it.

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
using MergePatchDtoSample.Api.Models;

[MergePatch(typeof(Person))]
public partial class UpdatePersonPatch
{
    public string Name { get; set; } = "";
    public string? Email { get; set; }
    public string? Bio { get; set; }
}
```

Use it directly in an endpoint:

```csharp
public IActionResult PatchWithGeneratedApplyTo(Guid id, UpdatePersonPatch patch)
{
    if (!people.TryGet(id, out var person))
    {
        return NotFound();
    }

    patch.ApplyTo(person);

    return NoContent();
}
```

`ApplyTo` only applies fields that were present in the JSON body. For example:

```json
{
  "bio": null
}
```

This clears `Person.Bio` and leaves every omitted property unchanged.

Use C# nullability to model whether a field may be set to `null`; presence is
tracked separately through the generated `Has` API. A property does not need to
be nullable just to be optional in the request body. Omitted fields are skipped
because `Has` is false. The sample patch uses nullable properties where the API
accepts explicit clearing.

For renamed target properties, accepted client metadata, or domain-specific
update methods, use the mapping attributes described below.

## When to Use MergePatchDto

Use MergePatchDto when the endpoint should accept a fixed DTO shape and update
only fields declared on that DTO.

The patch DTO is the allowlist. Properties that are not on the patch DTO are not patchable through that endpoint, even if they exist on the target type.

```csharp
[MergePatch(typeof(Person), UnknownPropertyHandling = UnknownPropertyHandling.Reject)]
public partial class StrictPersonPatch
{
    public string Name { get; set; } = "";
    public string? Email { get; set; }
}
```

The target type lets MergePatchDto generate typed mapping:

```csharp
patch.ApplyTo(person);
```

It does not mean every property on `Person` is patchable. The DTO remains the boundary.

## API Overview

Annotating a top-level partial class with `[MergePatch]` generates:

- a `Has` property with one boolean member per DTO property, using the CLR
  property name
- a `System.Text.Json` converter that records which top-level JSON properties
  were present
- `ApplyTo(Target target)` overloads for `[MergePatch(typeof(Target))]` and
  `[MergePatchTarget(typeof(Target))]`

Targetless `[MergePatch]` DTOs get `Has` and the JSON converter, but no
generated `ApplyTo` method.

## Presence Tracking

The generated `Has` API exposes JSON presence, so validation and domain logic
do not have to guess from nullable/default values:

```csharp
var has = patch.Has;

if (has.Name) person.Name = patch.Name;
if (has.Bio) person.Bio = patch.Bio;
if (has.Age) person.SetAge(patch.Age);
```

## Manual Updates Without ApplyTo

Generated `ApplyTo` is useful when patch properties map cleanly to a target type. When the update needs domain logic, omit the target type:

```csharp
using MergePatch;

[MergePatch]
public partial class ManualPersonPatch
{
    public bool? IsActive { get; set; }
    public string? AdminNote { get; set; }
}
```

MergePatchDto still generates the JSON converter and `Has` API:

```csharp
var has = patch.Has;

if (has.IsActive)
{
    person.IsActive = patch.IsActive.GetValueOrDefault();
    person.DeactivatedAt = person.IsActive ? null : DateTimeOffset.UtcNow;
}

if (has.AdminNote)
{
    person.AdminNote = patch.AdminNote;
}
```

Add a target type when you want a typed generated `ApplyTo` method.

## Mapping Attributes

By default, patch properties map to target properties with the same CLR name.
If a target property should not be patchable, leave it off the DTO.

- `[PatchTo(nameof(Target.OtherName))]` maps a patch property to a differently
  named target property.
- `[PatchIgnore]` is for accepted DTO properties that should still be
  deserialized and presence-tracked, but excluded from generated `ApplyTo`.
- `[PatchUsing(nameof(Method))]` calls custom domain logic when the patch
  property was provided.
- `[MergePatchTarget(typeof(OtherTarget))]` adds another typed `ApplyTo`
  overload.
- Targets can be interfaces. In that case, generated `ApplyTo` accepts the interface and maps only members declared on that interface.

```csharp
[MergePatch(typeof(Person))]
public partial class UpdatePersonPatch
{
    [JsonPropertyName("phone")]
    [PatchTo(nameof(Person.PhoneNumber))]
    public string? Phone { get; set; }

    [PatchIgnore]
    public string? RequestId { get; set; }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    [PatchUsing(nameof(ApplyAge))]
    public int? Age { get; set; }

    private static void ApplyAge(Person target, int? value)
    {
        target.SetAge(value);
    }
}
```

Supported custom apply signatures are:

```csharp
private static void ApplyAge(Person target, int? value)
private static void ApplyAge(UpdatePersonPatch patch, Person target, int? value)
```

Target-specific mapping attributes are invalid on targetless patch types and produce an error.

## JSON Deserialization

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
Add a target type or remove the target-specific attributes.

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

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
using YourApi.Models;

[MergePatch(typeof(Person))]
public partial class UpdatePersonPatch
{
    public string Name { get; set; } = "";
    public string? Email { get; set; }
    public string? Bio { get; set; }
}
```

Use it in a normal PATCH endpoint after loading the entity being updated:

```csharp
[HttpPatch("{id:guid}")]
public async Task<IActionResult> PatchPerson(
    Guid id,
    UpdatePersonPatch patch,
    CancellationToken cancellationToken)
{
    var person = await db.People.FindAsync([id], cancellationToken);

    if (person is null)
    {
        return NotFound();
    }

    patch.ApplyTo(person);

    await db.SaveChangesAsync(cancellationToken);

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

The same presence information is available directly when an endpoint needs
custom logic:

```csharp
if (patch.Has.Bio)
{
    person.Bio = patch.Bio;
}
```

Use C# nullability to model whether a field may be set to `null`; presence is
tracked separately through the generated `Has` API. A property does not need to
be nullable just to be optional in the request body. Omitted fields are skipped
because `Has` is false. This patch DTO uses nullable properties where the API
accepts explicit clearing.

For renamed target properties, accepted client metadata, or domain-specific
update methods, use the mapping attributes described below.

## When to Use MergePatchDto

Use MergePatchDto when the endpoint should accept a fixed DTO shape and update
only fields declared on that DTO.

The patch DTO is the allowlist. Properties that are not on the patch DTO are not patchable through that endpoint, even if they exist on the target type.

```csharp
[MergePatch(typeof(Person))]
public partial class PersonContactPatch
{
    public string Name { get; set; } = "";
    public string? Email { get; set; }
}
```

The target type lets MergePatchDto generate typed mapping:

```csharp
patch.ApplyTo(person);
```

If the same patch shape should apply to multiple concrete types, target a
shared interface. Generated `ApplyTo` accepts the interface and maps only
members declared on that interface.

It does not mean every property on `Person` is patchable. The DTO remains the boundary.

## Manual Updates

Generated `ApplyTo` is useful when patch properties map cleanly to a target type.
When validation or domain logic needs to act only on fields sent by the client,
use the generated `Has` API directly.

For manual update logic, omit the target type:

```csharp
using MergePatch;

[MergePatch]
public partial class ManualPersonPatch
{
    public bool? IsActive { get; set; }
    public string? AdminNote { get; set; }
}
```

MergePatchDto still generates the JSON converter and `Has` API, but no
`ApplyTo` method:

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
- Targets can be interfaces.

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

Target-specific mapping attributes are invalid on targetless patch types.
Invalid patch shapes and mappings fail at build time with `MPDxxx` diagnostics.

## JSON Behavior

MergePatchDto uses `System.Text.Json` for patch DTOs. It records top-level
property presence, including properties sent as `null`.

Unknown JSON properties are ignored by default. To reject them:

```csharp
[MergePatch(UnknownPropertyHandling = UnknownPropertyHandling.Reject)]
public partial class StrictPatch
{
    public string? Name { get; set; }
}
```

## Compatibility

MergePatchDto supports SDK-style projects built with the .NET 8 SDK or newer.
The runtime assembly targets `netstandard2.0`, but the source generator runs
inside the consumer's C# compiler, so the supported compiler floor is the .NET
8 SDK / Roslyn 4.8.

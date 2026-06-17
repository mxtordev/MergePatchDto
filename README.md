# MergePatchDto

MergePatchDto is a source-generator package for ergonomic merge-patch DTOs in ASP.NET Core.

It lets a PATCH endpoint distinguish:

- missing property: leave the target unchanged
- explicit `null`: assign or clear the target value
- explicit value: assign the target value

Install the `MergePatchDto` package, define a normal partial DTO, and call `ApplyTo`.

```csharp
using MergePatch;

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

    private static void ApplyCapacity(Event target, int? value)
    {
        target.SetCapacity(value);
    }
}
```

Use `[MergePatchTarget(typeof(OtherTarget))]` only when one patch DTO needs extra `ApplyTo` overloads.

If you do not want generated target assignment, omit the target type. MergePatchDto will still generate the JSON converter and presence API, but it will not generate an untyped or reflection-based `ApplyTo`:

```csharp
using MergePatch;

[MergePatch]
public partial class UpdateEventPatch
{
    public string? Name { get; set; }
    public int? CapacityDelta { get; set; }
}
```

```csharp
var has = patch.Has;

if (has.Name)
    entity.Name = patch.Name;

if (has.CapacityDelta)
    entity.IncrementCapacity(patch.CapacityDelta.GetValueOrDefault());
```

Target-specific mapping attributes such as `[PatchTo]`, `[PatchIgnore]`, and `[PatchUsing]` are only validated and applied when the patch type has a target. On targetless patch types, those attributes produce a warning because they have no generated `ApplyTo` effect.

```csharp
public async Task<IActionResult> Patch(Guid id, UpdateEventPatch patch)
{
    var ev = await db.Events.FindAsync(id);
    if (ev is null)
    {
        return NotFound();
    }

    patch.ApplyTo(ev);

    await db.SaveChangesAsync();
    return NoContent();
}
```

Manual fallback stays compact:

```csharp
var has = patch.Has;

if (has.Name) entity.Name = patch.Name;
if (has.Description) entity.Description = patch.Description;
if (has.TermsAndConditionsUrl) entity.TermsUrl = patch.TermsAndConditionsUrl;
```

The generated `System.Text.Json` converter tracks top-level JSON property presence, respects `JsonPropertyNameAttribute`, `JsonIgnoreAttribute`, and `JsonSerializerOptions.PropertyNamingPolicy`, and can reject unknown properties:

```csharp
[MergePatch(UnknownPropertyHandling = UnknownPropertyHandling.Reject)]
public partial class StrictPatch
{
    public string? Name { get; set; }
}
```

Nested object patching is not a v1 feature. If a nested object property is present, MergePatchDto treats that object as a provided value and assigns or applies it according to the property mapping. Arrays are treated as replacement values, not partially merged.

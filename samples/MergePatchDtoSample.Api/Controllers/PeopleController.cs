using MergePatchDtoSample.Api.Models;
using MergePatchDtoSample.Api.Patches;
using Microsoft.AspNetCore.Mvc;

namespace MergePatchDtoSample.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PeopleController(PersonStore people) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyCollection<Person>> List()
    {
        return Ok(people.List());
    }

    [HttpPost("reset")]
    public ActionResult<IReadOnlyCollection<Person>> Reset()
    {
        people.Reset();
        return Ok(people.List());
    }

    [HttpGet("{id:guid}")]
    public ActionResult<Person> Get(Guid id)
    {
        return people.TryGet(id, out var person)
            ? Ok(people.Snapshot(person))
            : NotFound();
    }

    [HttpPatch("{id:guid}/generated")]
    public ActionResult<PatchResult> PatchWithGeneratedApplyTo(Guid id, UpdatePersonPatch patch)
    {
        if (!people.TryGet(id, out var person))
        {
            return NotFound();
        }

        patch.ApplyTo(person);

        return Ok(new PatchResult(
            "generated ApplyTo",
            ProvidedFields(patch),
            people.Snapshot(person)));
    }

    [HttpPatch("{id:guid}/manual")]
    public ActionResult<PatchResult> PatchWithManualPresenceLogic(Guid id, ManualPersonPatch patch)
    {
        if (!people.TryGet(id, out var person))
        {
            return NotFound();
        }

        if (patch.Has.IsActive)
        {
            person.IsActive = patch.IsActive.GetValueOrDefault();
            person.DeactivatedAt = person.IsActive ? null : DateTimeOffset.UtcNow;
        }

        if (patch.Has.AdminNote)
        {
            person.AdminNote = patch.AdminNote;
        }

        return Ok(new PatchResult(
            "targetless Has checks",
            ProvidedFields(patch),
            people.Snapshot(person)));
    }

    [HttpPatch("{id:guid}/interface")]
    public ActionResult<PatchResult> PatchThroughInterfaceTarget(Guid id, InterfacePersonPatch patch)
    {
        if (!people.TryGet(id, out var person))
        {
            return NotFound();
        }

        IEditablePersonProfile editable = person;
        patch.ApplyTo(editable);

        return Ok(new PatchResult(
            "generated ApplyTo through interface target",
            ProvidedFields(patch),
            people.Snapshot(person)));
    }

    [HttpPatch("{id:guid}/strict")]
    public ActionResult<PatchResult> PatchWithStrictUnknownProperties(Guid id, StrictPersonPatch patch)
    {
        if (!people.TryGet(id, out var person))
        {
            return NotFound();
        }

        patch.ApplyTo(person);

        return Ok(new PatchResult(
            "generated ApplyTo with unknown-property rejection",
            ProvidedFields(patch),
            people.Snapshot(person)));
    }

    private static string[] ProvidedFields(UpdatePersonPatch patch)
    {
        var fields = new List<string>();
        var has = patch.Has;

        if (has.Name) fields.Add(nameof(patch.Name));
        if (has.Email) fields.Add(nameof(patch.Email));
        if (has.Phone) fields.Add(nameof(patch.Phone));
        if (has.Bio) fields.Add(nameof(patch.Bio));
        if (has.Age) fields.Add(nameof(patch.Age));
        if (has.Address) fields.Add(nameof(patch.Address));
        if (has.Skills) fields.Add(nameof(patch.Skills));
        if (has.RequestId) fields.Add(nameof(patch.RequestId));

        return [.. fields];
    }

    private static string[] ProvidedFields(ManualPersonPatch patch)
    {
        var fields = new List<string>();
        var has = patch.Has;

        if (has.IsActive) fields.Add(nameof(patch.IsActive));
        if (has.AdminNote) fields.Add(nameof(patch.AdminNote));

        return [.. fields];
    }

    private static string[] ProvidedFields(InterfacePersonPatch patch)
    {
        var fields = new List<string>();
        var has = patch.Has;

        if (has.Name) fields.Add(nameof(patch.Name));
        if (has.Email) fields.Add(nameof(patch.Email));
        if (has.Phone) fields.Add(nameof(patch.Phone));

        return [.. fields];
    }

    private static string[] ProvidedFields(StrictPersonPatch patch)
    {
        var fields = new List<string>();
        var has = patch.Has;

        if (has.Name) fields.Add(nameof(patch.Name));
        if (has.Email) fields.Add(nameof(patch.Email));

        return [.. fields];
    }
}

public sealed record PatchResult(string Mode, string[] ProvidedFields, Person Person);

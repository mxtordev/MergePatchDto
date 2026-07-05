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
    public IActionResult PatchWithGeneratedApplyTo(Guid id, UpdatePersonPatch patch)
    {
        if (!people.TryGet(id, out var person))
        {
            return NotFound();
        }

        patch.ApplyTo(person);

        return NoContent();
    }

    [HttpPatch("{id:guid}/manual")]
    public IActionResult PatchWithManualPresenceLogic(Guid id, ManualPersonPatch patch)
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

        return NoContent();
    }

    [HttpPatch("{id:guid}/interface")]
    public IActionResult PatchThroughInterfaceTarget(Guid id, InterfacePersonPatch patch)
    {
        if (!people.TryGet(id, out var person))
        {
            return NotFound();
        }

        IEditablePersonProfile editable = person;
        patch.ApplyTo(editable);

        return NoContent();
    }

    [HttpPatch("{id:guid}/strict")]
    public IActionResult PatchWithStrictUnknownProperties(Guid id, StrictPersonPatch patch)
    {
        if (!people.TryGet(id, out var person))
        {
            return NotFound();
        }

        patch.ApplyTo(person);

        return NoContent();
    }
}

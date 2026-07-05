using MergePatch;
using MergePatchDtoSample.Api.Models;

namespace MergePatchDtoSample.Api.Patches;

[MergePatch(typeof(IEditablePersonProfile))]
public partial class InterfacePersonPatch
{
    public string Name { get; set; } = "";

    public string? Email { get; set; }

    [PatchTo(nameof(IEditablePersonProfile.PhoneNumber))]
    public string? Phone { get; set; }
}

using MergePatch;

namespace MergePatchDtoSample.Api.Patches;

[MergePatch]
public partial class ManualPersonPatch
{
    public bool? IsActive { get; set; }

    public string? AdminNote { get; set; }
}

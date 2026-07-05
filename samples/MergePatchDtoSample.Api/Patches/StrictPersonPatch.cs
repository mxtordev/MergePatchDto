using MergePatch;
using MergePatchDtoSample.Api.Models;

namespace MergePatchDtoSample.Api.Patches;

[MergePatch(typeof(Person), UnknownPropertyHandling = UnknownPropertyHandling.Reject)]
public partial class StrictPersonPatch
{
    public string? Name { get; set; }

    public string? Email { get; set; }
}

using System.Text.Json.Serialization;
using MergePatch;
using MergePatchDtoSample.Api.Models;

namespace MergePatchDtoSample.Api.Patches;

[MergePatch(typeof(Person))]
public partial class UpdatePersonPatch
{
    public string? Name { get; set; }

    public string? Email { get; set; }

    [JsonPropertyName("phone")]
    [PatchTo(nameof(Person.PhoneNumber))]
    public string? Phone { get; set; }

    public string? Bio { get; set; }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    [PatchUsing(nameof(ApplyAge))]
    public int? Age { get; set; }

    public Address? Address { get; set; }

    public List<string>? Skills { get; set; }

    [PatchIgnore]
    public string? RequestId { get; set; }

    private static void ApplyAge(Person target, int? value)
    {
        target.SetAge(value);
    }
}

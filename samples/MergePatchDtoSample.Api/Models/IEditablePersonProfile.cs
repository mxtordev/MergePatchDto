namespace MergePatchDtoSample.Api.Models;

public interface IEditablePersonProfile
{
    string? Name { get; set; }

    string? Email { get; set; }

    string? PhoneNumber { get; set; }
}

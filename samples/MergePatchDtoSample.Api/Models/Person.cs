namespace MergePatchDtoSample.Api.Models;

public sealed class Person : IEditablePersonProfile
{
    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Bio { get; set; }

    public int? Age { get; private set; }

    public Address? Address { get; set; }

    public List<string>? Skills { get; set; }

    public bool IsActive { get; set; }

    public string? AdminNote { get; set; }

    public DateTimeOffset? DeactivatedAt { get; set; }

    public void SetAge(int? age)
    {
        Age = age;
    }
}

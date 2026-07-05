using System.Diagnostics.CodeAnalysis;
using MergePatchDtoSample.Api.Models;

namespace MergePatchDtoSample.Api;

public sealed class PersonStore
{
    private readonly object _gate = new();
    private readonly Dictionary<Guid, Person> _people = new();

    public PersonStore()
    {
        Reset();
    }

    public IReadOnlyCollection<Person> List()
    {
        lock (_gate)
        {
            return _people.Values
                .OrderBy(person => person.Name)
                .Select(Clone)
                .ToArray();
        }
    }

    public bool TryGet(Guid id, [NotNullWhen(true)] out Person? person)
    {
        lock (_gate)
        {
            if (_people.TryGetValue(id, out var stored))
            {
                person = stored;
                return true;
            }
        }

        person = null;
        return false;
    }

    public void Reset()
    {
        lock (_gate)
        {
            _people.Clear();

            var first = new Person
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Ada Lovelace",
                Email = "ada@example.com",
                PhoneNumber = "+46 70 111 11 11",
                Bio = "Original profile",
                Address = new Address { City = "Stockholm", Country = "SE" },
                Skills = ["math", "programming"],
                IsActive = true,
                AdminNote = "Only the manual endpoint can touch this"
            };
            first.SetAge(28);

            var second = new Person
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Grace Hopper",
                Email = "grace@example.com",
                PhoneNumber = "+47 22 222 22 22",
                Bio = "Already active",
                Address = new Address { City = "Oslo", Country = "NO" },
                Skills = ["compilers"],
                IsActive = true
            };
            second.SetAge(35);

            _people[first.Id] = first;
            _people[second.Id] = second;
        }
    }

    public Person Snapshot(Person person)
    {
        lock (_gate)
        {
            return Clone(person);
        }
    }

    private static Person Clone(Person source)
    {
        var clone = new Person
        {
            Id = source.Id,
            Name = source.Name,
            Email = source.Email,
            PhoneNumber = source.PhoneNumber,
            Bio = source.Bio,
            Address = source.Address is null
                ? null
                : new Address { City = source.Address.City, Country = source.Address.Country },
            Skills = source.Skills is null ? null : [.. source.Skills],
            IsActive = source.IsActive,
            AdminNote = source.AdminNote,
            DeactivatedAt = source.DeactivatedAt
        };

        clone.SetAge(source.Age);
        return clone;
    }
}

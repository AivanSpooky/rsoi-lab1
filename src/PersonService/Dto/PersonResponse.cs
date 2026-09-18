using PersonService.Domain;

namespace PersonService.Dto;

public class PersonResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int? Age { get; set; }

    public string? Address { get; set; }

    public string? Work { get; set; }

    public static PersonResponse From(Person person) => new()
    {
        Id = person.Id,
        Name = person.Name,
        Age = person.Age,
        Address = person.Address,
        Work = person.Work
    };
}

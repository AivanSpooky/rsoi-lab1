using Microsoft.EntityFrameworkCore;
using PersonService.Data;
using PersonService.Domain;
using PersonService.Dto;
using PersonService.Exceptions;

namespace PersonService.Services;

public class PersonServiceImpl(PersonDbContext context) : IPersonService
{
    public async Task<IReadOnlyList<PersonResponse>> ListAsync(CancellationToken cancellationToken = default)
    {
        var persons = await context.Persons
            .AsNoTracking()
            .OrderBy(person => person.Id)
            .ToListAsync(cancellationToken);

        return persons.Select(PersonResponse.From).ToList();
    }

    public async Task<PersonResponse> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var person = await context.Persons
            .AsNoTracking()
            .FirstOrDefaultAsync(person => person.Id == id, cancellationToken);

        return person is null ? throw new PersonNotFoundException(id) : PersonResponse.From(person);
    }

    public async Task<int> CreateAsync(PersonRequest request, CancellationToken cancellationToken = default)
    {
        var person = new Person
        {
            Name = request.Name!,
            Age = request.Age,
            Address = request.Address,
            Work = request.Work
        };

        context.Persons.Add(person);
        await context.SaveChangesAsync(cancellationToken);

        return person.Id;
    }

    public async Task<PersonResponse> UpdateAsync(
        int id,
        PersonRequest request,
        CancellationToken cancellationToken = default)
    {
        var person = await context.Persons
            .FirstOrDefaultAsync(person => person.Id == id, cancellationToken)
            ?? throw new PersonNotFoundException(id);

        person.Name = request.Name ?? person.Name;
        person.Age = request.Age ?? person.Age;
        person.Address = request.Address ?? person.Address;
        person.Work = request.Work ?? person.Work;

        await context.SaveChangesAsync(cancellationToken);

        return PersonResponse.From(person);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var person = await context.Persons
            .FirstOrDefaultAsync(person => person.Id == id, cancellationToken)
            ?? throw new PersonNotFoundException(id);

        context.Persons.Remove(person);
        await context.SaveChangesAsync(cancellationToken);
    }
}

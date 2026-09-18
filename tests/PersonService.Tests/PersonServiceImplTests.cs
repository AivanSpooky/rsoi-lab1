using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PersonService.Data;
using PersonService.Domain;
using PersonService.Dto;
using PersonService.Exceptions;
using PersonService.Services;

namespace PersonService.Tests;

public class PersonServiceImplTests
{
    private static PersonDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<PersonDbContext>()
            .UseInMemoryDatabase($"persons-{Guid.NewGuid()}")
            .Options);

    [Fact]
    public async Task CreateAsync_StoresPersonAndReturnsGeneratedId()
    {
        await using var context = CreateContext();
        var service = new PersonServiceImpl(context);

        var id = await service.CreateAsync(new PersonRequest
        {
            Name = "Alice",
            Age = 31,
            Address = "Nevsky prospect, 1",
            Work = "ITMO"
        });

        id.Should().BeGreaterThan(0);

        var stored = await context.Persons.SingleAsync();
        stored.Id.Should().Be(id);
        stored.Name.Should().Be("Alice");
        stored.Age.Should().Be(31);
        stored.Address.Should().Be("Nevsky prospect, 1");
        stored.Work.Should().Be("ITMO");
    }

    [Fact]
    public async Task GetAsync_ReturnsStoredPerson()
    {
        await using var context = CreateContext();
        var person = new Person { Name = "Bob", Age = 42, Address = "Baker street, 221b", Work = "Detective" };
        context.Persons.Add(person);
        await context.SaveChangesAsync();

        var response = await new PersonServiceImpl(context).GetAsync(person.Id);

        response.Id.Should().Be(person.Id);
        response.Name.Should().Be("Bob");
        response.Age.Should().Be(42);
        response.Address.Should().Be("Baker street, 221b");
        response.Work.Should().Be("Detective");
    }

    [Fact]
    public async Task GetAsync_ThrowsPersonNotFound_WhenIdIsUnknown()
    {
        await using var context = CreateContext();
        var service = new PersonServiceImpl(context);

        var act = () => service.GetAsync(9999);

        await act.Should().ThrowAsync<PersonNotFoundException>();
    }

    [Fact]
    public async Task ListAsync_ReturnsEveryPersonOrderedById()
    {
        await using var context = CreateContext();
        context.Persons.AddRange(
            new Person { Name = "Alice" },
            new Person { Name = "Bob" },
            new Person { Name = "Carol" });
        await context.SaveChangesAsync();

        var response = await new PersonServiceImpl(context).ListAsync();

        response.Should().HaveCount(3);
        response.Select(person => person.Name).Should().ContainInOrder("Alice", "Bob", "Carol");
    }

    [Fact]
    public async Task UpdateAsync_OverwritesOnlyTheFieldsPresentInRequest()
    {
        await using var context = CreateContext();
        var person = new Person { Name = "Alice", Age = 31, Address = "Old address", Work = "ITMO" };
        context.Persons.Add(person);
        await context.SaveChangesAsync();

        var response = await new PersonServiceImpl(context)
            .UpdateAsync(person.Id, new PersonRequest { Name = "Alice Smith", Address = "New address" });

        response.Name.Should().Be("Alice Smith");
        response.Address.Should().Be("New address");
        response.Age.Should().Be(31);
        response.Work.Should().Be("ITMO");
    }

    [Fact]
    public async Task UpdateAsync_ThrowsPersonNotFound_WhenIdIsUnknown()
    {
        await using var context = CreateContext();
        var service = new PersonServiceImpl(context);

        var act = () => service.UpdateAsync(9999, new PersonRequest { Name = "Nobody" });

        await act.Should().ThrowAsync<PersonNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_RemovesPersonFromStorage()
    {
        await using var context = CreateContext();
        var person = new Person { Name = "Alice" };
        context.Persons.Add(person);
        await context.SaveChangesAsync();

        await new PersonServiceImpl(context).DeleteAsync(person.Id);

        context.Persons.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_ThrowsPersonNotFound_WhenIdIsUnknown()
    {
        await using var context = CreateContext();
        var service = new PersonServiceImpl(context);

        var act = () => service.DeleteAsync(9999);

        await act.Should().ThrowAsync<PersonNotFoundException>();
    }
}

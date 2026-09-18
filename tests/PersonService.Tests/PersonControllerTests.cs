using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonService.Controllers;
using PersonService.Data;
using PersonService.Domain;
using PersonService.Dto;
using PersonService.Services;

namespace PersonService.Tests;

public class PersonControllerTests
{
    private static (PersonController Controller, PersonDbContext Context) CreateController()
    {
        var context = new PersonDbContext(new DbContextOptionsBuilder<PersonDbContext>()
            .UseInMemoryDatabase($"persons-{Guid.NewGuid()}")
            .Options);

        var controller = new PersonController(new PersonServiceImpl(context))
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return (controller, context);
    }

    [Fact]
    public async Task CreatePerson_Returns201WithLocationHeaderAndEmptyBody()
    {
        var (controller, context) = CreateController();
        await using var _ = context;

        var result = await controller.CreatePerson(new PersonRequest { Name = "Alice", Age = 31 }, default);

        result.Should().BeOfType<StatusCodeResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status201Created);

        var id = (await context.Persons.SingleAsync()).Id;
        controller.Response.Headers.Location.ToString().Should().Be($"/api/v1/persons/{id}");
    }

    [Fact]
    public async Task CreatePerson_Returns400WithValidationErrors_WhenNameIsMissing()
    {
        var (controller, context) = CreateController();
        await using var _ = context;

        var result = await controller.CreatePerson(new PersonRequest { Age = 31 }, default);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var body = badRequest.Value.Should().BeOfType<ValidationErrorResponse>().Subject;
        body.Errors.Should().ContainKey("name");

        context.Persons.Should().BeEmpty();
    }

    [Fact]
    public async Task RemovePerson_Returns204()
    {
        var (controller, context) = CreateController();
        await using var _ = context;

        var person = new Person { Name = "Alice" };
        context.Persons.Add(person);
        await context.SaveChangesAsync();

        var result = await controller.RemovePerson(person.Id, default);

        result.Should().BeOfType<NoContentResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }
}

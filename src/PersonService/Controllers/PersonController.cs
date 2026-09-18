using Microsoft.AspNetCore.Mvc;
using PersonService.Dto;
using PersonService.Services;

namespace PersonService.Controllers;

[ApiController]
[Route("api/v1/persons")]
[Produces("application/json")]
[Tags("Person REST API operations")]
public class PersonController(IPersonService personService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PersonResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PersonResponse>>> ListPersons(CancellationToken cancellationToken)
        => Ok(await personService.ListAsync(cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType<PersonResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PersonResponse>> GetPerson(int id, CancellationToken cancellationToken)
        => Ok(await personService.GetAsync(id, cancellationToken));

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePerson(
        [FromBody] PersonRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            ModelState.AddModelError(nameof(PersonRequest.Name), "Name is required");
            return BadRequest(ValidationErrorResponse.FromModelState(ModelState));
        }

        var id = await personService.CreateAsync(request, cancellationToken);

        Response.Headers.Location = $"/api/v1/persons/{id}";
        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPatch("{id:int}")]
    [ProducesResponseType<PersonResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PersonResponse>> EditPerson(
        int id,
        [FromBody] PersonRequest request,
        CancellationToken cancellationToken)
        => Ok(await personService.UpdateAsync(id, request, cancellationToken));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemovePerson(int id, CancellationToken cancellationToken)
    {
        await personService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

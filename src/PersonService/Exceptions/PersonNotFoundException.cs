namespace PersonService.Exceptions;

public class PersonNotFoundException(int id)
    : Exception($"Person with id {id} not found");

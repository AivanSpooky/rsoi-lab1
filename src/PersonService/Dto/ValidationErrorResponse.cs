using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PersonService.Dto;

public class ValidationErrorResponse(string message, IDictionary<string, string> errors)
{
    public string Message { get; set; } = message;

    public IDictionary<string, string> Errors { get; set; } = errors;

    public static ValidationErrorResponse FromModelState(ModelStateDictionary modelState)
    {
        var errors = modelState
            .Where(entry => entry.Value is { Errors.Count: > 0 })
            .ToDictionary(
                entry => NormalizeKey(entry.Key),
                entry => string.Join("; ", entry.Value!.Errors.Select(DescribeError)));

        return new ValidationErrorResponse("Invalid request data", errors);
    }

    private static string NormalizeKey(string key)
    {
        var name = key.StartsWith("$.", StringComparison.Ordinal) ? key[2..] : key;
        return name.Length == 0 ? "request" : char.ToLowerInvariant(name[0]) + name[1..];
    }

    private static string DescribeError(ModelError error) =>
        string.IsNullOrWhiteSpace(error.ErrorMessage) ? "Invalid value" : error.ErrorMessage;
}

using System.ComponentModel.DataAnnotations;

namespace PersonService.Dto;

public class PersonRequest
{
    [StringLength(255, MinimumLength = 1)]
    public string? Name { get; set; }

    [Range(1, 200)]
    public int? Age { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    [StringLength(255)]
    public string? Work { get; set; }
}

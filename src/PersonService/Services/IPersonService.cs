using PersonService.Dto;

namespace PersonService.Services;

public interface IPersonService
{
    Task<IReadOnlyList<PersonResponse>> ListAsync(CancellationToken cancellationToken = default);

    Task<PersonResponse> GetAsync(int id, CancellationToken cancellationToken = default);

    Task<int> CreateAsync(PersonRequest request, CancellationToken cancellationToken = default);

    Task<PersonResponse> UpdateAsync(int id, PersonRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}

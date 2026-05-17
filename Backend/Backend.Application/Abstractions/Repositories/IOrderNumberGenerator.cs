namespace Backend.Application.Abstractions.Repositories;

public interface IOrderNumberGenerator
{
    Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken = default);
}

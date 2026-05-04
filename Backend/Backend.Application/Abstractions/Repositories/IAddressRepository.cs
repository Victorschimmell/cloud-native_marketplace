using Backend.Domain.Entities.Location;

namespace Backend.Application.Abstractions.Repositories;

public interface IAddressRepository
{
    Task<Address?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Address address, CancellationToken cancellationToken = default);
}

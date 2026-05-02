using Backend.Domain.Entities.Location;

namespace Backend.Application.Abstractions.Repositories;

public interface IAddressRepository
{
    Task AddAsync(Address address, CancellationToken cancellationToken = default);
}

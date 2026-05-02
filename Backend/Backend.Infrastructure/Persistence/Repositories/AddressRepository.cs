using Backend.Application.Abstractions.Repositories;
using Backend.Domain.Entities.Location;

namespace Backend.Infrastructure.Persistence.Repositories;

internal sealed class AddressRepository(ApplicationDbContext dbContext) : IAddressRepository
{
    public Task AddAsync(Address address, CancellationToken cancellationToken = default)
    {
        dbContext.Addresses.Add(address);
        return Task.CompletedTask;
    }
}

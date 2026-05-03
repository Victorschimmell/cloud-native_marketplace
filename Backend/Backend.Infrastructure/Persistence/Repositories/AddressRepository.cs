using Backend.Application.Abstractions.Repositories;
using Backend.Domain.Entities.Location;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.Repositories;

internal sealed class AddressRepository(ApplicationDbContext dbContext) : IAddressRepository
{
    public async Task<Address?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Addresses
            .FirstOrDefaultAsync(address => address.Id == id, cancellationToken);
    }

    public Task AddAsync(Address address, CancellationToken cancellationToken = default)
    {
        dbContext.Addresses.Add(address);
        return Task.CompletedTask;
    }
}

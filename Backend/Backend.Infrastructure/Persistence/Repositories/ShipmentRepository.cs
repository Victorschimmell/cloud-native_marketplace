using Backend.Application.Abstractions.Repositories;
using Backend.Domain.Entities.Orders;
using Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.Repositories;

internal sealed class ShipmentRepository(ApplicationDbContext dbContext) : IShipmentRepository
{
    public async Task<Shipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Shipments
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Shipment>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Shipments
            .Where(s => s.OrderId == orderId)
            .OrderBy(s => s.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Shipment>> GetBySellerIdAsync(Guid sellerId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await dbContext.Shipments
            .Where(s => s.SellerId == sellerId)
            .OrderByDescending(s => s.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(Shipment shipment, CancellationToken cancellationToken = default)
    {
        dbContext.Shipments.Add(shipment);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Shipment shipment, CancellationToken cancellationToken = default)
    {
        dbContext.Shipments.Update(shipment);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Shipment shipment, CancellationToken cancellationToken = default)
    {
        dbContext.Shipments.Remove(shipment);
        return Task.CompletedTask;
    }
}

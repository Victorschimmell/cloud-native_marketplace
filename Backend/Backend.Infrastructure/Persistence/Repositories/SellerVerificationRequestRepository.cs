using Backend.Application.Abstractions.Repositories;
using Backend.Domain.Entities.IdentityAccess;
using Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.Repositories;

internal sealed class SellerVerificationRequestRepository(ApplicationDbContext dbContext) : ISellerVerificationRequestRepository
{
    public async Task<SellerVerificationRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.SellerVerificationRequests
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<SellerVerificationRequest>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await dbContext.SellerVerificationRequests
            .OrderByDescending(r => r.SubmittedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SellerVerificationRequest>> GetBySellerIdAsync(Guid sellerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.SellerVerificationRequests
            .Where(r => r.SellerId == sellerId)
            .OrderByDescending(r => r.SubmittedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(SellerVerificationRequest request, CancellationToken cancellationToken = default)
    {
        await dbContext.SellerVerificationRequests.AddAsync(request, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(SellerVerificationRequest request, CancellationToken cancellationToken = default)
    {
        dbContext.SellerVerificationRequests.Update(request);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

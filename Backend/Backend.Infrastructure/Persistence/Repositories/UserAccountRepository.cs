using Backend.Application.Abstractions.Repositories;
using Backend.Domain.Entities.IdentityAccess;
using Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.Repositories;

internal sealed class UserAccountRepository(ApplicationDbContext dbContext) : IUserAccountRepository
{
    public async Task<UserAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.UserAccounts
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<UserAccount?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await dbContext.UserAccounts
            .FirstOrDefaultAsync(u => u.Email.Value == email, cancellationToken);
    }

    public async Task<IReadOnlyList<UserAccount>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await dbContext.UserAccounts
            .OrderBy(u => u.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(UserAccount userAccount, CancellationToken cancellationToken = default)
    {
        await dbContext.UserAccounts.AddAsync(userAccount, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UserAccount userAccount, CancellationToken cancellationToken = default)
    {
        dbContext.UserAccounts.Update(userAccount);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(UserAccount userAccount, CancellationToken cancellationToken = default)
    {
        dbContext.UserAccounts.Remove(userAccount);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

using Backend.Application.Abstractions.Repositories;
using Backend.Domain.Entities.IdentityAccess;
using Backend.Domain.ValueObjects;
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
        var emailAddress = new EmailAddress(email);

        return await dbContext.UserAccounts
            .FirstOrDefaultAsync(u => u.Email == emailAddress, cancellationToken);
    }

    public async Task<IReadOnlyList<UserAccount>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await dbContext.UserAccounts
            .OrderBy(u => u.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(UserAccount userAccount, CancellationToken cancellationToken = default)
    {
        dbContext.UserAccounts.Add(userAccount);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(UserAccount userAccount, CancellationToken cancellationToken = default)
    {
        dbContext.UserAccounts.Update(userAccount);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(UserAccount userAccount, CancellationToken cancellationToken = default)
    {
        dbContext.UserAccounts.Remove(userAccount);
        return Task.CompletedTask;
    }
}

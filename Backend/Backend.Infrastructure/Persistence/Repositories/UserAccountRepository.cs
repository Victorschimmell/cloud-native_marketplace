using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Models;
using Backend.Domain.Entities.IdentityAccess;
using Backend.Domain.Enums;
using Backend.Domain.ValueObjects;
using Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.Repositories;

internal sealed class UserAccountRepository(ApplicationDbContext dbContext) : IUserAccountRepository
{
    public async Task<UserAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.UserAccounts
            .Include(u => u.CustomerProfile)
            .Include(u => u.SellerProfile)
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

    public async Task<PagedResult<UserAccount>> GetByFilterAsync(
        AdminUserRoleFilter role,
        AdminUserStatusFilter status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.UserAccounts
            .Include(u => u.CustomerProfile)
            .Include(u => u.SellerProfile)
                .ThenInclude(s => s!.VerificationRequests)
            .AsQueryable();

        query = role switch
        {
            AdminUserRoleFilter.Admin => query.Where(u => u.IsAdmin),
            AdminUserRoleFilter.Customer => query.Where(u => !u.IsAdmin && u.CustomerProfile != null),
            AdminUserRoleFilter.Seller => query.Where(u => !u.IsAdmin && u.SellerProfile != null),
            _ => query
        };

        query = status switch
        {
            AdminUserStatusFilter.Active => query.Where(u => !u.IsBlocked && u.AccountStatus == AccountStatus.Active),
            AdminUserStatusFilter.Blocked => query.Where(u => u.IsBlocked || u.AccountStatus == AccountStatus.Suspended),
            AdminUserStatusFilter.PendingVerification => query.Where(u => u.SellerProfile != null && u.SellerProfile.VerificationStatus == VerificationStatus.Pending),
            _ => query
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(u => u.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<UserAccount>(items, page, pageSize, totalCount);
    }

    public async Task<int> CountActiveAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.UserAccounts
            .CountAsync(u => !u.IsBlocked && u.AccountStatus == AccountStatus.Active, cancellationToken);
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

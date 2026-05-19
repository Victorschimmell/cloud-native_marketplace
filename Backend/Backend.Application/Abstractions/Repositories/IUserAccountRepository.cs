using Backend.Application.Common.Models;
using Backend.Domain.Entities.IdentityAccess;

namespace Backend.Application.Abstractions.Repositories;

public enum AdminUserRoleFilter
{
    Any,
    Customer,
    Seller,
    Admin
}

public enum AdminUserStatusFilter
{
    Any,
    Active,
    PendingVerification,
    Blocked
}

public interface IUserAccountRepository
{
    Task<UserAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserAccount?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserAccount>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<UserAccount>> GetByFilterAsync(
        AdminUserRoleFilter role,
        AdminUserStatusFilter status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<int> CountActiveAsync(CancellationToken cancellationToken = default);
    Task AddAsync(UserAccount userAccount, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserAccount userAccount, CancellationToken cancellationToken = default);
    Task DeleteAsync(UserAccount userAccount, CancellationToken cancellationToken = default);
}

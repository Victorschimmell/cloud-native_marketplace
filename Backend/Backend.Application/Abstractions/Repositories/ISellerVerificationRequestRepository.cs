using Backend.Domain.Entities.IdentityAccess;

namespace Backend.Application.Abstractions.Repositories;

public interface ISellerVerificationRequestRepository
{
    Task<SellerVerificationRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SellerVerificationRequest>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    Task AddAsync(SellerVerificationRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(SellerVerificationRequest request, CancellationToken cancellationToken = default);
}

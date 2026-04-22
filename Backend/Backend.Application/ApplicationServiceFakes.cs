// IMPORTANT:
// Before the following interfaces are implemented, this file is used to provide fake implementations to allow the application to run without errors.
// These file should be removed once the actual implementations are ready.

using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.DTOs;
using Backend.Domain.Entities.IdentityAccess;
using Backend.Domain.Entities.Orders;

namespace Backend.Application;

internal sealed class FakePaymentRepository : IPaymentRepository
{
    public Task AddAsync(OrderPayment payment, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task DeleteAsync(OrderPayment payment, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<OrderPayment?> GetByIdAsync(Guid orderId, int paymentSequential, CancellationToken cancellationToken = default) => Task.FromResult<OrderPayment?>(null);
    public Task<IReadOnlyList<OrderPayment>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<OrderPayment>>([]);
    public Task UpdateAsync(OrderPayment payment, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

internal sealed class FakeDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => new(2026, 4, 9, 12, 0, 0, TimeSpan.Zero);
}

internal sealed class FakeCurrentUserProvider : ICurrentUserProvider
{
    public Guid? UserId => Guid.Parse("11111111-1111-1111-1111-111111111111");
    public bool IsAuthenticated => true;
    public bool IsAdmin => true;
}

internal sealed class FakePasswordHasher : IPasswordHasher
{
    public string HashPassword(string password) => $"hashed::{password}";
    public bool VerifyPassword(UserAccount userAccount, string password) => userAccount.PasswordHash == HashPassword(password);
}

internal sealed class FakeAuthTokenGenerator : IAuthTokenGenerator
{
    public AuthTokenDto CreateToken(UserAccount userAccount) =>
        new($"token-for-{userAccount.Id}", DateTimeOffset.UtcNow.AddHours(1));
}

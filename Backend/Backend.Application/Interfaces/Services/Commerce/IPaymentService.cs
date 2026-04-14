using Backend.Application.Common.Results;
using Backend.Application.DTOs;

namespace Backend.Application.Interfaces.Services;

public interface IPaymentService
{
    Task<Result<IReadOnlyList<PaymentDto>>> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Result<PaymentDto>> RecordPaymentAsync(RecordPaymentRequest request, CancellationToken cancellationToken = default);
}

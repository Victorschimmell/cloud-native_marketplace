using Backend.Application.Common.Results;
using Backend.Application.DTOs;

namespace Backend.Application.Interfaces.Services;

public interface ICheckoutService
{
    Task<Result<CheckoutResponse>> CheckoutAsync(CheckoutRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CheckoutLineDto>>> GetCheckoutPreviewAsync(GetCheckoutPreviewRequest request, CancellationToken cancellationToken = default);
}

using Backend.Application.Common.Results;
using Backend.Application.DTOs;

namespace Backend.Application.Interfaces.Services;

public interface ICheckoutService
{
    Task<Result<CheckoutResponse>> CheckoutAsync(CheckoutRequest request, string displayCurrency, CancellationToken cancellationToken = default);
    Task<Result<CheckoutPreviewDto>> GetCheckoutPreviewAsync(GetCheckoutPreviewRequest request, string displayCurrency, CancellationToken cancellationToken = default);
}

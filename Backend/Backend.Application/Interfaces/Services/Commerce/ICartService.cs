using Backend.Application.Common.Results;
using Backend.Application.DTOs;

namespace Backend.Application.Interfaces.Services;

public interface ICartService
{
    Task<Result<CartDto>> GetCartAsync(GetCartRequest request, string displayCurrency, CancellationToken cancellationToken = default);
    Task<Result<CartDto>> AddItemAsync(AddCartItemRequest request, string displayCurrency, CancellationToken cancellationToken = default);
    Task<Result<CartDto>> UpdateItemAsync(UpdateCartItemRequest request, string displayCurrency, CancellationToken cancellationToken = default);
}

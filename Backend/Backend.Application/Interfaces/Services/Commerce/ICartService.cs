using Backend.Application.Common.Results;
using Backend.Application.DTOs;

namespace Backend.Application.Interfaces.Services;

public interface ICartService
{
    Task<Result<CartDto>> GetCartAsync(GetCartRequest request, CancellationToken cancellationToken = default);
    Task<Result<CartDto>> AddItemAsync(AddCartItemRequest request, CancellationToken cancellationToken = default);
    Task<Result<CartDto>> RemoveItemAsync(RemoveCartItemRequest request, CancellationToken cancellationToken = default);
}

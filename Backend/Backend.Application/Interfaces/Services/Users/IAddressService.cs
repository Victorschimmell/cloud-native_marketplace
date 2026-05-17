using Backend.Application.Common.Results;
using Backend.Application.DTOs;

namespace Backend.Application.Interfaces.Services;

public interface IAddressService
{
    Task<Result<AddressDto>> GetByIdAsync(Guid addressId, Guid authenticatedUserId, CancellationToken cancellationToken = default);
    Task<Result<AddressDto>> CreateAsync(CreateAddressRequest request, Guid authenticatedUserId, CancellationToken cancellationToken = default);
}

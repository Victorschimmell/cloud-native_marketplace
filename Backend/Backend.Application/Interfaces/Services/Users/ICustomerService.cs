using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;

namespace Backend.Application.Interfaces.Services;

public interface ICustomerService
{
    Task<Result<CustomerDto>> GetByIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<CustomerDto>>> GetCustomersAsync(PagedRequest request, CancellationToken cancellationToken = default);
}

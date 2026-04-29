using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;

namespace Backend.Application.Services;

public sealed class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        ArgumentNullException.ThrowIfNull(customerRepository);
        _customerRepository = customerRepository;
    }

    public Task<Result<CustomerDto>> GetByIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<CustomerDto>.NotImplemented());
    }

    public async Task<Result<PagedResult<CustomerDto>>> GetCustomersAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _customerRepository.GetAllAsync(request.Page, request.PageSize, cancellationToken);
        return Result<PagedResult<CustomerDto>>.Success(new PagedResult<CustomerDto>(
            result.Items.Select(customer => customer.ToCustomerDto()).ToArray(), request.Page, request.PageSize, result.TotalCount));
    }
}

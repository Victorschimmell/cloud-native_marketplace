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

    public async Task<Result<CustomerDto>> GetByIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            if (customerId == Guid.Empty)
            {
                return Result<CustomerDto>.ValidationFailure("Customer id is required.");
            }

            var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
            return customer is null
                ? Result<CustomerDto>.NotFound("Customer was not found.")
                : Result<CustomerDto>.Success(customer.ToDto());
        }, "Unable to get customer.");
    }

    public async Task<Result<PagedResult<CustomerDto>>> GetCustomersAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            if (request.Page <= 0 || request.PageSize <= 0)
            {
                return Result<PagedResult<CustomerDto>>.ValidationFailure("Page and page size must be greater than zero.");
            }

            var customers = await _customerRepository.GetAllAsync(request.Page, request.PageSize, cancellationToken);
            var items = customers.Select(static customer => customer.ToDto()).ToArray();
            return Result<PagedResult<CustomerDto>>.Success(new PagedResult<CustomerDto>(items, request.Page, request.PageSize, items.Length));
        }, "Unable to get customers.");
    }
}

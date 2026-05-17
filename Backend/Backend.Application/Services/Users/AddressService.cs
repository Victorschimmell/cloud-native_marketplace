using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Entities.Location;

namespace Backend.Application.Services;

public sealed class AddressService : IAddressService
{
    private readonly IAddressRepository _addressRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddressService(
        IAddressRepository addressRepository,
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(addressRepository);
        ArgumentNullException.ThrowIfNull(customerRepository);
        ArgumentNullException.ThrowIfNull(unitOfWork);

        _addressRepository = addressRepository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AddressDto>> GetByIdAsync(Guid addressId, Guid authenticatedUserId, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByUserIdAsync(authenticatedUserId, cancellationToken);
        if (customer is null)
        {
            return Result<AddressDto>.NotFound("Customer profile was not found for the authenticated user.");
        }

        if (customer.DefaultAddressId != addressId)
        {
            return Result<AddressDto>.NotFound("Address was not found for the authenticated customer.");
        }

        var address = await _addressRepository.GetByIdAsync(addressId, cancellationToken);
        if (address is null)
        {
            return Result<AddressDto>.NotFound("Address was not found.");
        }

        return Result<AddressDto>.Success(address.ToAddressDto());
    }

    public async Task<Result<AddressDto>> CreateAsync(CreateAddressRequest request, Guid authenticatedUserId, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByUserIdAsync(authenticatedUserId, cancellationToken);
        if (customer is null)
        {
            return Result<AddressDto>.NotFound("Customer profile was not found for the authenticated user.");
        }

        if (!TryCreateAddress(request, out var address, out var error))
        {
            return Result<AddressDto>.ValidationFailure(error);
        }

        await _addressRepository.AddAsync(address, cancellationToken);

        if (request.MakeDefault)
        {
            customer.DefaultAddressId = address.Id;
            await _customerRepository.UpdateAsync(customer, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<AddressDto>.Success(address.ToAddressDto());
    }

    private static bool TryCreateAddress(
        CreateAddressRequest request,
        out Address address,
        out string error)
    {
        address = null!;
        error = string.Empty;

        var postalCode = Normalize(request.PostalCode);
        var city = Normalize(request.City);
        var state = Normalize(request.State);
        var addressLine1 = Normalize(request.AddressLine1);
        var addressLine2 = NormalizeOptional(request.AddressLine2);
        var countryCode = Normalize(request.CountryCode).ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(addressLine1))
        {
            error = "Address line 1 is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            error = "City is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(state))
        {
            error = "State is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(postalCode))
        {
            error = "Postal code is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(countryCode))
        {
            error = "Country code is required.";
            return false;
        }

        address = new Address
        {
            PostalCode = postalCode,
            City = city,
            State = state,
            AddressLine1 = addressLine1,
            AddressLine2 = addressLine2,
            CountryCode = countryCode
        };
        return true;
    }

    private static string Normalize(string? value) => value?.Trim() ?? string.Empty;

    private static string? NormalizeOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}

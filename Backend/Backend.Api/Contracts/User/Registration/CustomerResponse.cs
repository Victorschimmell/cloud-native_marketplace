namespace Backend.Api.Contracts.User.Registration;

public sealed record CustomerResponse
{
    public required Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Phone { get; init; }
    public Guid? DefaultAddressId { get; init; }
    public string? OlistCustomerId { get; init; }
    public string? OlistCustomerUniqueId { get; init; }
}

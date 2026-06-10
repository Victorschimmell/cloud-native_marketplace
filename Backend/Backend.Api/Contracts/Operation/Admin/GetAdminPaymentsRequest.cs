namespace Backend.Api.Contracts.Operation.Admin;

public sealed record GetAdminPaymentsRequest
{
    public string? Status { get; init; }
}

namespace Backend.Api.Contracts.Operation.Admin;

public enum AdminPaymentStatusFilter
{
    Any,
    Completed,
    Pending,
    Failed
}

public sealed record GetAdminPaymentsRequest
{
    public AdminPaymentStatusFilter Status { get; init; } = AdminPaymentStatusFilter.Any;
}

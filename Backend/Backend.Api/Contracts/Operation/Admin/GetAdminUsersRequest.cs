namespace Backend.Api.Contracts.Operation.Admin;

public enum AdminUserRoleFilter
{
    Any,
    Customer,
    Seller,
    Admin
}

public enum AdminUserStatusFilter
{
    Any,
    Active,
    PendingVerification,
    Blocked
}

public sealed record GetAdminUsersRequest
{
    public AdminUserRoleFilter Role { get; init; } = AdminUserRoleFilter.Any;
    public AdminUserStatusFilter Status { get; init; } = AdminUserStatusFilter.Any;
}

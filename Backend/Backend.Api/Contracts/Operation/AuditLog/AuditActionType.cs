namespace Backend.Api.Contracts.Operation.AuditLog;

public enum AuditActionType
{
    Created = 1,
    Updated = 2,
    Deleted = 3,
    Login = 4,
    Logout = 5,
    Approve = 6,
    Reject = 7,
    Block = 8,
    Unblock = 9,
    Import = 10
}


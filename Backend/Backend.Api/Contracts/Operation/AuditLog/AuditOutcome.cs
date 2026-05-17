namespace Backend.Api.Contracts.Operation.AuditLog;

public enum AuditOutcome
{
    Succeeded = 1,
    Failed = 2,
    PartiallySucceeded = 3,
    Forbidden = 4,
}


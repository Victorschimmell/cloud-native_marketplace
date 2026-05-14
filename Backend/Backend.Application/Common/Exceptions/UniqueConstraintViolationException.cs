namespace Backend.Application.Common.Exceptions;

public enum UniqueConstraintTarget
{
    Unknown = 0,
    UserAccountEmail = 1,
    OrderReviewOrderItem = 2,
    OrderReviewCustomerProduct = 3
}

public sealed class UniqueConstraintViolationException : Exception
{
    public UniqueConstraintViolationException(
        UniqueConstraintTarget target,
        string? constraintName,
        Exception innerException)
        : base("A unique constraint was violated.", innerException)
    {
        Target = target;
        ConstraintName = constraintName;
    }

    public UniqueConstraintTarget Target { get; }

    public string? ConstraintName { get; }
}

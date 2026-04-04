namespace Backend.Domain.ValueObjects;

public sealed record EmailAddress(string Value)
{
    public override string ToString() => Value;
}

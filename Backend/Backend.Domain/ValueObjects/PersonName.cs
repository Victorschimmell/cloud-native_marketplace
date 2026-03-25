namespace Backend.Domain.ValueObjects;

public sealed record PersonName(string FirstName, string LastName)
{
    public string FullName => $"{FirstName} {LastName}".Trim();
}

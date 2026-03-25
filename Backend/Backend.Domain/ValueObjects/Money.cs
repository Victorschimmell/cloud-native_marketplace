using System.Globalization;

namespace Backend.Domain.ValueObjects;

public readonly record struct Money(decimal Amount, string Currency)
{
    public override string ToString() => $"{Amount.ToString("0.00", CultureInfo.InvariantCulture)} {Currency}";
}

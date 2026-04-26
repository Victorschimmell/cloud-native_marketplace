namespace Backend.Application.Services;

internal static class CurrencyConversion
{
    public const string BaseCurrency = "BRL";

    private static readonly IReadOnlyDictionary<string, decimal> RatesFromBrl = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
    {
        ["BRL"] = 1m,
        // Temporary fixed demo rates, which "can" be replaced with real persisted/auditet exchange rates, but I don't expect that this is a priority.
        ["USD"] = 0.18m,
        ["DKK"] = 1.17m
    };

    public static string NormalizeOrDefault(string? currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            return BaseCurrency;
        }

        return currency.Trim().ToUpperInvariant();
    }

    public static bool IsSupported(string currencyCode) => RatesFromBrl.ContainsKey(currencyCode);

    public static decimal FromBrl(decimal amount, string currencyCode)
    {
        var converted = amount * RatesFromBrl[currencyCode];
        return decimal.Round(converted, 2, MidpointRounding.AwayFromZero);
    }
}

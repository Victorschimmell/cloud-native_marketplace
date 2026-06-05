using Backend.Application.Common.Abstractions;

namespace Backend.Application.Services;

public sealed class FixedRateCurrencyConversionService : ICurrencyConversionService
{
    public string BaseCurrency => "BRL";

    private static readonly IReadOnlyDictionary<string, decimal> RatesFromBaseCurrency = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
    {
        ["BRL"] = 1m,
        // Temporary fixed demo rates until real persisted/audited exchange rates are introduced.
        ["USD"] = 0.18m,
        ["DKK"] = 1.17m
    };

    public string NormalizeOrDefault(string? currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            return BaseCurrency;
        }

        return currency.Trim().ToUpperInvariant();
    }

    public bool IsSupported(string currencyCode) => RatesFromBaseCurrency.ContainsKey(currencyCode);

    public decimal FromBaseCurrency(decimal amount, string currencyCode)
    {
        var converted = amount * RatesFromBaseCurrency[currencyCode];
        return decimal.Round(converted, 2, MidpointRounding.AwayFromZero);
    }

    public bool TryGetPriceFromBaseConverter(string? displayCurrency, out string currencyCode, out Func<decimal, decimal> priceConverter)
    {
        var normalizedCurrency = NormalizeOrDefault(displayCurrency);
        if (!IsSupported(normalizedCurrency))
        {
            currencyCode = BaseCurrency;
            priceConverter = static amount => amount;
            return false;
        }

        currencyCode = normalizedCurrency;
        priceConverter = amount => FromBaseCurrency(amount, normalizedCurrency);
        return true;
    }

    public Func<decimal, decimal> GetPriceFromBaseConverter(string displayCurrency)
    {
        var normalizedCurrency = NormalizeOrDefault(displayCurrency);
        if (!IsSupported(normalizedCurrency))
        {
            throw new ArgumentException($"Unsupported currency code: {displayCurrency}", nameof(displayCurrency));
        }

        return amount => FromBaseCurrency(amount, normalizedCurrency);
    }
}

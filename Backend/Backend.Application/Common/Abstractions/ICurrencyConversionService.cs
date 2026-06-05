namespace Backend.Application.Common.Abstractions;

public interface ICurrencyConversionService
{
    string BaseCurrency { get; }
    string NormalizeOrDefault(string? currency);
    bool IsSupported(string currencyCode);
    decimal FromBaseCurrency(decimal amount, string currencyCode);
    bool TryGetPriceFromBaseConverter(string? displayCurrency, out string currencyCode, out Func<decimal, decimal> priceConverter);
}

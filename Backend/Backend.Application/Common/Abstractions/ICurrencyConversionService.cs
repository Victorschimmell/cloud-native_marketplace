namespace Backend.Application.Common.Abstractions;

public interface ICurrencyConversionService
{
    string BaseCurrency { get; }
    string NormalizeOrDefault(string? currency);
    bool IsSupported(string currencyCode);
    decimal FromBaseCurrency(decimal amount, string currencyCode);
}

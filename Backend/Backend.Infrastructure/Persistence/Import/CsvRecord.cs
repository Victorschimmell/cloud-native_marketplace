using System.Globalization;

namespace Backend.Infrastructure.Persistence.Import;

public sealed class CsvRecord
{
    private readonly IReadOnlyDictionary<string, string> _values;

    public CsvRecord(IReadOnlyDictionary<string, string> values)
    {
        _values = values;
    }

    public string GetRequiredString(string columnName)
    {
        var value = GetOptionalString(columnName);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Column '{columnName}' is required.");
        }

        return value;
    }

    public string? GetOptionalString(string columnName)
    {
        if (!_values.TryGetValue(columnName, out var value))
        {
            throw new InvalidOperationException($"Column '{columnName}' was not found in the CSV file.");
        }

        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    public int GetRequiredInt32(string columnName)
    {
        var value = GetRequiredString(columnName);

        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            throw new InvalidOperationException($"Column '{columnName}' contains an invalid integer value '{value}'.");
        }

        return parsed;
    }

    public int? GetOptionalInt32(string columnName)
    {
        var value = GetOptionalString(columnName);
        if (value is null)
        {
            return null;
        }

        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            throw new InvalidOperationException($"Column '{columnName}' contains an invalid integer value '{value}'.");
        }

        return parsed;
    }

    public decimal GetRequiredDecimal(string columnName)
    {
        var value = GetRequiredString(columnName);

        if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
        {
            throw new InvalidOperationException($"Column '{columnName}' contains an invalid decimal value '{value}'.");
        }

        return parsed;
    }

    public DateTimeOffset? GetOptionalDateTimeOffset(string columnName)
    {
        var value = GetOptionalString(columnName);
        if (value is null)
        {
            return null;
        }

        var formats = new[]
        {
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-dd HH:mm:ss.FFF",
            "yyyy-MM-dd HH:mm:ss.FFFFFF",
            "yyyy-MM-dd"
        };

        if (DateTimeOffset.TryParseExact(
                value,
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var exact))
        {
            return exact;
        }

        if (DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var parsed))
        {
            return parsed;
        }

        throw new InvalidOperationException($"Column '{columnName}' contains an invalid date value '{value}'.");
    }
}

using System.Text;
using Backend.Infrastructure.Persistence.Import.Abstractions;
using Microsoft.VisualBasic.FileIO;

namespace Backend.Infrastructure.Persistence.Import.Services;

public sealed class CsvDatasetReader : ICsvDatasetReader
{
    public Task<IReadOnlyList<T>> ReadAsync<T>(
        string path,
        Func<CsvRecord, T> map,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Could not find dataset file '{path}'.", path);
        }

        var results = new List<T>();

        using var parser = new TextFieldParser(path, Encoding.UTF8)
        {
            TextFieldType = FieldType.Delimited,
            HasFieldsEnclosedInQuotes = true,
            TrimWhiteSpace = false
        };
        parser.SetDelimiters(",");

        var headers = parser.ReadFields();
        if (headers is null || headers.Length == 0)
        {
            throw new InvalidOperationException($"Dataset file '{path}' does not contain a header row.");
        }

        while (!parser.EndOfData)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fields = parser.ReadFields();
            if (fields is null)
            {
                continue;
            }

            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < headers.Length; i++)
            {
                var value = i < fields.Length ? fields[i] : string.Empty;
                values[headers[i]] = value;
            }

            results.Add(map(new CsvRecord(values)));
        }

        return Task.FromResult<IReadOnlyList<T>>(results);
    }
}

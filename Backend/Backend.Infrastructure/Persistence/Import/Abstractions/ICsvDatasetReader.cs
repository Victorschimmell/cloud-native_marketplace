namespace Backend.Infrastructure.Persistence.Import.Abstractions;

public interface ICsvDatasetReader
{
    Task<IReadOnlyList<T>> ReadAsync<T>(
        string path,
        Func<CsvRecord, T> map,
        CancellationToken cancellationToken = default);
}

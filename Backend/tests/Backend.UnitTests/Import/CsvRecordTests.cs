using Backend.Infrastructure.Persistence.Import;

namespace Backend.UnitTests.Import;

public sealed class CsvRecordTests
{
    [Fact]
    public void GetOptionalDateTimeOffset_ParsesTimestampValue()
    {
        var record = new CsvRecord(new Dictionary<string, string>
        {
            ["timestamp"] = "2018-08-08 10:15:00"
        });

        var result = record.GetOptionalDateTimeOffset("timestamp");

        Assert.Equal(new DateTimeOffset(2018, 8, 8, 10, 15, 0, TimeSpan.Zero), result);
    }

    [Fact]
    public void GetRequiredDecimal_ParsesInvariantNumber()
    {
        var record = new CsvRecord(new Dictionary<string, string>
        {
            ["price"] = "123.45"
        });

        var result = record.GetRequiredDecimal("price");

        Assert.Equal(123.45m, result);
    }
}

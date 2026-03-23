using Backend.Domain.ValueObjects;

namespace Backend.UnitTests.ValueObjects;

public class ValueObjectTests
{
    [Fact]
    public void EmailAddress_ToString_ReturnsValue()
    {
        var email = new EmailAddress("user@example.com");

        Assert.Equal("user@example.com", email.ToString());
    }

    [Fact]
    public void Money_ToString_FormatsAmountAndCurrency()
    {
        var money = new Money(123.4m, "DKK");

        Assert.Equal("123.40 DKK", money.ToString());
    }

    [Fact]
    public void PersonName_FullName_CombinesFirstAndLastName()
    {
        var name = new PersonName("Victor", "LastName");

        Assert.Equal("Victor LastName", name.FullName);
    }

    [Fact]
    public void ProductDimensions_RecordEquality_WorksForSameValues()
    {
        var left = new ProductDimensions(10, 20, 30);
        var right = new ProductDimensions(10, 20, 30);

        Assert.Equal(left, right);
    }

    [Fact]
    public void GeoCoordinate_RecordEquality_WorksForSameValues()
    {
        var left = new GeoCoordinate(55.6761m, 12.5683m);
        var right = new GeoCoordinate(55.6761m, 12.5683m);

        Assert.Equal(left, right);
    }
}

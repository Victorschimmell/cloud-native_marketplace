using Backend.Domain.Enums;
using Backend.Infrastructure.Persistence.Import;

namespace Backend.UnitTests.Import;

public sealed class OlistImportValueMapperTests
{
    [Theory]
    [InlineData("delivered", OrderStatus.Delivered)]
    [InlineData("shipped", OrderStatus.Shipped)]
    [InlineData("canceled", OrderStatus.Cancelled)]
    [InlineData("processing", OrderStatus.Processing)]
    [InlineData("invoiced", OrderStatus.Approved)]
    public void MapOrderStatus_MapsKnownValues(string sourceStatus, OrderStatus expected)
    {
        var result = OlistImportValueMapper.MapOrderStatus(sourceStatus);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("credit_card", PaymentType.CreditCard)]
    [InlineData("debit_card", PaymentType.DebitCard)]
    [InlineData("voucher", PaymentType.Voucher)]
    [InlineData("boleto", PaymentType.BankTransfer)]
    [InlineData("not_defined", PaymentType.Other)]
    public void MapPaymentType_MapsKnownValues(string sourceType, PaymentType expected)
    {
        var result = OlistImportValueMapper.MapPaymentType(sourceType);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void BuildListingSku_CreatesDeterministicSku()
    {
        var sku = OlistImportValueMapper.BuildListingSku("seller-1", "product-1");

        Assert.Equal("OLIST-SELLER-1-PRODUCT-1", sku);
    }
}

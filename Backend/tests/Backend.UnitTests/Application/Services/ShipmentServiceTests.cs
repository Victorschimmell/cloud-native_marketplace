using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Services;
using Backend.Domain.Entities.Orders;
using Backend.Domain.Enums;
using Backend.UnitTests.Application.Fakes;

namespace Backend.UnitTests.Application.Services;

public sealed class ShipmentServiceTests
{
    [Fact]
    public async Task RecordShipmentAsync_WhenOrderDoesNotExist_ReturnsNotFoundAndWritesAuditLog()
    {
        var shipmentRepository = new FakeShipmentRepository();
        var auditLogService = new FakeAuditLogService();
        var service = CreateService(new FakeOrderRepository(), shipmentRepository, auditLogService: auditLogService);

        var result = await service.RecordShipmentAsync(CreateRecordShipmentRequest(), TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
        Assert.Equal(0, shipmentRepository.AddCalls);
        Assert.Single(auditLogService.Entries);
    }

    [Fact]
    public async Task RecordShipmentAsync_WhenOrderIsCancelled_ReturnsValidationFailureAndWritesAuditLog()
    {
        var order = CreateOrder(OrderStatus.Cancelled);
        var orderRepository = new FakeOrderRepository { Order = order };
        var shipmentRepository = new FakeShipmentRepository();
        var auditLogService = new FakeAuditLogService();
        var service = CreateService(orderRepository, shipmentRepository, auditLogService: auditLogService);

        var result = await service.RecordShipmentAsync(
            CreateRecordShipmentRequest(order.Id),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.ValidationFailure, result.FailureType);
        Assert.Equal(0, shipmentRepository.AddCalls);
        Assert.Contains("order is cancelled", Assert.Single(auditLogService.Entries).Details);
    }

    [Theory]
    [InlineData(ShipmentStatus.InTransit)]
    [InlineData(ShipmentStatus.Delivered)]
    [InlineData(ShipmentStatus.Returned)]
    [InlineData(ShipmentStatus.Pending)]
    public async Task RecordShipmentAsync_WhenOrderAllowsShipment_AddsShipmentWithStatusTimestamps(ShipmentStatus status)
    {
        var order = CreateOrder(OrderStatus.Processing);
        var orderRepository = new FakeOrderRepository { Order = order };
        var shipmentRepository = new FakeShipmentRepository();
        var unitOfWork = new FakeUnitOfWork();
        var auditLogService = new FakeAuditLogService();
        var service = CreateService(orderRepository, shipmentRepository, unitOfWork: unitOfWork, auditLogService: auditLogService);

        var result = await service.RecordShipmentAsync(
            CreateRecordShipmentRequest(order.Id, shipmentStatus: status),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, shipmentRepository.AddCalls);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
        Assert.Single(auditLogService.Entries);

        var shipment = Assert.Single(shipmentRepository.Shipments);
        Assert.Equal(status, shipment.ShipmentStatus);
        Assert.Equal(order.Id, shipment.OrderId);
        Assert.Equal("PostNord", shipment.CarrierName);
        Assert.Equal("TRACK-123", shipment.TrackingNumber);

        var expectedTimestamp = new DateTimeOffset(2026, 4, 9, 12, 0, 0, TimeSpan.Zero);
        if (status == ShipmentStatus.InTransit)
        {
            Assert.Equal(expectedTimestamp, shipment.ShippedAtUtc);
            Assert.Null(shipment.DeliveredAtUtc);
            Assert.Null(shipment.ReturnedAtUtc);
        }
        else if (status == ShipmentStatus.Delivered)
        {
            Assert.Null(shipment.ShippedAtUtc);
            Assert.Equal(expectedTimestamp, shipment.DeliveredAtUtc);
            Assert.Null(shipment.ReturnedAtUtc);
        }
        else if (status == ShipmentStatus.Returned)
        {
            Assert.Null(shipment.ShippedAtUtc);
            Assert.Null(shipment.DeliveredAtUtc);
            Assert.Equal(expectedTimestamp, shipment.ReturnedAtUtc);
        }
        else
        {
            Assert.Null(shipment.ShippedAtUtc);
            Assert.Null(shipment.DeliveredAtUtc);
            Assert.Null(shipment.ReturnedAtUtc);
        }
    }

    private static ShipmentService CreateService(
        FakeOrderRepository orderRepository,
        FakeShipmentRepository? shipmentRepository = null,
        FakeDateTimeProvider? dateTimeProvider = null,
        FakeAuditLogService? auditLogService = null,
        FakeUnitOfWork? unitOfWork = null) =>
        new(
            shipmentRepository ?? new FakeShipmentRepository(),
            orderRepository,
            dateTimeProvider ?? new FakeDateTimeProvider(),
            auditLogService ?? new FakeAuditLogService(),
            unitOfWork ?? new FakeUnitOfWork());

    private static RecordShipmentRequest CreateRecordShipmentRequest(
        Guid? orderId = null,
        Guid? sellerId = null,
        ShipmentStatus shipmentStatus = ShipmentStatus.InTransit) =>
        new(
            orderId ?? Guid.NewGuid(),
            sellerId ?? Guid.NewGuid(),
            "PostNord",
            "TRACK-123",
            shipmentStatus);

    private static Order CreateOrder(OrderStatus orderStatus) =>
        new()
        {
            CustomerId = Guid.NewGuid(),
            ShippingAddressId = Guid.NewGuid(),
            OrderNumber = "ORDER-123456",
            OrderStatus = orderStatus,
            OrderPurchaseTimestampUtc = new DateTimeOffset(2026, 4, 8, 12, 0, 0, TimeSpan.Zero),
            SubtotalAmount = 100m,
            FreightAmount = 20m,
            TotalAmount = 120m
        };
}

using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Entities.Orders;
using Backend.Domain.Enums;
namespace Backend.Application.Services;

public sealed class ShipmentService : IShipmentService
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuditLogService _auditLogService;
    private readonly IUnitOfWork _unitOfWork;

    public ShipmentService(
        IShipmentRepository shipmentRepository,
        IOrderRepository orderRepository,
        IDateTimeProvider dateTimeProvider,
        IAuditLogService auditLogService,
        IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(shipmentRepository);
        ArgumentNullException.ThrowIfNull(orderRepository);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        ArgumentNullException.ThrowIfNull(auditLogService);
        ArgumentNullException.ThrowIfNull(unitOfWork);

        _shipmentRepository = shipmentRepository;
        _orderRepository = orderRepository;
        _dateTimeProvider = dateTimeProvider;
        _auditLogService = auditLogService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> RecordShipmentAsync(RecordShipmentRequest request, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: AuditActionType.Created,
                TargetEntityType: nameof(Shipment),
                TargetEntityId: request.OrderId.ToString(),
                Outcome: AuditOutcome.Failed,
                Details: "Shipment recording failed: order not found."
            ), cancellationToken);

            return Result<ShipmentDto>.NotFound("Order was not found.");
        }

        if (order.OrderStatus == OrderStatus.Cancelled)
        {
            await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
                ActionType: AuditActionType.Created,
                TargetEntityType: nameof(Shipment),
                TargetEntityId: request.OrderId.ToString(),
                Outcome: AuditOutcome.Failed,
                Details: "Shipment recording failed: order is cancelled."
            ), cancellationToken);

            return Result<ShipmentDto>.ValidationFailure("Cannot record shipment for a cancelled order.");
        }

        var shipment = new Shipment
        {
            OrderId = request.OrderId,
            SellerId = request.SellerId,
            CarrierName = request.CarrierName,
            TrackingNumber = request.TrackingNumber,
            ShipmentStatus = request.ShipmentStatus
        };

        var now = _dateTimeProvider.UtcNow;
        if (shipment.ShipmentStatus == ShipmentStatus.InTransit)
        {
            shipment.ShippedAtUtc = now;
        }
        else if (shipment.ShipmentStatus == ShipmentStatus.Delivered)
        {
            shipment.DeliveredAtUtc = now;
        }
        else if (shipment.ShipmentStatus == ShipmentStatus.Returned)
        {
            shipment.ReturnedAtUtc = now;
        }

        await _shipmentRepository.AddAsync(shipment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogService.WriteEntryAsync(new WriteAuditLogEntryRequest(
            ActionType: AuditActionType.Created,
            TargetEntityType: nameof(Shipment),
            TargetEntityId: shipment.Id.ToString(),
            Outcome: AuditOutcome.Succeeded,
            Details: $"Shipment {shipment.Id} recorded for order {shipment.OrderId} and seller {shipment.SellerId} with status {shipment.ShipmentStatus} via {shipment.CarrierName}."
        ), cancellationToken);

        return Result.Success();
    }
}

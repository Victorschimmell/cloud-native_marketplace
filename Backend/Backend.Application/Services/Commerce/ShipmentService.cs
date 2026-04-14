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

    public ShipmentService(IShipmentRepository shipmentRepository, IOrderRepository orderRepository, IDateTimeProvider dateTimeProvider)
    {
        ArgumentNullException.ThrowIfNull(shipmentRepository);
        ArgumentNullException.ThrowIfNull(orderRepository);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);

        _shipmentRepository = shipmentRepository;
        _orderRepository = orderRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<IReadOnlyList<ShipmentDto>>> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            if (orderId == Guid.Empty)
            {
                return Result<IReadOnlyList<ShipmentDto>>.ValidationFailure("Order id is required.");
            }

            var shipments = await _shipmentRepository.GetByOrderIdAsync(orderId, cancellationToken);
            return Result<IReadOnlyList<ShipmentDto>>.Success(shipments.Select(static shipment => shipment.ToDto()).ToArray());
        }, "Unable to get shipments for order.");
    }

    public async Task<Result<ShipmentDto>> RecordShipmentAsync(RecordShipmentRequest request, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            if (request.OrderId == Guid.Empty || request.SellerId == Guid.Empty || string.IsNullOrWhiteSpace(request.CarrierName) || string.IsNullOrWhiteSpace(request.TrackingNumber))
            {
                return Result<ShipmentDto>.ValidationFailure("Order, seller, carrier, and tracking number are required.");
            }

            if (await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken) is null)
            {
                return Result<ShipmentDto>.NotFound("Order was not found.");
            }

            var shipment = new Shipment
            {
                OrderId = request.OrderId,
                SellerId = request.SellerId,
                CarrierName = request.CarrierName.Trim(),
                TrackingNumber = request.TrackingNumber.Trim(),
                ShipmentStatus = request.ShipmentStatus,
                ShippedAtUtc = _dateTimeProvider.UtcNow
            };

            await _shipmentRepository.AddAsync(shipment, cancellationToken);
            return Result<ShipmentDto>.Success(shipment.ToDto());
        }, "Unable to record shipment.");
    }

    public async Task<Result<ShipmentDto>> UpdateShipmentStatusAsync(UpdateShipmentStatusRequest request, CancellationToken cancellationToken = default)
    {
        return await ServiceExecution.ExecuteAsync(async () =>
        {
            if (request.ShipmentId == Guid.Empty)
            {
                return Result<ShipmentDto>.ValidationFailure("Shipment id is required.");
            }

            var shipment = await _shipmentRepository.GetByIdAsync(request.ShipmentId, cancellationToken);
            if (shipment is null)
            {
                return Result<ShipmentDto>.NotFound("Shipment was not found.");
            }

            shipment.ShipmentStatus = request.ShipmentStatus;
            if (request.ShipmentStatus == ShipmentStatus.Delivered)
            {
                shipment.DeliveredAtUtc = _dateTimeProvider.UtcNow;
            }

            await _shipmentRepository.UpdateAsync(shipment, cancellationToken);
            return Result<ShipmentDto>.Success(shipment.ToDto());
        }, "Unable to update shipment status.");
    }
}

using Backend.Application.Abstractions.Repositories;
using Backend.Application.Common.Abstractions;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;
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

    public Task<Result<IReadOnlyList<ShipmentDto>>> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<IReadOnlyList<ShipmentDto>>.NotImplemented());
    }

    public Task<Result<ShipmentDto>> RecordShipmentAsync(RecordShipmentRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<ShipmentDto>.NotImplemented());
    }

    public Task<Result<ShipmentDto>> UpdateShipmentStatusAsync(UpdateShipmentStatusRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<ShipmentDto>.NotImplemented());
    }
}

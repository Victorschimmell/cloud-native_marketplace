using Backend.Application.Common.Results;
using Backend.Application.DTOs;

namespace Backend.Application.Interfaces.Services;

public interface IShipmentService
{
    Task<Result<IReadOnlyList<ShipmentDto>>> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Result<ShipmentDto>> RecordShipmentAsync(RecordShipmentRequest request, CancellationToken cancellationToken = default);
    Task<Result<ShipmentDto>> UpdateShipmentStatusAsync(UpdateShipmentStatusRequest request, CancellationToken cancellationToken = default);
}

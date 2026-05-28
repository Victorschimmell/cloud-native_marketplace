using Backend.Application.Common.Results;
using Backend.Application.DTOs;

namespace Backend.Application.Interfaces.Services;

public interface IShipmentService
{
    Task<Result> RecordShipmentAsync(RecordShipmentRequest request, CancellationToken cancellationToken = default);
}

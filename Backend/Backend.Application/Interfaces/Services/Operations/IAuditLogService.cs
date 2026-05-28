using Backend.Application.Common.Models;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;

namespace Backend.Application.Interfaces.Services;

public interface IAuditLogService
{
    Task<Result> WriteEntryAsync(WriteAuditLogEntryRequest request, CancellationToken cancellationToken = default);
}

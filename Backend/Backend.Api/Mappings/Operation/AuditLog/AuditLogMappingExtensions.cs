using Backend.Api.Contracts.Common;
using Backend.Api.Contracts.Operation.AuditLog;
using App = Backend.Application.DTOs;

namespace Backend.Api.Mappings.Operation.AuditLog;

public static class AuditLogMappingExtensions
{
    public static App.GetAuditLogsRequest ToApplicationRequest(this GetAuditLogsRequest request, PageRequest pageRequest) =>
        new(
            request.ActorUserId,
            request.EntityType,
            request.EntityId,
            pageRequest.Page,
            pageRequest.PageSize);

    public static AuditLogEntryResponse ToResponse(this App.AuditLogEntryDto dto) =>
        new()
        {
            Id = dto.Id,
            ActorUserId = dto.ActorUserId,
            ActorIpAddress = dto.ActorIpAddress,
            ActionType = (AuditActionType)dto.ActionType,
            TargetEntityType = dto.TargetEntityType,
            TargetEntityId = dto.TargetEntityId,
            Outcome = (AuditOutcome)dto.Outcome,
            Details = dto.Details,
            CreatedAtUtc = dto.CreatedAtUtc
        };
}

using Backend.Application.Common.Results;
using Backend.Application.DTOs;

namespace Backend.Application.Interfaces.Services;

public interface IAdminDashboardService
{
    Task<Result<DashboardStatsDto>> GetDashboardStatsAsync(GetDashboardStatsRequest request, CancellationToken cancellationToken = default);
}

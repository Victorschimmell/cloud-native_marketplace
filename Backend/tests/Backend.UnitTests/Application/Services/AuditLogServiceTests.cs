using Backend.Application.DTOs;
using Backend.Application.Services;
using Backend.Domain.Enums;
using Backend.UnitTests.Application.Fakes;

namespace Backend.UnitTests.Application.Services;

public sealed class AuditLogServiceTests
{
	[Fact]
	public async Task WriteEntryAsync_PersistsAuditLogWithCurrentUserContext()
	{
		var repository = new FakeAuditLogRepository();
		var dateTimeProvider = new FakeDateTimeProvider();
		var currentUserProvider = new FakeCurrentUserProvider
		{
			UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
			IpAddress = "192.168.1.10"
		};
		var service = new AuditLogService(repository, dateTimeProvider, currentUserProvider);

		var request = new WriteAuditLogEntryRequest(
			ActionType: AuditActionType.Login,
			TargetEntityType: "UserAccount",
			TargetEntityId: "user-123",
			Outcome: AuditOutcome.Succeeded,
			Details: "Login succeeded");

		var result = await service.WriteEntryAsync(request, TestContext.Current.CancellationToken);

		Assert.True(result.IsSuccess);
		Assert.Equal(1, repository.AddCalls);
		var auditLog = Assert.Single(repository.AddedLogs);
		Assert.Equal(currentUserProvider.UserId, auditLog.ActorUserId);
		Assert.Equal(currentUserProvider.IpAddress, auditLog.ActorIpAddress);
		Assert.Equal(AuditActionType.Login, auditLog.ActionType);
		Assert.Equal("UserAccount", auditLog.TargetEntityType);
		Assert.Equal("user-123", auditLog.TargetEntityId);
		Assert.Equal(AuditOutcome.Succeeded, auditLog.Outcome);
		Assert.Equal("Login succeeded", auditLog.Details);
		Assert.Equal(dateTimeProvider.UtcNow, auditLog.CreatedAtUtc);
	}

	[Fact]
	public async Task WriteEntryAsync_AllowsMissingActorUserId()
	{
		var repository = new FakeAuditLogRepository();
		var dateTimeProvider = new FakeDateTimeProvider();
		var currentUserProvider = new FakeCurrentUserProvider
		{
			UserId = null,
			IpAddress = "0.0.0.0"
		};
		var service = new AuditLogService(repository, dateTimeProvider, currentUserProvider);

		var request = new WriteAuditLogEntryRequest(
			ActionType: AuditActionType.Created,
			TargetEntityType: "Order",
			TargetEntityId: "order-456",
			Outcome: AuditOutcome.Failed,
			Details: "Order creation failed");

		var result = await service.WriteEntryAsync(request, TestContext.Current.CancellationToken);

		Assert.True(result.IsSuccess);
		var auditLog = Assert.Single(repository.AddedLogs);
		Assert.Null(auditLog.ActorUserId);
		Assert.Equal("0.0.0.0", auditLog.ActorIpAddress);
		Assert.Equal(AuditOutcome.Failed, auditLog.Outcome);
	}
}

using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Services;
using Backend.Domain.Entities.Operations;
using Backend.Domain.Enums;
using Backend.UnitTests.Application.Fakes;

namespace Backend.UnitTests.Application.Services;

public sealed class AdminIssueServiceTests
{
    [Fact]
    public async Task ResolveAsync_Rejects_Open_Unassigned_Issue()
    {
        var issueRepository = new FakeAdminIssueRepository
        {
            Issue = CreateIssue(IssueStatus.Open)
        };
        var service = CreateService(issueRepository);

        var result = await service.ResolveAsync(
            new ResolveAdminIssueRequest(issueRepository.Issue.Id, null),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.Conflict, result.FailureType);
        Assert.Equal(IssueStatus.Open, issueRepository.Issue.Status);
        Assert.Equal(0, issueRepository.UpdateCalls);
    }

    [Fact]
    public async Task ResolveAsync_Rejects_Issue_Assigned_To_Another_Admin()
    {
        var issueRepository = new FakeAdminIssueRepository
        {
            Issue = CreateIssue(IssueStatus.InProgress, assignedToUserId: Guid.Parse("22222222-2222-2222-2222-222222222222"))
        };
        var service = CreateService(issueRepository);

        var result = await service.ResolveAsync(
            new ResolveAdminIssueRequest(issueRepository.Issue.Id, null),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.Conflict, result.FailureType);
        Assert.Equal(IssueStatus.InProgress, issueRepository.Issue.Status);
        Assert.Equal(0, issueRepository.UpdateCalls);
    }

    [Fact]
    public async Task ResolveAsync_Resolves_Issue_Assigned_To_Current_Admin()
    {
        var currentAdminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var issueRepository = new FakeAdminIssueRepository
        {
            Issue = CreateIssue(IssueStatus.InProgress, assignedToUserId: currentAdminId)
        };
        var service = CreateService(issueRepository);

        var result = await service.ResolveAsync(
            new ResolveAdminIssueRequest(issueRepository.Issue.Id, "Done"),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(IssueStatus.Resolved, issueRepository.Issue.Status);
        Assert.Equal(currentAdminId, issueRepository.Issue.ResolvedByUserId);
        Assert.Equal("Done", issueRepository.Issue.Resolution);
        Assert.Equal(1, issueRepository.UpdateCalls);
    }

    private static AdminIssueService CreateService(FakeAdminIssueRepository issueRepository)
    {
        return new AdminIssueService(
            issueRepository,
            new FakeCurrentUserProvider(),
            new FakeDateTimeProvider(),
            new FakeAuditLogService(),
            new FakeUnitOfWork());
    }

    private static AdminIssue CreateIssue(IssueStatus status, Guid? assignedToUserId = null)
    {
        return new AdminIssue
        {
            Title = "Issue",
            Description = "Description",
            Type = IssueType.System,
            Priority = IssuePriority.Medium,
            Status = status,
            ReportedByUserId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            AssignedToUserId = assignedToUserId
        };
    }
}

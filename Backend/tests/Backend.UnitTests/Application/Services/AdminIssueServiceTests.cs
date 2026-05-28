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
    public async Task CreateAsync_WhenUserIsNotAdmin_ReturnsForbidden()
    {
        var issueRepository = new FakeAdminIssueRepository();
        var auditLogService = new FakeAuditLogService();
        var service = CreateService(
            issueRepository,
            currentUserProvider: new FakeCurrentUserProvider { IsAdmin = false },
            auditLogService: auditLogService);

        var result = await service.CreateAsync(
            new CreateAdminIssueRequest("Title", "Description", IssueType.System, IssuePriority.High),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.Forbidden, result.FailureType);
        Assert.Equal(0, issueRepository.AddCalls);
        Assert.Single(auditLogService.Entries);
    }

    [Fact]
    public async Task CreateAsync_WhenTitleIsBlank_ReturnsValidationFailure()
    {
        var issueRepository = new FakeAdminIssueRepository();
        var service = CreateService(issueRepository);

        var result = await service.CreateAsync(
            new CreateAdminIssueRequest(" ", "Description", IssueType.System, IssuePriority.High),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.ValidationFailure, result.FailureType);
        Assert.Equal(0, issueRepository.AddCalls);
    }

    [Fact]
    public async Task CreateAsync_WhenDescriptionIsBlank_ReturnsValidationFailure()
    {
        var issueRepository = new FakeAdminIssueRepository();
        var service = CreateService(issueRepository);

        var result = await service.CreateAsync(
            new CreateAdminIssueRequest("Title", " ", IssueType.System, IssuePriority.High),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.ValidationFailure, result.FailureType);
        Assert.Equal(0, issueRepository.AddCalls);
    }

    [Fact]
    public async Task CreateAsync_WhenRequestIsValid_CreatesOpenIssue()
    {
        var currentAdminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var issueRepository = new FakeAdminIssueRepository();
        var unitOfWork = new FakeUnitOfWork();
        var auditLogService = new FakeAuditLogService();
        var service = CreateService(issueRepository, unitOfWork: unitOfWork, auditLogService: auditLogService);

        var result = await service.CreateAsync(
            new CreateAdminIssueRequest("  Payment stuck  ", "  Customer cannot pay  ", IssueType.Payment, IssuePriority.High),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, issueRepository.AddCalls);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
        Assert.Equal("Payment stuck", issueRepository.Issue!.Title);
        Assert.Equal("Customer cannot pay", issueRepository.Issue.Description);
        Assert.Equal(IssueStatus.Open, issueRepository.Issue.Status);
        Assert.Equal(currentAdminId, issueRepository.Issue.ReportedByUserId);
        Assert.Equal(IssueType.Payment, result.Value!.Type);
        Assert.Equal(IssuePriority.High, result.Value.Priority);
        Assert.Single(auditLogService.Entries);
    }

    [Fact]
    public async Task AssignAsync_WhenUserIsNotAdmin_ReturnsForbidden()
    {
        var issueRepository = new FakeAdminIssueRepository
        {
            Issue = CreateIssue(IssueStatus.Open)
        };
        var service = CreateService(issueRepository, currentUserProvider: new FakeCurrentUserProvider { IsAdmin = false });

        var result = await service.AssignAsync(
            new AssignAdminIssueRequest(issueRepository.Issue.Id),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.Forbidden, result.FailureType);
        Assert.Equal(IssueStatus.Open, issueRepository.Issue.Status);
        Assert.Equal(0, issueRepository.UpdateCalls);
    }

    [Fact]
    public async Task AssignAsync_WhenIssueDoesNotExist_ReturnsNotFound()
    {
        var issueRepository = new FakeAdminIssueRepository();
        var service = CreateService(issueRepository);

        var result = await service.AssignAsync(
            new AssignAdminIssueRequest(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
        Assert.Equal(0, issueRepository.UpdateCalls);
    }

    [Fact]
    public async Task AssignAsync_WhenIssueIsResolved_ReturnsConflict()
    {
        var issueRepository = new FakeAdminIssueRepository
        {
            Issue = CreateIssue(IssueStatus.Resolved)
        };
        var service = CreateService(issueRepository);

        var result = await service.AssignAsync(
            new AssignAdminIssueRequest(issueRepository.Issue.Id),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.Conflict, result.FailureType);
        Assert.Equal(0, issueRepository.UpdateCalls);
    }

    [Fact]
    public async Task AssignAsync_WhenIssueIsAlreadyAssigned_ReturnsConflict()
    {
        var issueRepository = new FakeAdminIssueRepository
        {
            Issue = CreateIssue(IssueStatus.InProgress, assignedToUserId: Guid.Parse("22222222-2222-2222-2222-222222222222"))
        };
        var service = CreateService(issueRepository);

        var result = await service.AssignAsync(
            new AssignAdminIssueRequest(issueRepository.Issue.Id),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.Conflict, result.FailureType);
        Assert.Equal(0, issueRepository.UpdateCalls);
    }

    [Fact]
    public async Task AssignAsync_WhenIssueIsOpen_AssignsToCurrentAdmin()
    {
        var currentAdminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var issueRepository = new FakeAdminIssueRepository
        {
            Issue = CreateIssue(IssueStatus.Open)
        };
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(issueRepository, unitOfWork: unitOfWork);

        var result = await service.AssignAsync(
            new AssignAdminIssueRequest(issueRepository.Issue.Id),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(IssueStatus.InProgress, issueRepository.Issue.Status);
        Assert.Equal(currentAdminId, issueRepository.Issue.AssignedToUserId);
        Assert.Equal(1, issueRepository.UpdateCalls);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
        Assert.Equal(currentAdminId, result.Value!.AssignedToUserId);
    }

    [Fact]
    public async Task ResolveAsync_WhenUserIsNotAdmin_ReturnsForbidden()
    {
        var issueRepository = new FakeAdminIssueRepository
        {
            Issue = CreateIssue(IssueStatus.InProgress)
        };
        var service = CreateService(issueRepository, currentUserProvider: new FakeCurrentUserProvider { IsAdmin = false });

        var result = await service.ResolveAsync(
            new ResolveAdminIssueRequest(issueRepository.Issue.Id, null),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.Forbidden, result.FailureType);
        Assert.Equal(0, issueRepository.UpdateCalls);
    }

    [Fact]
    public async Task ResolveAsync_WhenIssueDoesNotExist_ReturnsNotFound()
    {
        var issueRepository = new FakeAdminIssueRepository();
        var service = CreateService(issueRepository);

        var result = await service.ResolveAsync(
            new ResolveAdminIssueRequest(Guid.NewGuid(), null),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
        Assert.Equal(0, issueRepository.UpdateCalls);
    }

    [Fact]
    public async Task ResolveAsync_WhenIssueIsAlreadyResolved_ReturnsConflict()
    {
        var issueRepository = new FakeAdminIssueRepository
        {
            Issue = CreateIssue(IssueStatus.Resolved)
        };
        var service = CreateService(issueRepository);

        var result = await service.ResolveAsync(
            new ResolveAdminIssueRequest(issueRepository.Issue.Id, null),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.Conflict, result.FailureType);
        Assert.Equal(0, issueRepository.UpdateCalls);
    }

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
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(issueRepository, unitOfWork: unitOfWork);

        var result = await service.ResolveAsync(
            new ResolveAdminIssueRequest(issueRepository.Issue.Id, "Done"),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(IssueStatus.Resolved, issueRepository.Issue.Status);
        Assert.Equal(currentAdminId, issueRepository.Issue.ResolvedByUserId);
        Assert.Equal("Done", issueRepository.Issue.Resolution);
        Assert.Equal(1, issueRepository.UpdateCalls);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task ResolveAsync_WhenResolutionIsWhitespace_SavesNullResolution()
    {
        var currentAdminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var issueRepository = new FakeAdminIssueRepository
        {
            Issue = CreateIssue(IssueStatus.InProgress, assignedToUserId: currentAdminId)
        };
        var service = CreateService(issueRepository);

        var result = await service.ResolveAsync(
            new ResolveAdminIssueRequest(issueRepository.Issue.Id, "   "),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(IssueStatus.Resolved, issueRepository.Issue.Status);
        Assert.Null(issueRepository.Issue.Resolution);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserIsNotAdmin_ReturnsForbidden()
    {
        var issueRepository = new FakeAdminIssueRepository
        {
            Issue = CreateIssue(IssueStatus.Open)
        };
        var service = CreateService(issueRepository, currentUserProvider: new FakeCurrentUserProvider { IsAdmin = false });

        var result = await service.GetByIdAsync(issueRepository.Issue.Id, TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.Forbidden, result.FailureType);
    }

    [Fact]
    public async Task GetByIdAsync_WhenIssueDoesNotExist_ReturnsNotFound()
    {
        var service = CreateService(new FakeAdminIssueRepository());

        var result = await service.GetByIdAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.NotFound, result.FailureType);
    }

    [Fact]
    public async Task GetByIdAsync_WhenIssueExists_ReturnsIssue()
    {
        var issueRepository = new FakeAdminIssueRepository
        {
            Issue = CreateIssue(IssueStatus.Open)
        };
        var service = CreateService(issueRepository);

        var result = await service.GetByIdAsync(issueRepository.Issue.Id, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(issueRepository.Issue.Id, result.Value!.Id);
        Assert.Equal("Issue", result.Value.Title);
    }

    [Fact]
    public async Task GetIssuesAsync_WhenUserIsNotAdmin_ReturnsForbidden()
    {
        var service = CreateService(new FakeAdminIssueRepository(), currentUserProvider: new FakeCurrentUserProvider { IsAdmin = false });

        var result = await service.GetIssuesAsync(
            new GetAdminIssuesRequest(null, null, Page: 1, PageSize: 20),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.Forbidden, result.FailureType);
    }

    [Fact]
    public async Task GetIssuesAsync_WhenPaginationIsInvalid_ReturnsValidationFailure()
    {
        var service = CreateService(new FakeAdminIssueRepository());

        var result = await service.GetIssuesAsync(
            new GetAdminIssuesRequest(null, null, Page: 0, PageSize: 20),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureType.ValidationFailure, result.FailureType);
    }

    [Fact]
    public async Task GetIssuesAsync_WhenFiltersAreProvided_ReturnsMatchingIssues()
    {
        var issueRepository = new FakeAdminIssueRepository();
        issueRepository.Issues.Add(CreateIssue(IssueStatus.Open, priority: IssuePriority.High));
        issueRepository.Issues.Add(CreateIssue(IssueStatus.Resolved, priority: IssuePriority.High));
        issueRepository.Issues.Add(CreateIssue(IssueStatus.Open, priority: IssuePriority.Low));
        var service = CreateService(issueRepository);

        var result = await service.GetIssuesAsync(
            new GetAdminIssuesRequest(null, IssuePriority.High, UnresolvedOnly: true, Page: 1, PageSize: 20),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        var issue = Assert.Single(result.Value!.Items);
        Assert.Equal(IssuePriority.High, issue.Priority);
        Assert.Equal(IssueStatus.Open, issue.Status);
    }

    private static AdminIssueService CreateService(
        FakeAdminIssueRepository issueRepository,
        FakeCurrentUserProvider? currentUserProvider = null,
        FakeDateTimeProvider? dateTimeProvider = null,
        FakeAuditLogService? auditLogService = null,
        FakeUnitOfWork? unitOfWork = null)
    {
        return new AdminIssueService(
            issueRepository,
            currentUserProvider ?? new FakeCurrentUserProvider(),
            dateTimeProvider ?? new FakeDateTimeProvider(),
            auditLogService ?? new FakeAuditLogService(),
            unitOfWork ?? new FakeUnitOfWork());
    }

    private static AdminIssue CreateIssue(
        IssueStatus status,
        Guid? assignedToUserId = null,
        IssuePriority priority = IssuePriority.Medium)
    {
        return new AdminIssue
        {
            Title = "Issue",
            Description = "Description",
            Type = IssueType.System,
            Priority = priority,
            Status = status,
            ReportedByUserId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            AssignedToUserId = assignedToUserId
        };
    }
}

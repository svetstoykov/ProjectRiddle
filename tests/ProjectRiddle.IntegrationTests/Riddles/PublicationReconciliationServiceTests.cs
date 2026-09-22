using ProjectRiddle.Core.Constants.Riddles;
using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Models.Riddles.Authoring;
using ProjectRiddle.Core.Results.Models;
using ProjectRiddle.IntegrationTests.Harness;

namespace ProjectRiddle.IntegrationTests.Riddles;

/// <summary>
/// Verifies scheduled-publication reconciliation and the expired lifecycle.
/// </summary>
public sealed class PublicationReconciliationServiceTests
{
    private static readonly DateTimeOffset NoonUtcOnTwentieth =
        new(2026, 8, 20, 9, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset NoonUtcOnTwentySecond =
        new(2026, 8, 22, 9, 0, 0, TimeSpan.Zero);

    private static readonly DateOnly August20 = new(2026, 8, 20);

    private static readonly DateOnly August21 = new(2026, 8, 21);

    private static readonly DateOnly August22 = new(2026, 8, 22);

    private static readonly DateOnly August25 = new(2026, 8, 25);

    /// <summary>
    /// Verifies that a cycle with nothing due leaves every riddle unchanged.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task NothingDueIsANoOp()
    {
        var workspace = new TestWorkspace(NoonUtcOnTwentieth);
        var futureId = await ScheduleAsync(workspace, August25, "бъдеща бяла врана лети");

        var result = await workspace.Publication.ReconcileAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.PublishedRiddleId);
        Assert.Empty(result.Value.ExpiredRiddleIds);
        await AssertStateAsync(workspace, futureId, RiddlePublicationState.Scheduled, August25);
    }

    /// <summary>
    /// Verifies that a riddle scheduled for today is published without changing its date.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task TodaySchedulePublishesOnItsOwnDate()
    {
        var workspace = new TestWorkspace(NoonUtcOnTwentieth);
        var id = await ScheduleAsync(workspace, August20, "днешна бяла врана лети");

        var result = await workspace.Publication.ReconcileAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(id, result.Value!.PublishedRiddleId);
        Assert.Equal(August20, result.Value.PublishedDate);
        Assert.Empty(result.Value.ExpiredRiddleIds);
        await AssertStateAsync(workspace, id, RiddlePublicationState.Published, August20);

        var today = await workspace.Service.GetTodayAsync(CancellationToken.None);
        Assert.True(today.IsSuccess);
        Assert.Equal(id, today.Value!.Id);
    }

    /// <summary>
    /// Verifies that the latest due schedule publishes, older due schedules expire, and a future schedule stays.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task LatestDuePublishesAndOlderDueSchedulesExpire()
    {
        var workspace = new TestWorkspace(NoonUtcOnTwentieth);
        var olderId = await ScheduleAsync(workspace, August20, "първа бяла врана лети");
        var latestId = await ScheduleAsync(workspace, August21, "втора бяла врана лети");
        var futureId = await ScheduleAsync(workspace, August25, "трета бяла врана лети");
        workspace.Clock.UtcDateTime = NoonUtcOnTwentySecond;

        var result = await workspace.Publication.ReconcileAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(latestId, result.Value!.PublishedRiddleId);
        Assert.Equal(August21, result.Value.PublishedDate);
        Assert.Equal([olderId], result.Value.ExpiredRiddleIds);
        await AssertStateAsync(workspace, latestId, RiddlePublicationState.Published, August21);
        await AssertStateAsync(workspace, olderId, RiddlePublicationState.Expired, August20);
        await AssertStateAsync(workspace, futureId, RiddlePublicationState.Scheduled, August25);

        var today = await workspace.Service.GetTodayAsync(CancellationToken.None);
        Assert.True(today.IsFailure);
        Assert.Equal(RiddleErrorCodes.TodayUnavailable, today.Error!.Code);
    }

    /// <summary>
    /// Verifies that an already published riddle is left in place when a later schedule comes due.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task AlreadyPublishedRiddleIsNotExpired()
    {
        var workspace = new TestWorkspace(NoonUtcOnTwentieth);
        var publishedId = await ScheduleAsync(workspace, August20, "първа бяла врана лети");
        var published = await workspace.AdminService.PublishAsync(
            new PublishRiddleInput(publishedId, null),
            CancellationToken.None);
        Assert.True(published.IsSuccess);
        var scheduledId = await ScheduleAsync(workspace, August21, "втора бяла врана лети");
        workspace.Clock.UtcDateTime = NoonUtcOnTwentySecond;

        var result = await workspace.Publication.ReconcileAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(scheduledId, result.Value!.PublishedRiddleId);
        Assert.Empty(result.Value.ExpiredRiddleIds);
        await AssertStateAsync(workspace, publishedId, RiddlePublicationState.Published, August20);
        await AssertStateAsync(workspace, scheduledId, RiddlePublicationState.Published, August21);
    }

    /// <summary>
    /// Verifies that a repeated cycle does not publish or expire the same riddles again.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task RepeatedReconciliationIsANoOp()
    {
        var workspace = new TestWorkspace(NoonUtcOnTwentieth);
        var olderId = await ScheduleAsync(workspace, August20, "първа бяла врана лети");
        var latestId = await ScheduleAsync(workspace, August21, "втора бяла врана лети");
        workspace.Clock.UtcDateTime = NoonUtcOnTwentySecond;

        var first = await workspace.Publication.ReconcileAsync(CancellationToken.None);
        var second = await workspace.Publication.ReconcileAsync(CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Null(second.Value!.PublishedRiddleId);
        Assert.Empty(second.Value.ExpiredRiddleIds);
        await AssertStateAsync(workspace, latestId, RiddlePublicationState.Published, August21);
        await AssertStateAsync(workspace, olderId, RiddlePublicationState.Expired, August20);
    }

    /// <summary>
    /// Verifies that an expired riddle can be rescheduled and cannot be published or deleted in place.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task ExpiredRiddleCanBeRescheduledButNotPublishedOrDeleted()
    {
        var workspace = new TestWorkspace(NoonUtcOnTwentieth);
        var expiredId = await ScheduleAsync(workspace, August20, "първа бяла врана лети");
        await ScheduleAsync(workspace, August21, "втора бяла врана лети");
        workspace.Clock.UtcDateTime = NoonUtcOnTwentySecond;
        var reconciled = await workspace.Publication.ReconcileAsync(CancellationToken.None);
        Assert.True(reconciled.IsSuccess);

        var published = await workspace.AdminService.PublishAsync(
            new PublishRiddleInput(expiredId, August22),
            CancellationToken.None);
        var deleted = await workspace.AdminService.DeleteAsync(expiredId, CancellationToken.None);
        var past = await workspace.AdminService.ScheduleAsync(
            new ScheduleRiddleInput(expiredId, August20),
            CancellationToken.None);
        var rescheduled = await workspace.AdminService.ScheduleAsync(
            new ScheduleRiddleInput(expiredId, August22),
            CancellationToken.None);

        Assert.True(published.IsFailure);
        Assert.Equal(RiddleErrorCodes.TransitionInvalid, published.Error!.Code);
        Assert.True(deleted.IsFailure);
        Assert.Equal(RiddleErrorCodes.DeleteNotPermitted, deleted.Error!.Code);
        Assert.True(past.IsFailure);
        Assert.Equal(RiddleErrorCodes.PublicationDateInvalid, past.Error!.Code);
        Assert.True(rescheduled.IsSuccess);
        Assert.Equal(RiddlePublicationState.Scheduled, rescheduled.Value!.PublicationState);
        Assert.Equal(August22, rescheduled.Value.SofiaPublicationDate);
    }

    /// <summary>
    /// Verifies that a stale reconciliation batch is reloaded and then stored.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task StaleBatchIsRetried()
    {
        var workspace = new TestWorkspace(NoonUtcOnTwentieth);
        var id = await ScheduleAsync(workspace, August20, "днешна бяла врана лети");
        workspace.Riddles.FailNextBatchUpdateCount = 1;

        var result = await workspace.Publication.ReconcileAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(id, result.Value!.PublishedRiddleId);
        await AssertStateAsync(workspace, id, RiddlePublicationState.Published, August20);
    }

    /// <summary>
    /// Verifies that a batch which stays stale is not partially applied.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task ExhaustedStaleBatchLeavesSchedulesUnchanged()
    {
        var workspace = new TestWorkspace(NoonUtcOnTwentieth);
        var olderId = await ScheduleAsync(workspace, August20, "първа бяла врана лети");
        var latestId = await ScheduleAsync(workspace, August21, "втора бяла врана лети");
        workspace.Clock.UtcDateTime = NoonUtcOnTwentySecond;
        workspace.Riddles.FailNextBatchUpdateCount = PublicationReconciliationLimits.WriteRetryLimit;

        var result = await workspace.Publication.ReconcileAsync(CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal(RiddleErrorCodes.StaleWrite, result.Error.Code);
        await AssertStateAsync(workspace, olderId, RiddlePublicationState.Scheduled, August20);
        await AssertStateAsync(workspace, latestId, RiddlePublicationState.Scheduled, August21);
    }

    /// <summary>
    /// Verifies that an administrative publish reports a stale write instead of overwriting the stored riddle.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task StaleAdministrativeWriteIsRejected()
    {
        var workspace = new TestWorkspace(NoonUtcOnTwentieth);
        var id = await ScheduleAsync(workspace, August25, "бъдеща бяла врана лети");
        workspace.Riddles.FailNextUpdateCount = 1;

        var published = await workspace.AdminService.PublishAsync(
            new PublishRiddleInput(id, null),
            CancellationToken.None);

        Assert.True(published.IsFailure);
        Assert.Equal(ErrorType.Conflict, published.Error!.Type);
        Assert.Equal(RiddleErrorCodes.StaleWrite, published.Error.Code);
        await AssertStateAsync(workspace, id, RiddlePublicationState.Scheduled, August25);
    }

    private static async Task<Guid> ScheduleAsync(TestWorkspace workspace, DateOnly date, string clue)
    {
        var created = await workspace.AdminService.CreateAsync(
            TestWorkspace.CreateRiddleInput(clue: clue),
            CancellationToken.None);
        Assert.True(created.IsSuccess);

        var scheduled = await workspace.AdminService.ScheduleAsync(
            new ScheduleRiddleInput(created.Value!.Id, date),
            CancellationToken.None);
        Assert.True(scheduled.IsSuccess);
        return created.Value.Id;
    }

    private static async Task AssertStateAsync(
        TestWorkspace workspace,
        Guid id,
        RiddlePublicationState state,
        DateOnly date)
    {
        var loaded = await workspace.AdminService.GetByIdAsync(id, CancellationToken.None);
        Assert.True(loaded.IsSuccess);
        Assert.Equal(state, loaded.Value!.PublicationState);
        Assert.Equal(date, loaded.Value.SofiaPublicationDate);
    }
}

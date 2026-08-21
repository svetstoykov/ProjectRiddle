using Microsoft.Extensions.DependencyInjection;
using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Interfaces.Services;
using ProjectRiddle.Core.Interfaces.Time;
using ProjectRiddle.Core.Models.Riddles;
using ProjectRiddle.Core.Results.Models;
using ProjectRiddle.Core.Services.Riddles;
using ProjectRiddle.IntegrationTests.Harness;

namespace ProjectRiddle.IntegrationTests.Riddles;

/// <summary>
/// Verifies riddle authoring, range validation, and publication transitions.
/// </summary>
public sealed class RiddlesServiceTests
{
    private static readonly DateTimeOffset NoonUtcOnTwentieth =
        new(2026, 8, 20, 9, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Verifies that a valid riddle can be created as a draft.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task CreatePersistsADraftRiddle()
    {
        await using var workspace = TestWorkspace.Create(NoonUtcOnTwentieth);
        var (scope, riddles) = workspace.GetScopedService<IRiddlesService>();
        using (scope)
        {
            var result = await riddles.CreateAsync(TestWorkspace.CreateRiddleInput(), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(RiddlePublicationState.Draft, result.Value!.PublicationState);
            Assert.Null(result.Value.SofiaPublicationDate);
            Assert.Equal(2, result.Value.Ranges.Count);
        }
    }

    /// <summary>
    /// Verifies that an answer pattern that does not match the answer is rejected.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task InvalidAnswerPatternIsRejected()
    {
        await using var workspace = TestWorkspace.Create(NoonUtcOnTwentieth);
        var (scope, riddles) = workspace.GetScopedService<IRiddlesService>();
        using (scope)
        {
            var result = await riddles.CreateAsync(
                TestWorkspace.CreateRiddleInput(answerPattern: "3,2"),
                CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.Error!.Type);
            Assert.Equal(RiddleErrorCodes.AnswerPatternInvalid, result.Error.Code);
        }
    }

    /// <summary>
    /// Verifies that a range outside the clue is rejected.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task InvalidRangeIsRejected()
    {
        await using var workspace = TestWorkspace.Create(NoonUtcOnTwentieth);
        var (scope, riddles) = workspace.GetScopedService<IRiddlesService>();
        using (scope)
        {
            var input = new CreateRiddleInput(
                "бяла врана",
                "бяла врана",
                "4,5",
                "Обяснение на уликата.",
                [new RiddleRangeInput(RiddleRangeKind.Definition, 0, 40)]);

            var result = await riddles.CreateAsync(input, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.Error!.Type);
            Assert.Equal(RiddleErrorCodes.RangeInvalid, result.Error.Code);
        }
    }

    /// <summary>
    /// Verifies legal schedule, publish, and unpublish transitions.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task LegalPublicationTransitionsWork()
    {
        await using var workspace = TestWorkspace.Create(NoonUtcOnTwentieth);
        var (scope, riddles) = workspace.GetScopedService<IRiddlesService>();
        using (scope)
        {
            var created = await riddles.CreateAsync(TestWorkspace.CreateRiddleInput(), CancellationToken.None);
            Assert.True(created.IsSuccess);
            var id = created.Value!.Id;
            var date = new DateOnly(2026, 8, 25);

            var scheduled = await riddles.ScheduleAsync(new ScheduleRiddleInput(id, date), CancellationToken.None);
            Assert.True(scheduled.IsSuccess);
            Assert.Equal(RiddlePublicationState.Scheduled, scheduled.Value!.PublicationState);
            Assert.Equal(date, scheduled.Value.SofiaPublicationDate);

            var published = await riddles.PublishAsync(new PublishRiddleInput(id, null), CancellationToken.None);
            Assert.True(published.IsSuccess);
            Assert.Equal(RiddlePublicationState.Published, published.Value!.PublicationState);
            Assert.Equal(date, published.Value.SofiaPublicationDate);

            var unpublished = await riddles.UnpublishAsync(id, CancellationToken.None);
            Assert.True(unpublished.IsSuccess);
            Assert.Equal(RiddlePublicationState.Unpublished, unpublished.Value!.PublicationState);
            Assert.Equal(date, unpublished.Value.SofiaPublicationDate);
        }
    }

    /// <summary>
    /// Verifies that two scheduled or published riddles cannot share a Sofia date.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task DuplicateSofiaDatesConflict()
    {
        await using var workspace = TestWorkspace.Create(NoonUtcOnTwentieth);
        var (scope, riddles) = workspace.GetScopedService<IRiddlesService>();
        using (scope)
        {
            var first = await riddles.CreateAsync(TestWorkspace.CreateRiddleInput(), CancellationToken.None);
            var second = await riddles.CreateAsync(
                TestWorkspace.CreateRiddleInput(clue: "втора бяла врана лети"),
                CancellationToken.None);
            Assert.True(first.IsSuccess);
            Assert.True(second.IsSuccess);

            var date = new DateOnly(2026, 8, 25);
            var scheduled = await riddles.ScheduleAsync(
                new ScheduleRiddleInput(first.Value!.Id, date),
                CancellationToken.None);
            var conflict = await riddles.ScheduleAsync(
                new ScheduleRiddleInput(second.Value!.Id, date),
                CancellationToken.None);

            Assert.True(scheduled.IsSuccess);
            Assert.True(conflict.IsFailure);
            Assert.Equal(ErrorType.Conflict, conflict.Error!.Type);
            Assert.Equal(RiddleErrorCodes.PublicationDateConflict, conflict.Error.Code);

            var unpublished = await riddles.UnpublishAsync(first.Value.Id, CancellationToken.None);
            var reused = await riddles.ScheduleAsync(
                new ScheduleRiddleInput(second.Value.Id, date),
                CancellationToken.None);

            Assert.True(unpublished.IsSuccess);
            Assert.True(reused.IsSuccess);
        }
    }

    /// <summary>
    /// Verifies that scheduled and published riddles cannot be deleted.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task DeleteIsNotPermittedForScheduledOrPublishedRiddles()
    {
        await using var workspace = TestWorkspace.Create(NoonUtcOnTwentieth);
        var (scope, riddles) = workspace.GetScopedService<IRiddlesService>();
        using (scope)
        {
            var created = await riddles.CreateAsync(TestWorkspace.CreateRiddleInput(), CancellationToken.None);
            var scheduled = await riddles.ScheduleAsync(
                new ScheduleRiddleInput(created.Value!.Id, new DateOnly(2026, 8, 25)),
                CancellationToken.None);
            var deleted = await riddles.DeleteAsync(created.Value.Id, CancellationToken.None);

            Assert.True(scheduled.IsSuccess);
            Assert.True(deleted.IsFailure);
            Assert.Equal(ErrorType.InvalidOperation, deleted.Error!.Type);
            Assert.Equal(RiddleErrorCodes.DeleteNotPermitted, deleted.Error.Code);
        }
    }

    /// <summary>
    /// Verifies that a Sofia date boundary changes scheduling eligibility.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task SofiaDateBoundaryControlsScheduling()
    {
        var beforeMidnightUtc = new DateTimeOffset(2026, 8, 20, 20, 59, 59, TimeSpan.Zero);
        await using var workspace = TestWorkspace.Create(beforeMidnightUtc);
        var clock = workspace.Factory.Services.GetRequiredService<FixedDateTimeProvider>();
        var (scope, riddles) = workspace.GetScopedService<IRiddlesService>();
        using (scope)
        {
            Assert.Equal(new DateOnly(2026, 8, 20), clock.LocalDate);

            var created = await riddles.CreateAsync(TestWorkspace.CreateRiddleInput(), CancellationToken.None);
            var scheduledToday = await riddles.ScheduleAsync(
                new ScheduleRiddleInput(created.Value!.Id, new DateOnly(2026, 8, 20)),
                CancellationToken.None);
            Assert.True(scheduledToday.IsSuccess);

            clock.UtcDateTime = new DateTimeOffset(2026, 8, 20, 21, 0, 0, TimeSpan.Zero);
            Assert.Equal(new DateOnly(2026, 8, 21), clock.LocalDate);

            var another = await riddles.CreateAsync(
                TestWorkspace.CreateRiddleInput(clue: "втора бяла врана лети"),
                CancellationToken.None);
            var scheduledYesterday = await riddles.ScheduleAsync(
                new ScheduleRiddleInput(another.Value!.Id, new DateOnly(2026, 8, 20)),
                CancellationToken.None);

            Assert.True(scheduledYesterday.IsFailure);
            Assert.Equal(ErrorType.Validation, scheduledYesterday.Error!.Type);
            Assert.Equal(RiddleErrorCodes.PublicationDateInvalid, scheduledYesterday.Error.Code);
        }
    }

    /// <summary>
    /// Verifies that content can be updated without changing publication state.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task UpdateChangesContentWithoutChangingPublicationState()
    {
        await using var workspace = TestWorkspace.Create(NoonUtcOnTwentieth);
        var (scope, riddles) = workspace.GetScopedService<IRiddlesService>();
        using (scope)
        {
            var created = await riddles.CreateAsync(TestWorkspace.CreateRiddleInput(), CancellationToken.None);
            var scheduled = await riddles.ScheduleAsync(
                new ScheduleRiddleInput(created.Value!.Id, new DateOnly(2026, 8, 25)),
                CancellationToken.None);
            var updated = await riddles.UpdateAsync(
                new UpdateRiddleInput(
                    created.Value.Id,
                    "нова бяла врана лети",
                    "нова врана",
                    "4,5",
                    "Ново обяснение.",
                    [new RiddleRangeInput(RiddleRangeKind.Definition, 0, 4)]),
                CancellationToken.None);

            Assert.True(scheduled.IsSuccess);
            Assert.True(updated.IsSuccess);
            Assert.Equal(RiddlePublicationState.Scheduled, updated.Value!.PublicationState);
            Assert.Equal("нова бяла врана лети", updated.Value.Clue);
            Assert.Equal("нова врана", updated.Value.Answer);
        }
    }
}

using ProjectRiddle.Core.Constants.Riddles;
using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Models.Riddles;
using ProjectRiddle.Core.Results.Models;
using ProjectRiddle.Core.Validators.Riddles;
using ProjectRiddle.IntegrationTests.Harness;

namespace ProjectRiddle.IntegrationTests.Riddles;

/// <summary>
/// Verifies public riddle eligibility, play commands, and monotonic progress.
/// </summary>
public sealed class PublicRiddlesServiceTests
{
    private static readonly DateTimeOffset NoonUtcOnTwentieth =
        new(2026, 8, 20, 9, 0, 0, TimeSpan.Zero);

    private static readonly DateOnly Today = new(2026, 8, 20);

    private static readonly Guid AccountId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    /// <summary>
    /// Verifies that only a published riddle for the current local date is today's public riddle.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task TodayRequiresAPublishedRiddleOnTheLocalDate()
    {
        var workspace = new TestWorkspace(NoonUtcOnTwentieth);
        var missing = await workspace.PublicService.GetTodayAsync(CancellationToken.None);
        Assert.True(missing.IsFailure);
        Assert.Equal(RiddleErrorCodes.TodayUnavailable, missing.Error!.Code);

        var created = await workspace.Service.CreateAsync(TestWorkspace.CreateRiddleInput(), CancellationToken.None);
        var scheduled = await workspace.Service.ScheduleAsync(
            new ScheduleRiddleInput(created.Value!.Id, Today),
            CancellationToken.None);
        Assert.True(scheduled.IsSuccess);

        var stillMissing = await workspace.PublicService.GetTodayAsync(CancellationToken.None);
        Assert.True(stillMissing.IsFailure);
        Assert.Equal(RiddleErrorCodes.TodayUnavailable, stillMissing.Error!.Code);

        var published = await workspace.Service.PublishAsync(
            new PublishRiddleInput(created.Value.Id, null),
            CancellationToken.None);
        Assert.True(published.IsSuccess);

        var today = await workspace.PublicService.GetTodayAsync(CancellationToken.None);
        Assert.True(today.IsSuccess);
        Assert.Equal(created.Value.Id, today.Value!.Id);
        Assert.Equal(Today, today.Value.PublicationDate);
        Assert.Equal("бяла врана лети високо", today.Value.Clue);
        Assert.Equal("4,5", today.Value.AnswerPattern);
    }

    /// <summary>
    /// Verifies that an anonymous caller cannot play a riddle after the local date rolls over.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task LocalDateRolloverTurnsTodayIntoArchive()
    {
        var workspace = new TestWorkspace(NoonUtcOnTwentieth);
        var published = await PublishAsync(workspace, Today);

        workspace.Clock.UtcDateTime = new DateTimeOffset(2026, 8, 20, 21, 0, 0, TimeSpan.Zero);
        Assert.Equal(new DateOnly(2026, 8, 21), workspace.Clock.LocalDate);

        var today = await workspace.PublicService.GetTodayAsync(CancellationToken.None);
        Assert.True(today.IsFailure);
        Assert.Equal(RiddleErrorCodes.TodayUnavailable, today.Error!.Code);

        var anonymousPlay = await workspace.PublicService.GetPlayAsync(published.Id, CancellationToken.None);
        Assert.True(anonymousPlay.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, anonymousPlay.Error!.Type);
        Assert.Equal(RiddleErrorCodes.ArchiveAuthenticationRequired, anonymousPlay.Error.Code);

        workspace.Account.AccountId = AccountId;
        var authenticated = await workspace.PublicService.GetPlayAsync(published.Id, CancellationToken.None);
        Assert.True(authenticated.IsSuccess);
        Assert.Equal(published.Id, authenticated.Value!.Id);
    }

    /// <summary>
    /// Verifies that public discovery projections omit answer-sensitive fields.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task DiscoveryProjectionsAreSafe()
    {
        var workspace = new TestWorkspace(NoonUtcOnTwentieth);
        var archive = await PublishAsync(workspace, new DateOnly(2026, 8, 19), "архивна бяла врана лети");
        var today = await PublishAsync(workspace, Today);

        var list = await workspace.PublicService.ListArchiveAsync(
            new ListPublicRiddlesInput(1, 31),
            CancellationToken.None);
        Assert.True(list.IsSuccess);
        Assert.Equal(1, list.Value!.TotalCount);
        Assert.Equal(archive.Id, list.Value.Items[0].Id);
        Assert.Equal("архивна бяла врана лети", list.Value.Items[0].ClueExcerpt);
        Assert.DoesNotContain("Обяснение", list.Value.Items[0].ClueExcerpt);

        var week = await workspace.PublicService.ListWeekAsync(CancellationToken.None);
        Assert.True(week.IsSuccess);
        Assert.Equal(2, week.Value!.Count);
        Assert.Contains(week.Value, item => item.Id == archive.Id);
        Assert.Contains(week.Value, item => item.Id == today.Id);

        var play = await workspace.PublicService.GetTodayAsync(CancellationToken.None);
        Assert.True(play.IsSuccess);
        Assert.All(play.Value!.Ranges, range => Assert.True(Enum.IsDefined(range.Kind)));
    }

    /// <summary>
    /// Verifies normalized incorrect and correct answers, attempt counting, and terminal idempotency.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task AnswersAreNormalizedAndTerminalRetriesAreIdempotent()
    {
        var workspace = new TestWorkspace(NoonUtcOnTwentieth, AccountId);
        var published = await PublishAsync(workspace, Today);

        var incorrect = await workspace.PublicService.SubmitAnswerAsync(
            new SubmitRiddleAnswerInput(published.Id, "грешен отговор", null),
            CancellationToken.None);
        Assert.True(incorrect.IsSuccess);
        Assert.False(incorrect.Value!.IsCorrect);
        Assert.Equal(1, incorrect.Value.Progress.AnswerAttemptCount);
        Assert.Equal(RiddleProgressStatus.InProgress, incorrect.Value.Progress.Status);
        Assert.Null(incorrect.Value.Answer);
        Assert.Null(incorrect.Value.Explanation);

        var correct = await workspace.PublicService.SubmitAnswerAsync(
            new SubmitRiddleAnswerInput(published.Id, "  бяла   врана ", null),
            CancellationToken.None);
        Assert.True(correct.IsSuccess);
        Assert.True(correct.Value!.IsCorrect);
        Assert.Equal(2, correct.Value.Progress.AnswerAttemptCount);
        Assert.Equal(RiddleProgressStatus.Solved, correct.Value.Progress.Status);
        Assert.Equal(AnswerNormalizer.Normalize("бяла врана"), correct.Value.Answer);
        Assert.Equal("Обяснение на уликата.", correct.Value.Explanation);

        var retry = await workspace.PublicService.SubmitAnswerAsync(
            new SubmitRiddleAnswerInput(published.Id, "грешен", null),
            CancellationToken.None);
        Assert.True(retry.IsSuccess);
        Assert.False(retry.Value!.IsCorrect);
        Assert.Equal(2, retry.Value.Progress.AnswerAttemptCount);
        Assert.Equal(RiddleProgressStatus.Solved, retry.Value.Progress.Status);
        Assert.Equal(AnswerNormalizer.Normalize("бяла врана"), retry.Value.Answer);
    }

    /// <summary>
    /// Verifies that each structural hint kind is recorded once.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task StructuralHintsAreRecordedOnce()
    {
        var workspace = new TestWorkspace(NoonUtcOnTwentieth, AccountId);
        var published = await PublishAsync(workspace, Today);

        var first = await workspace.PublicService.UseHintAsync(
            new UseRiddleHintInput(published.Id, RiddleRangeKind.Definition, null),
            CancellationToken.None);
        var repeat = await workspace.PublicService.UseHintAsync(
            new UseRiddleHintInput(published.Id, RiddleRangeKind.Definition, null),
            CancellationToken.None);
        var indicator = await workspace.PublicService.UseHintAsync(
            new UseRiddleHintInput(published.Id, RiddleRangeKind.Indicator, null),
            CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.True(repeat.IsSuccess);
        Assert.True(indicator.IsSuccess);
        Assert.Equal(new[] { RiddleRangeKind.Definition }, first.Value!.Progress.UsedHints);
        Assert.Equal(new[] { RiddleRangeKind.Definition }, repeat.Value!.Progress.UsedHints);
        Assert.Equal(
            new[] { RiddleRangeKind.Definition, RiddleRangeKind.Indicator },
            indicator.Value!.Progress.UsedHints);
        Assert.Null(indicator.Value.Answer);
    }

    /// <summary>
    /// Verifies random unique letter reveals, separate reveal counts, complete reveal, and solved precedence.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task LetterRevealsAreUniqueAndSolvedTakesPrecedence()
    {
        var workspace = new TestWorkspace(
            NoonUtcOnTwentieth,
            AccountId,
            new ScriptedRandomNumberGenerator(3, 0));
        var published = await PublishAsync(workspace, Today);

        var first = await workspace.PublicService.RevealLetterAsync(
            new RevealRiddleLetterInput(published.Id, null),
            CancellationToken.None);
        Assert.True(first.IsSuccess);
        Assert.Equal(3, first.Value!.Progress.RevealedPositions.Single());
        Assert.Equal(1, first.Value.Progress.LetterRevealCount);
        Assert.Equal(3, first.Value.RevealedLetters[0].Position);
        Assert.Null(first.Value.Answer);

        var second = await workspace.PublicService.RevealLetterAsync(
            new RevealRiddleLetterInput(published.Id, null),
            CancellationToken.None);
        Assert.True(second.IsSuccess);
        Assert.Equal(0, second.Value!.Progress.RevealedPositions[0]);
        Assert.Equal(3, second.Value.Progress.RevealedPositions[1]);
        Assert.Equal(2, second.Value.Progress.LetterRevealCount);

        var remainingWorkspace = new TestWorkspace(NoonUtcOnTwentieth, AccountId);
        var remaining = await PublishAsync(remainingWorkspace, Today);
        RiddlePlayStateOutput? last = null;
        for (var index = 0; index < 9; index++)
        {
            var revealed = await remainingWorkspace.PublicService.RevealLetterAsync(
                new RevealRiddleLetterInput(remaining.Id, null),
                CancellationToken.None);
            Assert.True(revealed.IsSuccess);
            last = revealed.Value;
        }

        Assert.NotNull(last);
        Assert.Equal(RiddleProgressStatus.FullyRevealed, last.Progress.Status);
        Assert.Equal(9, last.Progress.LetterRevealCount);
        Assert.Equal(AnswerNormalizer.Normalize("бяла врана"), last.Answer);

        var solved = await remainingWorkspace.PublicService.SubmitAnswerAsync(
            new SubmitRiddleAnswerInput(remaining.Id, "бяла врана", null),
            CancellationToken.None);
        Assert.True(solved.IsSuccess);
        Assert.Equal(RiddleProgressStatus.FullyRevealed, solved.Value!.Progress.Status);
        Assert.Equal(9, solved.Value.Progress.LetterRevealCount);
        Assert.True(solved.Value.IsCorrect);
    }

    /// <summary>
    /// Verifies anonymous resume validation and rehydration of permitted characters.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task AnonymousResumeValidatesAndRehydratesPermittedLetters()
    {
        var workspace = new TestWorkspace(NoonUtcOnTwentieth);
        var published = await PublishAsync(workspace, Today);
        var snapshot = new AnonymousRiddleProgressInput(
            PublicRiddleLimits.AnonymousProgressSchemaVersion,
            published.Id,
            Today,
            RiddleProgressStatus.InProgress,
            2,
            [RiddleRangeKind.Fodder],
            [0, 4]);

        var resumed = await workspace.PublicService.ResumeAsync(
            new ResumeRiddleInput(published.Id, snapshot),
            CancellationToken.None);
        Assert.True(resumed.IsSuccess);
        Assert.Equal(2, resumed.Value!.Progress.AnswerAttemptCount);
        Assert.Equal(0, resumed.Value.Progress.RevealedPositions[0]);
        Assert.Equal(4, resumed.Value.Progress.RevealedPositions[1]);
        Assert.Equal(2, resumed.Value.RevealedLetters.Count);
        Assert.Null(resumed.Value.Answer);

        var invalid = await workspace.PublicService.ResumeAsync(
            new ResumeRiddleInput(
                published.Id,
                snapshot with { RevealedPositions = [99] }),
            CancellationToken.None);
        Assert.True(invalid.IsFailure);
        Assert.Equal(RiddleErrorCodes.ProgressPositionInvalid, invalid.Error!.Code);
    }

    /// <summary>
    /// Verifies import validation, union and maximum merges, solved precedence, and deleted-riddle rejection.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task ImportMergesMonotonicallyAndRejectsDeletedRiddles()
    {
        var workspace = new TestWorkspace(NoonUtcOnTwentieth, AccountId);
        var published = await PublishAsync(workspace, Today);
        var first = new AnonymousRiddleProgressInput(
            PublicRiddleLimits.AnonymousProgressSchemaVersion,
            published.Id,
            Today,
            RiddleProgressStatus.FullyRevealed,
            3,
            [RiddleRangeKind.Definition],
            [0, 1, 2, 3, 4, 5, 6, 7, 8]);
        var imported = await workspace.PublicService.ImportProgressAsync(first, CancellationToken.None);
        Assert.True(imported.IsSuccess);
        Assert.Equal(RiddleProgressStatus.FullyRevealed, imported.Value!.Status);

        var stale = first with
        {
            Status = RiddleProgressStatus.InProgress,
            AnswerAttemptCount = 1,
            UsedHints = [RiddleRangeKind.Indicator],
            RevealedPositions = [1, 2]
        };
        var merged = await workspace.PublicService.ImportProgressAsync(stale, CancellationToken.None);
        Assert.True(merged.IsSuccess);
        Assert.Equal(RiddleProgressStatus.FullyRevealed, merged.Value!.Status);
        Assert.Equal(3, merged.Value.AnswerAttemptCount);
        Assert.Equal(
            new[] { RiddleRangeKind.Definition, RiddleRangeKind.Indicator },
            merged.Value.UsedHints);
        Assert.Equal(9, merged.Value.LetterRevealCount);

        var solvedImport = await workspace.PublicService.ImportProgressAsync(
            first with { Status = RiddleProgressStatus.Solved, RevealedPositions = [0] },
            CancellationToken.None);
        Assert.True(solvedImport.IsSuccess);
        Assert.Equal(RiddleProgressStatus.Solved, solvedImport.Value!.Status);

        var unpublished = await workspace.Service.UnpublishAsync(published.Id, CancellationToken.None);
        var deleted = await workspace.Service.DeleteAsync(published.Id, CancellationToken.None);
        Assert.True(unpublished.IsSuccess);
        Assert.True(deleted.IsSuccess);

        var afterDelete = await workspace.PublicService.ImportProgressAsync(first, CancellationToken.None);
        Assert.True(afterDelete.IsFailure);
        Assert.Equal(RiddleErrorCodes.ProgressReferenceInvalid, afterDelete.Error!.Code);
    }

    /// <summary>
    /// Verifies that empty answers and unknown hint kinds are rejected.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task InvalidPlayCommandsAreRejected()
    {
        var workspace = new TestWorkspace(NoonUtcOnTwentieth, AccountId);
        var published = await PublishAsync(workspace, Today);

        var empty = await workspace.PublicService.SubmitAnswerAsync(
            new SubmitRiddleAnswerInput(published.Id, "   ", null),
            CancellationToken.None);
        Assert.True(empty.IsFailure);
        Assert.Equal(RiddleErrorCodes.AnswerRequestInvalid, empty.Error!.Code);

        var hint = await workspace.PublicService.UseHintAsync(
            new UseRiddleHintInput(published.Id, (RiddleRangeKind)42, null),
            CancellationToken.None);
        Assert.True(hint.IsFailure);
        Assert.Equal(RiddleErrorCodes.HintKindInvalid, hint.Error!.Code);
    }

    /// <summary>
    /// Verifies that missing, future, and unpublished riddles are non-disclosing public misses.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task NonPublicRiddlesAreNotDisclosed()
    {
        var workspace = new TestWorkspace(NoonUtcOnTwentieth);
        var missing = await workspace.PublicService.GetPlayAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.True(missing.IsFailure);
        Assert.Equal(RiddleErrorCodes.NotFound, missing.Error!.Code);

        var created = await workspace.Service.CreateAsync(TestWorkspace.CreateRiddleInput(), CancellationToken.None);
        var draft = await workspace.PublicService.GetPlayAsync(created.Value!.Id, CancellationToken.None);
        Assert.True(draft.IsFailure);
        Assert.Equal(RiddleErrorCodes.NotFound, draft.Error!.Code);

        var future = await PublishAsync(workspace, new DateOnly(2026, 8, 21), "бъдеща бяла врана лети");
        var futurePlay = await workspace.PublicService.GetPlayAsync(future.Id, CancellationToken.None);
        Assert.True(futurePlay.IsFailure);
        Assert.Equal(RiddleErrorCodes.NotFound, futurePlay.Error!.Code);
    }

    /// <summary>
    /// Verifies archive paging bounds and anonymous today play without creating account progress.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task ArchivePagingAndAnonymousTodayPlayWork()
    {
        var workspace = new TestWorkspace(NoonUtcOnTwentieth);
        var invalidPage = await workspace.PublicService.ListArchiveAsync(
            new ListPublicRiddlesInput(0, 31),
            CancellationToken.None);
        Assert.True(invalidPage.IsFailure);
        Assert.Equal(RiddleErrorCodes.ArchivePageInvalid, invalidPage.Error!.Code);

        var published = await PublishAsync(workspace, Today);
        var answered = await workspace.PublicService.SubmitAnswerAsync(
            new SubmitRiddleAnswerInput(published.Id, "грешен", null),
            CancellationToken.None);
        Assert.True(answered.IsSuccess);
        Assert.Equal(1, answered.Value!.Progress.AnswerAttemptCount);

        workspace.Account.AccountId = AccountId;
        var progress = await workspace.PublicService.ListProgressAsync(
            new ListAccountRiddleProgressInput(Today, Today),
            CancellationToken.None);
        Assert.True(progress.IsSuccess);
        Assert.Empty(progress.Value!.Items);
    }

    /// <summary>
    /// Verifies that account progress reads require authentication and a bounded date range.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task AccountProgressReadsAreBoundedAndAuthenticated()
    {
        var anonymous = new TestWorkspace(NoonUtcOnTwentieth);
        var unauthenticated = await anonymous.PublicService.ListProgressAsync(
            new ListAccountRiddleProgressInput(Today, Today),
            CancellationToken.None);
        Assert.True(unauthenticated.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, unauthenticated.Error!.Type);

        var workspace = new TestWorkspace(NoonUtcOnTwentieth, AccountId);
        var inverted = await workspace.PublicService.ListProgressAsync(
            new ListAccountRiddleProgressInput(Today.AddDays(1), Today),
            CancellationToken.None);
        Assert.True(inverted.IsFailure);
        Assert.Equal(RiddleErrorCodes.ProgressInvalid, inverted.Error!.Code);

        var published = await PublishAsync(workspace, Today);
        var answered = await workspace.PublicService.SubmitAnswerAsync(
            new SubmitRiddleAnswerInput(published.Id, "бяла врана", null),
            CancellationToken.None);
        Assert.True(answered.IsSuccess);

        var listed = await workspace.PublicService.ListProgressAsync(
            new ListAccountRiddleProgressInput(Today, Today),
            CancellationToken.None);
        Assert.True(listed.IsSuccess);
        Assert.Equal(published.Id, listed.Value!.Items[0].RiddleId);
        Assert.Equal(RiddleProgressStatus.Solved, listed.Value.Items[0].Status);
    }

    private static async Task<RiddleOutput> PublishAsync(
        TestWorkspace workspace,
        DateOnly publicationDate,
        string clue = "бяла врана лети високо")
    {
        var created = await workspace.Service.CreateAsync(
            TestWorkspace.CreateRiddleInput(clue),
            CancellationToken.None);
        Assert.True(created.IsSuccess);
        var published = await workspace.Service.PublishAsync(
            new PublishRiddleInput(created.Value!.Id, publicationDate),
            CancellationToken.None);
        Assert.True(published.IsSuccess);
        return published.Value!;
    }
}

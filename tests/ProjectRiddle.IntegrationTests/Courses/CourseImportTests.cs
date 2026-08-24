using ProjectRiddle.Core.Constants.Courses;
using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Models.Courses.Play;
using ProjectRiddle.Core.Models.Courses.Progress;
using ProjectRiddle.Core.Results.Models;
using ProjectRiddle.IntegrationTests.Harness;

namespace ProjectRiddle.IntegrationTests.Courses;

/// <summary>
/// Verifies account course completion and the bounded, monotonic, atomically validated import.
/// </summary>
public sealed class CourseImportTests
{
    private static readonly DateTimeOffset NoonUtc = new(2026, 8, 20, 9, 0, 0, TimeSpan.Zero);

    private static readonly Guid AccountId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private const string AuthoredAnswer = "бяла врана";

    /// <summary>
    /// Verifies that both account progress routes require an account and never accept one from the caller.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task AccountProgressRoutesRequireAnAccount()
    {
        var workspace = await CourseWorkspace.CreateAsync(NoonUtc);

        var read = await workspace.Service.GetProgressAsync(CancellationToken.None);
        Assert.True(read.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, read.Error!.Type);

        var import = await workspace.Service.ImportProgressAsync(
            new AnonymousCourseProgressInput(CourseLimits.AnonymousProgressSchemaVersion, []),
            CancellationToken.None);
        Assert.True(import.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, import.Error!.Type);
    }

    /// <summary>
    /// Verifies that account progress reports completed exercises and per-lesson completion counts.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task AccountProgressReportsExercisesAndLessons()
    {
        var workspace = await CourseWorkspace.CreateAsync(NoonUtc, AccountId);
        await workspace.SolveExerciseAsync("anagrams", 1);

        var progress = await workspace.Service.GetProgressAsync(CancellationToken.None);

        Assert.True(progress.IsSuccess);
        Assert.Single(progress.Value!.CompletedExerciseIds);
        Assert.Contains(workspace.ExerciseId("anagrams", 1), progress.Value.CompletedExerciseIds);

        var anagrams = progress.Value.Lessons.Single(lesson => lesson.LessonKey == "anagrams");
        Assert.Equal(2, anagrams.ExerciseCount);
        Assert.Equal(1, anagrams.CompletedExerciseCount);
        Assert.False(anagrams.IsComplete);

        await workspace.SolveExerciseAsync("anagrams", 2);
        var finished = await workspace.Service.GetProgressAsync(CancellationToken.None);
        Assert.True(finished.IsSuccess);
        Assert.True(finished.Value!.Lessons.Single(lesson => lesson.LessonKey == "anagrams").IsComplete);
    }

    /// <summary>
    /// Verifies that an import writes account progress for the exercises' underlying clues and that the outcome is
    /// carried rather than flattened, so a fully revealed exercise is not recorded as a solve.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task ImportWritesTheCarriedOutcome()
    {
        var workspace = await CourseWorkspace.CreateAsync(NoonUtc, AccountId);
        var solved = workspace.ExerciseId("anagrams", 1);
        var revealed = workspace.ExerciseId("anagrams", 2);

        var import = await workspace.Service.ImportProgressAsync(
            new AnonymousCourseProgressInput(
                CourseLimits.AnonymousProgressSchemaVersion,
                [
                    new CourseExerciseCompletionInput(solved, RiddleProgressStatus.Solved),
                    new CourseExerciseCompletionInput(revealed, RiddleProgressStatus.FullyRevealed)
                ]),
            CancellationToken.None);

        Assert.True(import.IsSuccess);
        Assert.Equal(2, import.Value!.CompletedExerciseIds.Count);
        Assert.True(import.Value.Lessons.Single(lesson => lesson.LessonKey == "anagrams").IsComplete);

        var solvedState = await workspace.Service.ResumeAsync(
            new ResumeCourseExerciseInput(solved, null),
            CancellationToken.None);
        Assert.True(solvedState.IsSuccess);
        Assert.Equal(RiddleProgressStatus.Solved, solvedState.Value!.Progress.Status);

        var revealedState = await workspace.Service.ResumeAsync(
            new ResumeCourseExerciseInput(revealed, null),
            CancellationToken.None);
        Assert.True(revealedState.IsSuccess);
        Assert.Equal(RiddleProgressStatus.FullyRevealed, revealedState.Value!.Progress.Status);
        Assert.Equal(AuthoredAnswer.Count(char.IsLetter), revealedState.Value.RevealedLetters.Count);
        Assert.NotNull(revealedState.Value.Answer);
    }

    /// <summary>
    /// Verifies that merging is monotonic: an import can advance a record but never demote one.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task ImportMergesMonotonically()
    {
        var workspace = await CourseWorkspace.CreateAsync(NoonUtc, AccountId);
        var exerciseId = workspace.ExerciseId("anagrams", 1);
        await workspace.SolveExerciseAsync("anagrams", 1);

        var demotion = await workspace.Service.ImportProgressAsync(
            new AnonymousCourseProgressInput(
                CourseLimits.AnonymousProgressSchemaVersion,
                [new CourseExerciseCompletionInput(exerciseId, RiddleProgressStatus.FullyRevealed)]),
            CancellationToken.None);

        Assert.True(demotion.IsSuccess);
        var state = await workspace.Service.ResumeAsync(
            new ResumeCourseExerciseInput(exerciseId, null),
            CancellationToken.None);
        Assert.True(state.IsSuccess);
        Assert.Equal(RiddleProgressStatus.Solved, state.Value!.Progress.Status);
    }

    /// <summary>
    /// Verifies that an invalid entry rejects the whole import before anything is written, leaving stored progress
    /// untouched.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task AnInvalidEntryRejectsTheWholeImport()
    {
        var workspace = await CourseWorkspace.CreateAsync(NoonUtc, AccountId);
        var valid = workspace.ExerciseId("anagrams", 1);

        var result = await workspace.Service.ImportProgressAsync(
            new AnonymousCourseProgressInput(
                CourseLimits.AnonymousProgressSchemaVersion,
                [
                    new CourseExerciseCompletionInput(valid, RiddleProgressStatus.Solved),
                    new CourseExerciseCompletionInput(Guid.NewGuid(), RiddleProgressStatus.Solved)
                ]),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(CourseErrorCodes.ProgressReferenceInvalid, result.Error!.Code);

        var progress = await workspace.Service.GetProgressAsync(CancellationToken.None);
        Assert.True(progress.IsSuccess);
        Assert.Empty(progress.Value!.CompletedExerciseIds);
    }

    /// <summary>
    /// Verifies that payload shape and bounds are enforced: the schema version, the entry cap, duplicate
    /// identifiers, and an outcome that is not a completion.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task ImportPayloadShapeAndBoundsAreEnforced()
    {
        var workspace = await CourseWorkspace.CreateAsync(NoonUtc, AccountId);
        var exerciseId = workspace.ExerciseId("anagrams", 1);

        var staleSchema = await workspace.Service.ImportProgressAsync(
            new AnonymousCourseProgressInput(
                CourseLimits.AnonymousProgressSchemaVersion + 1,
                [new CourseExerciseCompletionInput(exerciseId, RiddleProgressStatus.Solved)]),
            CancellationToken.None);
        Assert.True(staleSchema.IsFailure);
        Assert.Equal(CourseErrorCodes.ProgressInvalid, staleSchema.Error!.Code);

        var oversized = Enumerable
            .Range(0, CourseLimits.MaxImportedExerciseCount + 1)
            .Select(_ => new CourseExerciseCompletionInput(Guid.NewGuid(), RiddleProgressStatus.Solved))
            .ToArray();
        var tooLarge = await workspace.Service.ImportProgressAsync(
            new AnonymousCourseProgressInput(CourseLimits.AnonymousProgressSchemaVersion, oversized),
            CancellationToken.None);
        Assert.True(tooLarge.IsFailure);
        Assert.Equal(CourseErrorCodes.ProgressInvalid, tooLarge.Error!.Code);

        var duplicated = await workspace.Service.ImportProgressAsync(
            new AnonymousCourseProgressInput(
                CourseLimits.AnonymousProgressSchemaVersion,
                [
                    new CourseExerciseCompletionInput(exerciseId, RiddleProgressStatus.Solved),
                    new CourseExerciseCompletionInput(exerciseId, RiddleProgressStatus.Solved)
                ]),
            CancellationToken.None);
        Assert.True(duplicated.IsFailure);
        Assert.Equal(CourseErrorCodes.ProgressInvalid, duplicated.Error!.Code);

        var notACompletion = await workspace.Service.ImportProgressAsync(
            new AnonymousCourseProgressInput(
                CourseLimits.AnonymousProgressSchemaVersion,
                [new CourseExerciseCompletionInput(exerciseId, RiddleProgressStatus.InProgress)]),
            CancellationToken.None);
        Assert.True(notACompletion.IsFailure);
        Assert.Equal(CourseErrorCodes.ProgressInvalid, notACompletion.Error!.Code);
    }

    /// <summary>
    /// Verifies that an empty import is accepted and changes nothing, so a client with no stored completion is not
    /// treated as an error.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task AnEmptyImportIsAcceptedAndChangesNothing()
    {
        var workspace = await CourseWorkspace.CreateAsync(NoonUtc, AccountId);
        await workspace.SolveExerciseAsync("anagrams", 1);

        var result = await workspace.Service.ImportProgressAsync(
            new AnonymousCourseProgressInput(CourseLimits.AnonymousProgressSchemaVersion, []),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.CompletedExerciseIds);
    }
}

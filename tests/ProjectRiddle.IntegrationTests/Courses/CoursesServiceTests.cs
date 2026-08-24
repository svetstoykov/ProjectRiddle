using ProjectRiddle.Core.Constants.Courses;
using ProjectRiddle.Core.Constants.Riddles;
using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Models.Courses.Play;
using ProjectRiddle.IntegrationTests.Harness;

namespace ProjectRiddle.IntegrationTests.Courses;

/// <summary>
/// Verifies course play semantics, safe projections, and the account boundary.
/// </summary>
public sealed class CoursesServiceTests
{
    private static readonly DateTimeOffset NoonUtc = new(2026, 8, 20, 9, 0, 0, TimeSpan.Zero);

    private static readonly Guid AccountId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private const string AuthoredAnswer = "бяла врана";

    /// <summary>
    /// Verifies that catalog and lesson reads carry no answer, explanation, or teaching note anywhere in them.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task CatalogAndLessonReadsAreFreeOfAnswerSensitiveContent()
    {
        var workspace = await CourseWorkspace.CreateAsync(NoonUtc, AccountId);

        var catalog = await workspace.Service.GetCatalogAsync(CancellationToken.None);
        Assert.True(catalog.IsSuccess);

        var lesson = await workspace.Service.GetLessonAsync(workspace.LessonId("anagrams"), CancellationToken.None);
        Assert.True(lesson.IsSuccess);
        foreach (var exercise in lesson.Value!.Exercises)
        {
            Assert.DoesNotContain("Обяснение", exercise.Clue, StringComparison.Ordinal);
            Assert.NotEqual(AuthoredAnswer, exercise.Clue);
            Assert.Equal("4,5", exercise.AnswerPattern);
            Assert.NotEmpty(exercise.Ranges);
        }
    }

    /// <summary>
    /// Verifies that a wrong answer withholds the answer, the explanation, and the teaching note, and that a
    /// correct one releases all three at once.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task AnswerSensitiveContentIsReleasedOnlyAtATerminalState()
    {
        var workspace = await CourseWorkspace.CreateAsync(NoonUtc);
        var exerciseId = workspace.ExerciseId("anagrams", 1);

        var wrong = await workspace.Service.SubmitAnswerAsync(
            new SubmitCourseAnswerInput(exerciseId, "нещо друго", null),
            CancellationToken.None);
        Assert.True(wrong.IsSuccess);
        Assert.False(wrong.Value!.IsCorrect);
        Assert.Null(wrong.Value.Answer);
        Assert.Null(wrong.Value.Explanation);
        Assert.Null(wrong.Value.TeachingNote);
        Assert.Equal(RiddleProgressStatus.InProgress, wrong.Value.Progress.Status);

        var correct = await workspace.Service.SubmitAnswerAsync(
            new SubmitCourseAnswerInput(exerciseId, AuthoredAnswer, null),
            CancellationToken.None);
        Assert.True(correct.IsSuccess);
        Assert.True(correct.Value!.IsCorrect);
        Assert.Equal("БЯЛА ВРАНА", correct.Value.Answer);
        Assert.NotNull(correct.Value.Explanation);
        Assert.NotNull(correct.Value.TeachingNote);
        Assert.Equal(RiddleProgressStatus.Solved, correct.Value.Progress.Status);
    }

    /// <summary>
    /// Verifies that a play state is addressed by exercise identifier and never discloses the riddle behind it.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task PlayStatesCarryTheExerciseIdentifier()
    {
        var workspace = await CourseWorkspace.CreateAsync(NoonUtc);
        var exerciseId = workspace.ExerciseId("anagrams", 1);

        var resumed = await workspace.Service.ResumeAsync(
            new ResumeCourseExerciseInput(exerciseId, null),
            CancellationToken.None);

        Assert.True(resumed.IsSuccess);
        Assert.Equal(exerciseId, resumed.Value!.Progress.ExerciseId);
    }

    /// <summary>
    /// Verifies that a structural hint is recorded once and that an unrecognized kind is rejected with the shared
    /// play code rather than a parallel course code.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task HintsAreRecordedOnceAndUnknownKindsAreRejected()
    {
        var workspace = await CourseWorkspace.CreateAsync(NoonUtc, AccountId);
        var exerciseId = workspace.ExerciseId("anagrams", 1);

        var first = await workspace.Service.UseHintAsync(
            new UseCourseHintInput(exerciseId, RiddleRangeKind.Definition, null),
            CancellationToken.None);
        Assert.True(first.IsSuccess);
        Assert.Equal([RiddleRangeKind.Definition], first.Value!.Progress.UsedHints);

        var repeated = await workspace.Service.UseHintAsync(
            new UseCourseHintInput(exerciseId, RiddleRangeKind.Definition, null),
            CancellationToken.None);
        Assert.True(repeated.IsSuccess);
        Assert.Equal([RiddleRangeKind.Definition], repeated.Value!.Progress.UsedHints);

        var unknown = await workspace.Service.UseHintAsync(
            new UseCourseHintInput(exerciseId, (RiddleRangeKind)42, null),
            CancellationToken.None);
        Assert.True(unknown.IsFailure);
        Assert.Equal(RiddleErrorCodes.HintKindInvalid, unknown.Error!.Code);
    }

    /// <summary>
    /// Verifies that reveals never repeat a position and that revealing every letter reaches the fully revealed
    /// state with the answer, the explanation, and the teaching note released.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task RevealsNeverRepeatAndCompleteTheExercise()
    {
        var workspace = await CourseWorkspace.CreateAsync(NoonUtc, AccountId);
        var exerciseId = workspace.ExerciseId("anagrams", 1);
        var letterCount = AuthoredAnswer.Count(char.IsLetter);

        CoursePlayStateOutput? latest = null;
        for (var reveal = 0; reveal < letterCount; reveal++)
        {
            var result = await workspace.Service.RevealLetterAsync(
                new RevealCourseLetterInput(exerciseId, null),
                CancellationToken.None);
            Assert.True(result.IsSuccess);
            latest = result.Value!;
            Assert.Equal(reveal + 1, latest.Progress.LetterRevealCount);
            Assert.Equal(
                latest.Progress.RevealedPositions.Count,
                latest.Progress.RevealedPositions.Distinct().Count());
        }

        Assert.NotNull(latest);
        Assert.Equal(RiddleProgressStatus.FullyRevealed, latest!.Progress.Status);
        Assert.Equal("БЯЛА ВРАНА", latest.Answer);
        Assert.NotNull(latest.TeachingNote);
        Assert.Equal(letterCount, latest.RevealedLetters.Count);
    }

    /// <summary>
    /// Verifies that an anonymous caller's play creates no account progress at all.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task AnonymousPlayCreatesNoAccountProgress()
    {
        var workspace = await CourseWorkspace.CreateAsync(NoonUtc);
        var exerciseId = workspace.ExerciseId("anagrams", 1);

        var solved = await workspace.Service.SubmitAnswerAsync(
            new SubmitCourseAnswerInput(exerciseId, AuthoredAnswer, null),
            CancellationToken.None);
        Assert.True(solved.IsSuccess);

        workspace.Account.AccountId = AccountId;
        var catalog = await workspace.Service.GetCatalogAsync(CancellationToken.None);
        Assert.True(catalog.IsSuccess);
        Assert.All(
            catalog.Value!.Courses.SelectMany(course => course.Lessons),
            lesson => Assert.Equal(0, lesson.Progress!.CompletedExerciseCount));
    }

    /// <summary>
    /// Verifies that an anonymous snapshot rehydrates permitted state, and that a snapshot claiming positions
    /// outside the answer is rejected.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task AnonymousSnapshotsAreValidatedAndRehydrated()
    {
        var workspace = await CourseWorkspace.CreateAsync(NoonUtc);
        var exerciseId = workspace.ExerciseId("anagrams", 1);

        var snapshot = new AnonymousCourseExerciseProgressInput(
            CourseLimits.AnonymousProgressSchemaVersion,
            exerciseId,
            RiddleProgressStatus.InProgress,
            2,
            [RiddleRangeKind.Definition],
            [0, 1]);

        var resumed = await workspace.Service.ResumeAsync(
            new ResumeCourseExerciseInput(exerciseId, snapshot),
            CancellationToken.None);
        Assert.True(resumed.IsSuccess);
        Assert.Equal(2, resumed.Value!.Progress.AnswerAttemptCount);
        Assert.Equal([0, 1], resumed.Value.Progress.RevealedPositions);
        Assert.Equal(2, resumed.Value.RevealedLetters.Count);
        Assert.Null(resumed.Value.Answer);

        var outOfRange = snapshot with { RevealedPositions = [0, 400] };
        var rejected = await workspace.Service.ResumeAsync(
            new ResumeCourseExerciseInput(exerciseId, outOfRange),
            CancellationToken.None);
        Assert.True(rejected.IsFailure);
        Assert.Equal(CourseErrorCodes.ProgressInvalid, rejected.Error!.Code);

        var wrongExercise = snapshot with { ExerciseId = workspace.ExerciseId("anagrams", 2) };
        var mismatched = await workspace.Service.ResumeAsync(
            new ResumeCourseExerciseInput(exerciseId, wrongExercise),
            CancellationToken.None);
        Assert.True(mismatched.IsFailure);
        Assert.Equal(CourseErrorCodes.ProgressReferenceInvalid, mismatched.Error!.Code);
    }

    /// <summary>
    /// Verifies that an authenticated caller's account progress wins over any snapshot in the request body.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task AccountProgressOverridesASuppliedSnapshot()
    {
        var workspace = await CourseWorkspace.CreateAsync(NoonUtc, AccountId);
        var exerciseId = workspace.ExerciseId("anagrams", 1);

        var solved = await workspace.Service.SubmitAnswerAsync(
            new SubmitCourseAnswerInput(exerciseId, AuthoredAnswer, null),
            CancellationToken.None);
        Assert.True(solved.IsSuccess);

        var snapshot = new AnonymousCourseExerciseProgressInput(
            CourseLimits.AnonymousProgressSchemaVersion,
            exerciseId,
            RiddleProgressStatus.InProgress,
            0,
            [],
            []);
        var resumed = await workspace.Service.ResumeAsync(
            new ResumeCourseExerciseInput(exerciseId, snapshot),
            CancellationToken.None);

        Assert.True(resumed.IsSuccess);
        Assert.Equal(RiddleProgressStatus.Solved, resumed.Value!.Progress.Status);
    }

    /// <summary>
    /// Verifies that an unknown exercise is a non-disclosing miss on every play command.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task UnknownExercisesAreNotDisclosed()
    {
        var workspace = await CourseWorkspace.CreateAsync(NoonUtc);
        var missing = Guid.NewGuid();

        var answered = await workspace.Service.SubmitAnswerAsync(
            new SubmitCourseAnswerInput(missing, AuthoredAnswer, null),
            CancellationToken.None);
        Assert.True(answered.IsFailure);
        Assert.Equal(CourseErrorCodes.ExerciseNotFound, answered.Error!.Code);

        var hinted = await workspace.Service.UseHintAsync(
            new UseCourseHintInput(missing, RiddleRangeKind.Definition, null),
            CancellationToken.None);
        Assert.True(hinted.IsFailure);
        Assert.Equal(CourseErrorCodes.ExerciseNotFound, hinted.Error!.Code);

        var revealed = await workspace.Service.RevealLetterAsync(
            new RevealCourseLetterInput(missing, null),
            CancellationToken.None);
        Assert.True(revealed.IsFailure);

        var resumed = await workspace.Service.ResumeAsync(
            new ResumeCourseExerciseInput(missing, null),
            CancellationToken.None);
        Assert.True(resumed.IsFailure);
    }

    /// <summary>
    /// Verifies that an empty submitted answer is rejected with the shared play code.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task EmptyAnswersAreRejected()
    {
        var workspace = await CourseWorkspace.CreateAsync(NoonUtc);

        var result = await workspace.Service.SubmitAnswerAsync(
            new SubmitCourseAnswerInput(workspace.ExerciseId("anagrams", 1), "   ", null),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(RiddleErrorCodes.AnswerRequestInvalid, result.Error!.Code);
    }
}

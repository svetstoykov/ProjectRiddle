using Microsoft.Extensions.Logging;
using ProjectRiddle.Core.Constants.Courses;
using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Interfaces.Accounts;
using ProjectRiddle.Core.Interfaces.Repositories;
using ProjectRiddle.Core.Interfaces.Services;
using ProjectRiddle.Core.Models.Courses;
using ProjectRiddle.Core.Models.Courses.Catalog;
using ProjectRiddle.Core.Models.Courses.Play;
using ProjectRiddle.Core.Models.Courses.Progress;
using ProjectRiddle.Core.Models.Play;
using ProjectRiddle.Core.Models.Riddles;
using ProjectRiddle.Core.Models.Riddles.Discovery;
using ProjectRiddle.Core.Results.Models;
using ProjectRiddle.Core.Validators.Courses;
using ProjectRiddle.Core.Validators.Riddles;

namespace ProjectRiddle.Core.Services.Courses;

/// <summary>
/// Coordinates course discovery, guided practice, and account course completion.
/// </summary>
/// <remarks>
/// Completion is derived from the riddle progress that course play already writes. There is no course progress
/// table, because a separate one would record the same fact about the same row twice and could disagree with
/// itself.
/// </remarks>
public sealed class CoursesService : ICoursesService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IRiddleRepository _riddleRepository;
    private readonly IRiddleProgressRepository _progressRepository;
    private readonly ICluePlayEngine _playEngine;
    private readonly ICurrentAccount _currentAccount;
    private readonly ILogger<CoursesService> _logger;

    /// <summary>
    /// Initializes the courses service.
    /// </summary>
    /// <param name="courseRepository">The curriculum persistence boundary.</param>
    /// <param name="riddleRepository">The riddle persistence boundary.</param>
    /// <param name="progressRepository">The account progress persistence boundary.</param>
    /// <param name="playEngine">The shared clue play engine.</param>
    /// <param name="currentAccount">The current caller identity.</param>
    /// <param name="logger">The logger for safe course events.</param>
    public CoursesService(
        ICourseRepository courseRepository,
        IRiddleRepository riddleRepository,
        IRiddleProgressRepository progressRepository,
        ICluePlayEngine playEngine,
        ICurrentAccount currentAccount,
        ILogger<CoursesService> logger)
    {
        ArgumentNullException.ThrowIfNull(courseRepository);
        ArgumentNullException.ThrowIfNull(riddleRepository);
        ArgumentNullException.ThrowIfNull(progressRepository);
        ArgumentNullException.ThrowIfNull(playEngine);
        ArgumentNullException.ThrowIfNull(currentAccount);
        ArgumentNullException.ThrowIfNull(logger);

        this._courseRepository = courseRepository;
        this._riddleRepository = riddleRepository;
        this._progressRepository = progressRepository;
        this._playEngine = playEngine;
        this._currentAccount = currentAccount;
        this._logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<CourseCatalogOutput>> GetCatalogAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var courses = await _courseRepository.ListActiveCurriculumAsync(cancellationToken);
        var completion = await LoadCompletionAsync(courses, cancellationToken);

        var items = courses
            .OrderBy(course => course.Ordinal)
            .Select(course => ToCourseOutput(course, completion))
            .ToArray();

        return Result.Success(new CourseCatalogOutput(items));
    }

    /// <inheritdoc />
    public async Task<Result<LessonDetailOutput>> GetLessonAsync(Guid lessonId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var lesson = await _courseRepository.GetActiveLessonAsync(lessonId, cancellationToken);
        if (lesson is null)
        {
            return Result.Failure<LessonDetailOutput>(
                new OperationError(
                    "The lesson was not found.",
                    ErrorType.NotFound,
                    CourseErrorCodes.LessonNotFound));
        }

        var exercises = lesson.Exercises.Where(exercise => exercise.IsActive).OrderBy(exercise => exercise.Ordinal).ToArray();
        var riddles = await _riddleRepository.GetByIdsAsync(
            exercises.Select(exercise => exercise.RiddleId).ToArray(),
            cancellationToken);
        var riddlesById = riddles.ToDictionary(riddle => riddle.Id);

        var completedExerciseIds = await LoadCompletedExerciseIdsAsync(exercises, cancellationToken);
        var projected = exercises
            .Where(exercise => riddlesById.ContainsKey(exercise.RiddleId))
            .Select(exercise => ToExerciseOutput(exercise, riddlesById[exercise.RiddleId], completedExerciseIds))
            .ToArray();

        return Result.Success(
            new LessonDetailOutput(
                lesson.Id,
                lesson.Key,
                lesson.Ordinal,
                lesson.Title,
                lesson.Kind,
                lesson.Intro,
                lesson.Prerequisites.Select(prerequisite => prerequisite.LessonKey).ToArray(),
                projected));
    }

    /// <inheritdoc />
    public async Task<Result<CoursePrimerOutput>> GetPrimerAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var pages = await _courseRepository.ListActivePrimerPagesAsync(cancellationToken);
        var items = pages
            .OrderBy(page => page.Ordinal)
            .Select(page => new PrimerPageOutput(page.Ordinal, page.Title, page.Body, page.Figure))
            .ToArray();

        return Result.Success(new CoursePrimerOutput(items));
    }

    /// <inheritdoc />
    public async Task<Result<CoursePlayStateOutput>> SubmitAnswerAsync(
        SubmitCourseAnswerInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();

        var resolved = await ResolveExerciseAsync(input.ExerciseId, input.Progress, cancellationToken);
        if (resolved.IsFailure)
        {
            return Result.Failure<CoursePlayStateOutput>(resolved.Error!);
        }

        var context = resolved.Value!;
        var outcome = await _playEngine.SubmitAnswerAsync(
            context.Riddle,
            input.Answer,
            context.Anonymous,
            cancellationToken);
        if (outcome.IsFailure)
        {
            return Result.Failure<CoursePlayStateOutput>(outcome.Error!);
        }

        _logger.LogInformation(
            "Checked a course exercise answer. ExerciseId: {ExerciseId} Correct: {IsCorrect}",
            context.Exercise.Id,
            outcome.Value!.IsCorrect);
        return Result.Success(ToPlayState(context.Exercise, outcome.Value));
    }

    /// <inheritdoc />
    public async Task<Result<CoursePlayStateOutput>> UseHintAsync(
        UseCourseHintInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();

        var resolved = await ResolveExerciseAsync(input.ExerciseId, input.Progress, cancellationToken);
        if (resolved.IsFailure)
        {
            return Result.Failure<CoursePlayStateOutput>(resolved.Error!);
        }

        var context = resolved.Value!;
        var outcome = await _playEngine.UseHintAsync(
            context.Riddle,
            input.Kind,
            context.Anonymous,
            cancellationToken);
        if (outcome.IsFailure)
        {
            return Result.Failure<CoursePlayStateOutput>(outcome.Error!);
        }

        _logger.LogInformation("Recorded a course exercise hint. ExerciseId: {ExerciseId}", context.Exercise.Id);
        return Result.Success(ToPlayState(context.Exercise, outcome.Value!));
    }

    /// <inheritdoc />
    public async Task<Result<CoursePlayStateOutput>> RevealLetterAsync(
        RevealCourseLetterInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();

        var resolved = await ResolveExerciseAsync(input.ExerciseId, input.Progress, cancellationToken);
        if (resolved.IsFailure)
        {
            return Result.Failure<CoursePlayStateOutput>(resolved.Error!);
        }

        var context = resolved.Value!;
        var outcome = await _playEngine.RevealLetterAsync(context.Riddle, context.Anonymous, cancellationToken);
        if (outcome.IsFailure)
        {
            return Result.Failure<CoursePlayStateOutput>(outcome.Error!);
        }

        _logger.LogInformation("Revealed a course exercise letter. ExerciseId: {ExerciseId}", context.Exercise.Id);
        return Result.Success(ToPlayState(context.Exercise, outcome.Value!));
    }

    /// <inheritdoc />
    public async Task<Result<CoursePlayStateOutput>> ResumeAsync(
        ResumeCourseExerciseInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();

        var resolved = await ResolveExerciseAsync(input.ExerciseId, input.Progress, cancellationToken);
        if (resolved.IsFailure)
        {
            return Result.Failure<CoursePlayStateOutput>(resolved.Error!);
        }

        var context = resolved.Value!;
        var outcome = await _playEngine.ResumeAsync(context.Riddle, context.Anonymous, cancellationToken);
        if (outcome.IsFailure)
        {
            return Result.Failure<CoursePlayStateOutput>(outcome.Error!);
        }

        return Result.Success(ToPlayState(context.Exercise, outcome.Value!));
    }

    /// <inheritdoc />
    public async Task<Result<AccountCourseProgressOutput>> GetProgressAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_currentAccount.AccountId is null)
        {
            return Result.Failure<AccountCourseProgressOutput>(AuthenticationRequired());
        }

        var courses = await _courseRepository.ListActiveCurriculumAsync(cancellationToken);
        return Result.Success(await ProjectAccountProgressAsync(courses, cancellationToken));
    }

    /// <inheritdoc />
    public async Task<Result<AccountCourseProgressOutput>> ImportProgressAsync(
        AnonymousCourseProgressInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();

        var accountId = _currentAccount.AccountId;
        if (accountId is null)
        {
            return Result.Failure<AccountCourseProgressOutput>(AuthenticationRequired());
        }

        var validation = AnonymousCourseProgressValidator.ValidateImport(input);
        if (validation.IsFailure)
        {
            return Result.Failure<AccountCourseProgressOutput>(validation.Error!);
        }

        var exerciseIds = input.Entries.Select(entry => entry.ExerciseId).ToArray();
        var exercises = await _courseRepository.ListActiveExercisesByIdsAsync(exerciseIds, cancellationToken);
        if (exercises.Count != exerciseIds.Length)
        {
            return Result.Failure<AccountCourseProgressOutput>(ReferenceInvalid());
        }

        var exercisesById = exercises.ToDictionary(exercise => exercise.Id);
        var riddles = await _riddleRepository.GetByIdsAsync(
            exercises.Select(exercise => exercise.RiddleId).Distinct().ToArray(),
            cancellationToken);
        var riddlesById = riddles.Where(riddle => riddle.IsLesson).ToDictionary(riddle => riddle.Id);
        if (exercises.Any(exercise => !riddlesById.ContainsKey(exercise.RiddleId)))
        {
            return Result.Failure<AccountCourseProgressOutput>(ReferenceInvalid());
        }

        foreach (var entry in input.Entries)
        {
            var riddle = riddlesById[exercisesById[entry.ExerciseId].RiddleId];
            var letters = AnswerLetters.FromNormalizedAnswer(AnswerNormalizer.Normalize(riddle.Answer));
            var imported = new CluePlayState(
                entry.Status,
                AnswerAttemptCount: 0,
                UsedHints: [],
                RevealedPositions: entry.Status is RiddleProgressStatus.FullyRevealed
                    ? Enumerable.Range(0, letters.Count).ToArray()
                    : []);

            var merged = await _playEngine.MergeAccountProgressAsync(
                riddle,
                accountId!.Value,
                imported,
                cancellationToken);
            if (merged.IsFailure)
            {
                return Result.Failure<AccountCourseProgressOutput>(merged.Error!);
            }
        }

        _logger.LogInformation(
            "Imported anonymous course completion. ExerciseCount: {ExerciseCount}",
            input.Entries.Count);

        var courses = await _courseRepository.ListActiveCurriculumAsync(cancellationToken);
        return Result.Success(await ProjectAccountProgressAsync(courses, cancellationToken));
    }

    /// <summary>
    /// Projects the current account's completion across the active curriculum.
    /// </summary>
    /// <param name="courses">The active curriculum.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The account's completion.</returns>
    private async Task<AccountCourseProgressOutput> ProjectAccountProgressAsync(
        IReadOnlyList<Course> courses,
        CancellationToken cancellationToken)
    {
        var exercises = courses
            .SelectMany(course => course.Lessons)
            .SelectMany(lesson => lesson.Exercises)
            .Where(exercise => exercise.IsActive)
            .ToArray();
        var completedExerciseIds = await LoadCompletedExerciseIdsAsync(exercises, cancellationToken);

        var lessons = courses
            .OrderBy(course => course.Ordinal)
            .SelectMany(course => course.Lessons.Where(lesson => lesson.IsActive).OrderBy(lesson => lesson.Ordinal))
            .Select(lesson => ToLessonCompletion(lesson, completedExerciseIds))
            .ToArray();

        return new AccountCourseProgressOutput(completedExerciseIds.OrderBy(id => id).ToArray(), lessons);
    }

    private static LessonCompletionOutput ToLessonCompletion(Lesson lesson, IReadOnlySet<Guid> completedExerciseIds)
    {
        var active = lesson.Exercises.Where(exercise => exercise.IsActive).ToArray();
        var completed = active.Count(exercise => completedExerciseIds.Contains(exercise.Id));

        return new LessonCompletionOutput(
            lesson.Id,
            lesson.Key,
            completed,
            active.Length,
            active.Length > 0 && completed == active.Length);
    }

    private static OperationError AuthenticationRequired()
    {
        return new OperationError("An authenticated account is required.", ErrorType.Unauthorized);
    }

    private static OperationError ReferenceInvalid()
    {
        return new OperationError(
            "The imported course progress refers to content that is not active lesson content.",
            ErrorType.Validation,
            CourseErrorCodes.ProgressReferenceInvalid);
    }

    /// <summary>
    /// Resolves an exercise to the clue behind it and validates any anonymous snapshot the caller supplied.
    /// </summary>
    /// <param name="exerciseId">The exercise identifier.</param>
    /// <param name="progress">The claimed snapshot, or <see langword="null" />.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The play context, or an expected failure.</returns>
    /// <remarks>
    /// An exercise whose riddle is missing is reported as a missing exercise rather than surfacing as an internal
    /// error. A caller cannot tell the two apart and does not need to.
    /// </remarks>
    private async Task<Result<CoursePlayContext>> ResolveExerciseAsync(
        Guid exerciseId,
        AnonymousCourseExerciseProgressInput? progress,
        CancellationToken cancellationToken)
    {
        var exercise = await _courseRepository.GetActiveExerciseAsync(exerciseId, cancellationToken);
        if (exercise is null)
        {
            return Result.Failure<CoursePlayContext>(ExerciseNotFound());
        }

        var riddle = await _riddleRepository.GetByIdAsync(exercise.RiddleId, cancellationToken);
        if (riddle is null || !riddle.IsLesson)
        {
            return Result.Failure<CoursePlayContext>(ExerciseNotFound());
        }

        if (_currentAccount.AccountId is not null || progress is null)
        {
            return Result.Success(new CoursePlayContext(exercise, riddle, null));
        }

        var letters = AnswerLetters.FromNormalizedAnswer(AnswerNormalizer.Normalize(riddle.Answer));
        var validation = AnonymousCourseProgressValidator.ValidateExerciseSnapshot(
            progress,
            exerciseId,
            letters.Count);
        if (validation.IsFailure)
        {
            return Result.Failure<CoursePlayContext>(validation.Error!);
        }

        var state = new CluePlayState(
            progress.Status,
            progress.AnswerAttemptCount,
            progress.UsedHints,
            progress.RevealedPositions);
        return Result.Success(new CoursePlayContext(exercise, riddle, state));
    }

    /// <summary>
    /// Shapes a play outcome for the course contract, adding the teaching note under the terminal-state rule.
    /// </summary>
    /// <param name="exercise">The exercise being played.</param>
    /// <param name="outcome">The engine's outcome.</param>
    /// <returns>The course play state.</returns>
    private static CoursePlayStateOutput ToPlayState(LessonExercise exercise, CluePlayOutcome outcome)
    {
        var isTerminal = IsComplete(outcome.State.Status);

        return new CoursePlayStateOutput(
            new CourseProgressSnapshotOutput(
                exercise.Id,
                outcome.State.Status,
                outcome.State.AnswerAttemptCount,
                outcome.State.UsedHints,
                outcome.State.RevealedPositions,
                outcome.State.RevealedPositions.Count),
            outcome.RevealedLetters,
            outcome.Answer,
            outcome.Explanation,
            isTerminal ? exercise.TeachingNote : null,
            outcome.IsCorrect);
    }

    private static OperationError ExerciseNotFound()
    {
        return new OperationError(
            "The lesson exercise was not found.",
            ErrorType.NotFound,
            CourseErrorCodes.ExerciseNotFound);
    }

    /// <summary>
    /// Holds a resolved exercise, its clue, and the validated anonymous state for one play command.
    /// </summary>
    /// <param name="Exercise">The exercise being played.</param>
    /// <param name="Riddle">The riddle holding the clue.</param>
    /// <param name="Anonymous">The validated anonymous state, or <see langword="null" />.</param>
    private sealed record CoursePlayContext(LessonExercise Exercise, Riddle Riddle, CluePlayState? Anonymous);

    /// <summary>
    /// Loads the signed-in caller's completion across the whole curriculum, or an empty projection when anonymous.
    /// </summary>
    /// <param name="courses">The active curriculum.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The completion facts, or <see langword="null" /> when the caller is anonymous.</returns>
    private async Task<CurriculumCompletion?> LoadCompletionAsync(
        IReadOnlyList<Course> courses,
        CancellationToken cancellationToken)
    {
        var accountId = _currentAccount.AccountId;
        if (accountId is null)
        {
            return null;
        }

        var exercises = courses
            .SelectMany(course => course.Lessons)
            .SelectMany(lesson => lesson.Exercises)
            .Where(exercise => exercise.IsActive)
            .ToArray();
        var completedExerciseIds = await LoadCompletedExerciseIdsAsync(exercises, cancellationToken);

        var completedLessonKeys = courses
            .SelectMany(course => course.Lessons)
            .Where(lesson => IsLessonComplete(lesson, completedExerciseIds))
            .Select(lesson => lesson.Key)
            .ToHashSet(StringComparer.Ordinal);

        return new CurriculumCompletion(completedExerciseIds, completedLessonKeys);
    }

    /// <summary>
    /// Maps the account's complete progress rows back onto the exercises that reference them.
    /// </summary>
    /// <param name="exercises">The exercises to resolve.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The complete exercise identifiers, empty when the caller is anonymous.</returns>
    private async Task<IReadOnlySet<Guid>> LoadCompletedExerciseIdsAsync(
        LessonExercise[] exercises,
        CancellationToken cancellationToken)
    {
        var accountId = _currentAccount.AccountId;
        if (accountId is null || exercises.Length == 0)
        {
            return new HashSet<Guid>();
        }

        var riddleIds = exercises.Select(exercise => exercise.RiddleId).Distinct().ToArray();
        var records = await _progressRepository.ListByAccountAndRiddleIdsAsync(
            accountId!.Value,
            riddleIds,
            cancellationToken);
        var completedRiddleIds = records
            .Where(record => IsComplete(record.Status))
            .Select(record => record.RiddleId)
            .ToHashSet();

        return exercises
            .Where(exercise => completedRiddleIds.Contains(exercise.RiddleId))
            .Select(exercise => exercise.Id)
            .ToHashSet();
    }

    /// <summary>
    /// Reports whether every active exercise in the lesson is complete.
    /// </summary>
    /// <param name="lesson">The lesson to inspect.</param>
    /// <param name="completedExerciseIds">The caller's complete exercise identifiers.</param>
    /// <returns><see langword="true" /> when the lesson counts as complete.</returns>
    /// <remarks>A lesson with no active exercises is never complete, so it can never satisfy a prerequisite.</remarks>
    private static bool IsLessonComplete(Lesson lesson, IReadOnlySet<Guid> completedExerciseIds)
    {
        var active = lesson.Exercises.Where(exercise => exercise.IsActive).ToArray();
        return active.Length > 0 && active.All(exercise => completedExerciseIds.Contains(exercise.Id));
    }

    /// <summary>
    /// Reports whether a status counts as completion. A full reveal counts: the courses teach, they do not grade.
    /// </summary>
    /// <param name="status">The progress status.</param>
    /// <returns><see langword="true" /> when the exercise counts as complete.</returns>
    private static bool IsComplete(RiddleProgressStatus status)
    {
        return status is RiddleProgressStatus.Solved or RiddleProgressStatus.FullyRevealed;
    }

    private static CourseOutput ToCourseOutput(Course course, CurriculumCompletion? completion)
    {
        var lessons = course.Lessons
            .Where(lesson => lesson.IsActive)
            .OrderBy(lesson => lesson.Ordinal)
            .Select(lesson => ToLessonOutput(lesson, completion))
            .ToArray();

        return new CourseOutput(course.Id, course.Key, course.Ordinal, course.Title, course.Intro, lessons);
    }

    private static LessonOutput ToLessonOutput(Lesson lesson, CurriculumCompletion? completion)
    {
        var prerequisiteKeys = lesson.Prerequisites.Select(prerequisite => prerequisite.LessonKey).ToArray();
        var activeExercises = lesson.Exercises.Where(exercise => exercise.IsActive).ToArray();

        LessonProgressOutput? progress = null;
        if (completion is not null)
        {
            var completedIds = activeExercises
                .Where(exercise => completion.CompletedExerciseIds.Contains(exercise.Id))
                .Select(exercise => exercise.Id)
                .ToArray();
            var isAvailable = prerequisiteKeys.All(completion.CompletedLessonKeys.Contains);
            progress = new LessonProgressOutput(completedIds.Length, isAvailable, completedIds);
        }

        return new LessonOutput(
            lesson.Id,
            lesson.Key,
            lesson.Ordinal,
            lesson.Title,
            lesson.Kind,
            activeExercises.Length,
            prerequisiteKeys,
            progress);
    }

    private LessonExerciseOutput ToExerciseOutput(
        LessonExercise exercise,
        Riddle riddle,
        IReadOnlySet<Guid> completedExerciseIds)
    {
        var ranges = riddle.Ranges
            .Select(range => new PublicRiddleRangeOutput(range.Kind, range.Start, range.End))
            .ToArray();
        bool? isComplete = _currentAccount.AccountId is null
            ? null
            : completedExerciseIds.Contains(exercise.Id);

        return new LessonExerciseOutput(
            exercise.Id,
            exercise.Ordinal,
            exercise.Setup,
            riddle.Clue,
            riddle.AnswerPattern,
            ranges,
            isComplete);
    }

    /// <summary>
    /// Holds the completion facts a catalog projection needs, computed once per read.
    /// </summary>
    /// <param name="CompletedExerciseIds">The caller's complete exercise identifiers.</param>
    /// <param name="CompletedLessonKeys">The lesson keys whose every active exercise is complete.</param>
    private sealed record CurriculumCompletion(
        IReadOnlySet<Guid> CompletedExerciseIds,
        IReadOnlySet<string> CompletedLessonKeys);
}

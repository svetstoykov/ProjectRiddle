using Microsoft.Extensions.Logging;
using ProjectRiddle.Core.Constants.Courses;
using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Interfaces.Accounts;
using ProjectRiddle.Core.Interfaces.Repositories;
using ProjectRiddle.Core.Interfaces.Services;
using ProjectRiddle.Core.Models.Courses;
using ProjectRiddle.Core.Models.Courses.Catalog;
using ProjectRiddle.Core.Models.Riddles;
using ProjectRiddle.Core.Models.Riddles.Discovery;
using ProjectRiddle.Core.Results.Models;

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
    private readonly ICurrentAccount _currentAccount;
    private readonly ILogger<CoursesService> _logger;

    /// <summary>
    /// Initializes the courses service.
    /// </summary>
    /// <param name="courseRepository">The curriculum persistence boundary.</param>
    /// <param name="riddleRepository">The riddle persistence boundary.</param>
    /// <param name="progressRepository">The account progress persistence boundary.</param>
    /// <param name="currentAccount">The current caller identity.</param>
    /// <param name="logger">The logger for safe course events.</param>
    public CoursesService(
        ICourseRepository courseRepository,
        IRiddleRepository riddleRepository,
        IRiddleProgressRepository progressRepository,
        ICurrentAccount currentAccount,
        ILogger<CoursesService> logger)
    {
        ArgumentNullException.ThrowIfNull(courseRepository);
        ArgumentNullException.ThrowIfNull(riddleRepository);
        ArgumentNullException.ThrowIfNull(progressRepository);
        ArgumentNullException.ThrowIfNull(currentAccount);
        ArgumentNullException.ThrowIfNull(logger);

        this._courseRepository = courseRepository;
        this._riddleRepository = riddleRepository;
        this._progressRepository = progressRepository;
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

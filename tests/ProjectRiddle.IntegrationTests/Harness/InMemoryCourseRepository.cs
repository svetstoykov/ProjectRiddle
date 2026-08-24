using ProjectRiddle.Core.Interfaces.Repositories;
using ProjectRiddle.Core.Models.Courses;

namespace ProjectRiddle.IntegrationTests.Harness;

/// <summary>
/// Stores the curriculum in memory so Core course tests do not depend on Infrastructure.
/// </summary>
public sealed class InMemoryCourseRepository : ICourseRepository
{
    private readonly List<Course> _courses = [];
    private readonly List<PrimerPage> _primerPages = [];

    /// <inheritdoc />
    public Task<IReadOnlyList<Course>> ListActiveCurriculumAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var active = _courses
            .Where(course => course.IsActive)
            .OrderBy(course => course.Ordinal)
            .ToArray();
        return Task.FromResult<IReadOnlyList<Course>>(active);
    }

    /// <inheritdoc />
    public Task<Lesson?> GetActiveLessonAsync(Guid lessonId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var lesson = _courses
            .Where(course => course.IsActive)
            .SelectMany(course => course.Lessons)
            .SingleOrDefault(candidate => candidate.Id == lessonId && candidate.IsActive);
        return Task.FromResult(lesson);
    }

    /// <inheritdoc />
    public Task<LessonExercise?> GetActiveExerciseAsync(Guid exerciseId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(ActiveExercises().SingleOrDefault(exercise => exercise.Id == exerciseId));
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<LessonExercise>> ListActiveExercisesByIdsAsync(
        IReadOnlyCollection<Guid> exerciseIds,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(exerciseIds);
        cancellationToken.ThrowIfCancellationRequested();
        var matches = ActiveExercises()
            .Where(exercise => exerciseIds.Contains(exercise.Id))
            .ToArray();
        return Task.FromResult<IReadOnlyList<LessonExercise>>(matches);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<PrimerPage>> ListActivePrimerPagesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var active = _primerPages.Where(page => page.IsActive).OrderBy(page => page.Ordinal).ToArray();
        return Task.FromResult<IReadOnlyList<PrimerPage>>(active);
    }

    /// <inheritdoc />
    public Task SeedCurriculumAsync(CourseCurriculum curriculum, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(curriculum);
        cancellationToken.ThrowIfCancellationRequested();

        _courses.Clear();
        _courses.AddRange(curriculum.Courses);
        _primerPages.Clear();
        _primerPages.AddRange(curriculum.PrimerPages);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Lists every active exercise across the curriculum.
    /// </summary>
    /// <returns>The active exercises.</returns>
    public IEnumerable<LessonExercise> ActiveExercises()
    {
        return _courses
            .Where(course => course.IsActive)
            .SelectMany(course => course.Lessons)
            .Where(lesson => lesson.IsActive)
            .SelectMany(lesson => lesson.Exercises)
            .Where(exercise => exercise.IsActive);
    }

    /// <summary>
    /// Gets the active lesson with the supplied key.
    /// </summary>
    /// <param name="key">The lesson key. Cannot be <see langword="null" /> or whitespace.</param>
    /// <returns>The lesson.</returns>
    public Lesson LessonByKey(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return _courses.SelectMany(course => course.Lessons).Single(lesson => lesson.Key == key);
    }
}

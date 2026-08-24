using ProjectRiddle.Core.Models.Courses;

namespace ProjectRiddle.Core.Interfaces.Repositories;

/// <summary>
/// Persists the course curriculum without exposing storage types to Core.
/// </summary>
public interface ICourseRepository
{
    /// <summary>
    /// Lists the active curriculum: active courses in ordinal order, each with its active lessons in ordinal
    /// order, each with its prerequisites and its active exercises in ordinal order.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The active curriculum.</returns>
    Task<IReadOnlyList<Course>> ListActiveCurriculumAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets an active lesson with its prerequisites and active exercises.
    /// </summary>
    /// <param name="lessonId">The lesson identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The lesson when it exists and is active; otherwise <see langword="null" />.</returns>
    Task<Lesson?> GetActiveLessonAsync(Guid lessonId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets an active exercise by identifier.
    /// </summary>
    /// <param name="exerciseId">The exercise identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The exercise when it exists and is active; otherwise <see langword="null" />.</returns>
    Task<LessonExercise?> GetActiveExerciseAsync(Guid exerciseId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists the active exercises with the supplied identifiers.
    /// </summary>
    /// <param name="exerciseIds">The exercise identifiers. Cannot be <see langword="null" />.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The matching active exercises; unknown or deactivated identifiers are simply absent.</returns>
    Task<IReadOnlyList<LessonExercise>> ListActiveExercisesByIdsAsync(
        IReadOnlyCollection<Guid> exerciseIds,
        CancellationToken cancellationToken);

    /// <summary>
    /// Lists the active primer pages in ordinal order.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The active primer pages.</returns>
    Task<IReadOnlyList<PrimerPage>> ListActivePrimerPagesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Upserts a validated curriculum by stable identifier and deactivates whatever the curriculum no longer names.
    /// </summary>
    /// <param name="curriculum">The validated curriculum. Cannot be <see langword="null" />.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that represents the seed operation.</returns>
    /// <remarks>
    /// The lesson riddles travel with the curriculum so courses, lessons, exercises, and clues are written in one
    /// transaction. Nothing is deleted and no progress record is ever written, updated, or removed here.
    /// </remarks>
    Task SeedCurriculumAsync(CourseCurriculum curriculum, CancellationToken cancellationToken);
}

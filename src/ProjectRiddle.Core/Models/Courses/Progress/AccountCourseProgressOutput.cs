namespace ProjectRiddle.Core.Models.Courses.Progress;

/// <summary>
/// Represents the current account's course completion.
/// </summary>
/// <param name="CompletedExerciseIds">The complete exercise identifiers. Cannot be <see langword="null" />.</param>
/// <param name="Lessons">Per-lesson completion for the active curriculum. Cannot be <see langword="null" />.</param>
public sealed record AccountCourseProgressOutput(
    IReadOnlyList<Guid> CompletedExerciseIds,
    IReadOnlyList<LessonCompletionOutput> Lessons);

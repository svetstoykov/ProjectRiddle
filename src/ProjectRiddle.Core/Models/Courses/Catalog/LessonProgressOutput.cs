namespace ProjectRiddle.Core.Models.Courses.Catalog;

/// <summary>
/// Represents a signed-in caller's completion of one lesson and whether it is available.
/// </summary>
/// <param name="CompletedExerciseCount">The number of complete exercises in the lesson.</param>
/// <param name="IsAvailable">A value indicating whether every authored prerequisite is complete.</param>
/// <param name="CompletedExerciseIds">The complete exercise identifiers. Cannot be <see langword="null" />.</param>
public sealed record LessonProgressOutput(
    int CompletedExerciseCount,
    bool IsAvailable,
    IReadOnlyList<Guid> CompletedExerciseIds);

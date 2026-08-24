using ProjectRiddle.Core.Models.Courses.Catalog;

namespace ProjectRiddle.Api.Models.Courses.Catalog;

/// <summary>
/// Represents signed-in completion and availability for one lesson.
/// </summary>
public sealed record LessonProgressResponse
{
    /// <summary>
    /// Gets the number of complete exercises in the lesson.
    /// </summary>
    public required int CompletedExerciseCount { get; init; }

    /// <summary>
    /// Gets a value indicating whether every authored prerequisite is complete.
    /// </summary>
    public required bool IsAvailable { get; init; }

    /// <summary>
    /// Gets the complete exercise identifiers.
    /// </summary>
    public required IReadOnlyList<Guid> CompletedExerciseIds { get; init; }

    /// <summary>
    /// Maps a Core lesson progress output to the API response.
    /// </summary>
    /// <param name="output">The Core output. Cannot be <see langword="null" />.</param>
    /// <returns>The corresponding API response.</returns>
    public static LessonProgressResponse FromCoreLessonProgressOutput(LessonProgressOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        return new LessonProgressResponse
        {
            CompletedExerciseCount = output.CompletedExerciseCount,
            IsAvailable = output.IsAvailable,
            CompletedExerciseIds = output.CompletedExerciseIds
        };
    }
}

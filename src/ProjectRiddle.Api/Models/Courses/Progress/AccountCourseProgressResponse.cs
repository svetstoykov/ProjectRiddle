using ProjectRiddle.Core.Models.Courses.Progress;

namespace ProjectRiddle.Api.Models.Courses.Progress;

/// <summary>
/// Represents the current account's guided-course completion.
/// </summary>
public sealed record AccountCourseProgressResponse
{
    /// <summary>
    /// Gets the complete exercise identifiers.
    /// </summary>
    public required IReadOnlyList<Guid> CompletedExerciseIds { get; init; }

    /// <summary>
    /// Gets per-lesson completion for the active curriculum.
    /// </summary>
    public required IReadOnlyList<LessonCompletionResponse> Lessons { get; init; }

    /// <summary>
    /// Maps a Core account progress output to the API response.
    /// </summary>
    /// <param name="output">The Core output. Cannot be <see langword="null" />.</param>
    /// <returns>The corresponding API response.</returns>
    public static AccountCourseProgressResponse FromCoreAccountCourseProgressOutput(
        AccountCourseProgressOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        return new AccountCourseProgressResponse
        {
            CompletedExerciseIds = output.CompletedExerciseIds,
            Lessons = output.Lessons.Select(LessonCompletionResponse.FromCoreLessonCompletionOutput).ToArray()
        };
    }
}

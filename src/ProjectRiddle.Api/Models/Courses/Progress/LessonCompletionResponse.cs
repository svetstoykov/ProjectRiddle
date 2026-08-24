using ProjectRiddle.Core.Models.Courses.Progress;

namespace ProjectRiddle.Api.Models.Courses.Progress;

/// <summary>
/// Represents one lesson's completion for the current account.
/// </summary>
public sealed record LessonCompletionResponse
{
    /// <summary>
    /// Gets the stable lesson identifier.
    /// </summary>
    public required Guid LessonId { get; init; }

    /// <summary>
    /// Gets the lesson key.
    /// </summary>
    public required string LessonKey { get; init; }

    /// <summary>
    /// Gets the number of complete exercises.
    /// </summary>
    public required int CompletedExerciseCount { get; init; }

    /// <summary>
    /// Gets the number of active exercises.
    /// </summary>
    public required int ExerciseCount { get; init; }

    /// <summary>
    /// Gets a value indicating whether every active exercise is complete.
    /// </summary>
    public required bool IsComplete { get; init; }

    /// <summary>
    /// Maps a Core lesson completion output to the API response.
    /// </summary>
    /// <param name="output">The Core output. Cannot be <see langword="null" />.</param>
    /// <returns>The corresponding API response.</returns>
    public static LessonCompletionResponse FromCoreLessonCompletionOutput(LessonCompletionOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        return new LessonCompletionResponse
        {
            LessonId = output.LessonId,
            LessonKey = output.LessonKey,
            CompletedExerciseCount = output.CompletedExerciseCount,
            ExerciseCount = output.ExerciseCount,
            IsComplete = output.IsComplete
        };
    }
}

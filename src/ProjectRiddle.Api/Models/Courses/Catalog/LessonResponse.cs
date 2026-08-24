using System.Text.Json.Serialization;
using ProjectRiddle.Core.Enums.Courses;
using ProjectRiddle.Core.Models.Courses.Catalog;

namespace ProjectRiddle.Api.Models.Courses.Catalog;

/// <summary>
/// Represents one guided-course lesson in the catalog.
/// </summary>
public sealed record LessonResponse
{
    /// <summary>
    /// Gets the stable lesson identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the lesson key.
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Gets the one-based position within its course.
    /// </summary>
    public required int Ordinal { get; init; }

    /// <summary>
    /// Gets the lesson title.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets the role the lesson plays.
    /// </summary>
    public required LessonKind Kind { get; init; }

    /// <summary>
    /// Gets the number of active exercises beneath the lesson.
    /// </summary>
    public required int ExerciseCount { get; init; }

    /// <summary>
    /// Gets the lesson keys that must be complete first.
    /// </summary>
    public required IReadOnlyList<string> PrerequisiteLessonKeys { get; init; }

    /// <summary>
    /// Gets signed-in completion and availability when present.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public LessonProgressResponse? Progress { get; init; }

    /// <summary>
    /// Maps a Core lesson to the API response.
    /// </summary>
    /// <param name="output">The Core output. Cannot be <see langword="null" />.</param>
    /// <returns>The corresponding API response.</returns>
    public static LessonResponse FromCoreLessonOutput(LessonOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        return new LessonResponse
        {
            Id = output.Id,
            Key = output.Key,
            Ordinal = output.Ordinal,
            Title = output.Title,
            Kind = output.Kind,
            ExerciseCount = output.ExerciseCount,
            PrerequisiteLessonKeys = output.PrerequisiteLessonKeys,
            Progress = output.Progress is null
                ? null
                : LessonProgressResponse.FromCoreLessonProgressOutput(output.Progress)
        };
    }
}

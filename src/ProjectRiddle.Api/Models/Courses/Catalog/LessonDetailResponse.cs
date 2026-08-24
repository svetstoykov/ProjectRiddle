using System.Text.Json.Serialization;
using ProjectRiddle.Core.Enums.Courses;
using ProjectRiddle.Core.Models.Courses.Catalog;

namespace ProjectRiddle.Api.Models.Courses.Catalog;

/// <summary>
/// Represents a lesson's teaching prose and ordered safe exercise projections.
/// </summary>
public sealed record LessonDetailResponse
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
    /// Gets the optional technique prose.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Intro { get; init; }

    /// <summary>
    /// Gets the lesson keys that must be complete first.
    /// </summary>
    public required IReadOnlyList<string> PrerequisiteLessonKeys { get; init; }

    /// <summary>
    /// Gets the active exercises in ordinal order.
    /// </summary>
    public required IReadOnlyList<LessonExerciseResponse> Exercises { get; init; }

    /// <summary>
    /// Maps a Core lesson detail output to the API response.
    /// </summary>
    /// <param name="output">The Core output. Cannot be <see langword="null" />.</param>
    /// <returns>The corresponding API response.</returns>
    public static LessonDetailResponse FromCoreLessonDetailOutput(LessonDetailOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        return new LessonDetailResponse
        {
            Id = output.Id,
            Key = output.Key,
            Ordinal = output.Ordinal,
            Title = output.Title,
            Kind = output.Kind,
            Intro = output.Intro,
            PrerequisiteLessonKeys = output.PrerequisiteLessonKeys,
            Exercises = output.Exercises.Select(LessonExerciseResponse.FromCoreLessonExerciseOutput).ToArray()
        };
    }
}

using System.Text.Json.Serialization;
using ProjectRiddle.Api.Models.Riddles.Discovery;
using ProjectRiddle.Core.Models.Courses.Catalog;

namespace ProjectRiddle.Api.Models.Courses.Catalog;

/// <summary>
/// Represents the safe projection of one lesson exercise.
/// </summary>
public sealed record LessonExerciseResponse
{
    /// <summary>
    /// Gets the stable exercise identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the one-based position within the lesson.
    /// </summary>
    public required int Ordinal { get; init; }

    /// <summary>
    /// Gets the optional one-line nudge shown before solving.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Setup { get; init; }

    /// <summary>
    /// Gets the full clue text.
    /// </summary>
    public required string Clue { get; init; }

    /// <summary>
    /// Gets the public answer pattern.
    /// </summary>
    public required string AnswerPattern { get; init; }

    /// <summary>
    /// Gets the safe structural ranges.
    /// </summary>
    public required IReadOnlyList<PublicRiddleRangeResponse> Ranges { get; init; }

    /// <summary>
    /// Gets whether the signed-in caller has completed the exercise, when known.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsComplete { get; init; }

    /// <summary>
    /// Maps a Core lesson exercise output to the API response.
    /// </summary>
    /// <param name="output">The Core output. Cannot be <see langword="null" />.</param>
    /// <returns>The corresponding API response.</returns>
    public static LessonExerciseResponse FromCoreLessonExerciseOutput(LessonExerciseOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        return new LessonExerciseResponse
        {
            Id = output.Id,
            Ordinal = output.Ordinal,
            Setup = output.Setup,
            Clue = output.Clue,
            AnswerPattern = output.AnswerPattern,
            Ranges = output.Ranges.Select(PublicRiddleRangeResponse.FromCorePublicRiddleRangeOutput).ToArray(),
            IsComplete = output.IsComplete
        };
    }
}

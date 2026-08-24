using System.Text.Json.Serialization;
using ProjectRiddle.Api.Models.Riddles.Discovery;
using ProjectRiddle.Api.Models.Riddles.Play;
using ProjectRiddle.Core.Models.Courses.Play;

namespace ProjectRiddle.Api.Models.Courses.Play;

/// <summary>
/// Represents play state returned by a course answer, hint, reveal, or resume command.
/// </summary>
public sealed record CoursePlayStateResponse
{
    /// <summary>
    /// Gets the progress snapshot.
    /// </summary>
    public required CourseProgressSnapshotResponse Progress { get; init; }

    /// <summary>
    /// Gets the characters for permitted revealed positions.
    /// </summary>
    public required IReadOnlyList<RevealedLetterResponse> RevealedLetters { get; init; }

    /// <summary>
    /// Gets the normalized answer when a terminal state permits it.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Answer { get; init; }

    /// <summary>
    /// Gets the explanation when a terminal state permits it.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Explanation { get; init; }

    /// <summary>
    /// Gets the post-solve teaching note when a terminal state permits it.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TeachingNote { get; init; }

    /// <summary>
    /// Gets a value indicating whether the submitted answer is correct, when this response is an answer result.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsCorrect { get; init; }

    /// <summary>
    /// Maps a Core play state to the API response.
    /// </summary>
    /// <param name="output">The Core output. Cannot be <see langword="null" />.</param>
    /// <returns>The corresponding API response.</returns>
    public static CoursePlayStateResponse FromCoreCoursePlayStateOutput(CoursePlayStateOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        return new CoursePlayStateResponse
        {
            Progress = CourseProgressSnapshotResponse.FromCoreCourseProgressSnapshotOutput(output.Progress),
            RevealedLetters = output.RevealedLetters
                .Select(RevealedLetterResponse.FromCoreRevealedLetterOutput)
                .ToArray(),
            Answer = output.Answer,
            Explanation = output.Explanation,
            TeachingNote = output.TeachingNote,
            IsCorrect = output.IsCorrect
        };
    }
}

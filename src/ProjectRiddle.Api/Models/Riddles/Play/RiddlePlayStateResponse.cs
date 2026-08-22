using System.Text.Json.Serialization;
using ProjectRiddle.Api.Models.Riddles.Progress;
using ProjectRiddle.Core.Models.Riddles.Play;

namespace ProjectRiddle.Api.Models.Riddles.Play;

/// <summary>
/// Represents play-state returned by resume, hint, reveal, or answer operations.
/// </summary>
public sealed record RiddlePlayStateResponse
{
    /// <summary>
    /// Gets the progress snapshot.
    /// </summary>
    public required RiddleProgressSnapshotResponse Progress { get; init; }

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
    /// Gets the final explanation when a terminal state permits it.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Explanation { get; init; }

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
    public static RiddlePlayStateResponse FromCoreRiddlePlayStateOutput(RiddlePlayStateOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        return new RiddlePlayStateResponse
        {
            Progress = RiddleProgressSnapshotResponse.FromCoreRiddleProgressSnapshotOutput(output.Progress),
            RevealedLetters = output.RevealedLetters.Select(RevealedLetterResponse.FromCoreRevealedLetterOutput)
                .ToArray(),
            Answer = output.Answer,
            Explanation = output.Explanation,
            IsCorrect = output.IsCorrect
        };
    }
}

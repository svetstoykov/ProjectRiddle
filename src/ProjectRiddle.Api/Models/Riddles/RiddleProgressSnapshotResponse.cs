using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Models.Riddles;

namespace ProjectRiddle.Api.Models.Riddles;

/// <summary>
/// Represents a progress snapshot without answer characters.
/// </summary>
public sealed record RiddleProgressSnapshotResponse
{
    /// <summary>
    /// Gets the riddle identifier.
    /// </summary>
    public required Guid RiddleId { get; init; }

    /// <summary>
    /// Gets the local publication date.
    /// </summary>
    public required DateOnly PublicationDate { get; init; }

    /// <summary>
    /// Gets the play status.
    /// </summary>
    public required RiddleProgressStatus Status { get; init; }

    /// <summary>
    /// Gets the total number of accepted answer submissions.
    /// </summary>
    public required int AnswerAttemptCount { get; init; }

    /// <summary>
    /// Gets the recorded structural hint kinds.
    /// </summary>
    public required IReadOnlyList<RiddleRangeKind> UsedHints { get; init; }

    /// <summary>
    /// Gets the unique revealed letter positions.
    /// </summary>
    public required IReadOnlyList<int> RevealedPositions { get; init; }

    /// <summary>
    /// Gets the number of unique revealed letter positions.
    /// </summary>
    public required int LetterRevealCount { get; init; }

    /// <summary>
    /// Maps a Core snapshot to the API response.
    /// </summary>
    /// <param name="output">The Core output. Cannot be <see langword="null" />.</param>
    /// <returns>The corresponding API response.</returns>
    public static RiddleProgressSnapshotResponse FromCoreRiddleProgressSnapshotOutput(
        RiddleProgressSnapshotOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        return new RiddleProgressSnapshotResponse
        {
            RiddleId = output.RiddleId,
            PublicationDate = output.PublicationDate,
            Status = output.Status,
            AnswerAttemptCount = output.AnswerAttemptCount,
            UsedHints = output.UsedHints,
            RevealedPositions = output.RevealedPositions,
            LetterRevealCount = output.LetterRevealCount
        };
    }
}

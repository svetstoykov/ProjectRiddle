using ProjectRiddle.Core.Enums.Riddles;

namespace ProjectRiddle.Core.Models.Riddles;

/// <summary>
/// Represents a progress snapshot without answer characters.
/// </summary>
/// <param name="RiddleId">The riddle identifier.</param>
/// <param name="PublicationDate">The local publication date.</param>
/// <param name="Status">The play status.</param>
/// <param name="AnswerAttemptCount">The total number of accepted answer submissions.</param>
/// <param name="UsedHints">The recorded structural hint kinds. Cannot be <see langword="null" />.</param>
/// <param name="RevealedPositions">The unique revealed letter positions. Cannot be <see langword="null" />.</param>
/// <param name="LetterRevealCount">The number of unique revealed letter positions.</param>
public sealed record RiddleProgressSnapshotOutput(
    Guid RiddleId,
    DateOnly PublicationDate,
    RiddleProgressStatus Status,
    int AnswerAttemptCount,
    IReadOnlyList<RiddleRangeKind> UsedHints,
    IReadOnlyList<int> RevealedPositions,
    int LetterRevealCount);

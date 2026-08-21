using ProjectRiddle.Core.Enums.Riddles;

namespace ProjectRiddle.Core.Models.Riddles;

/// <summary>
/// Represents a self-asserted anonymous riddle progress snapshot.
/// </summary>
/// <param name="SchemaVersion">The snapshot schema version.</param>
/// <param name="RiddleId">The riddle identifier.</param>
/// <param name="PublicationDate">The claimed local publication date.</param>
/// <param name="Status">The claimed play status.</param>
/// <param name="AnswerAttemptCount">The claimed attempt total.</param>
/// <param name="UsedHints">The claimed structural hint kinds. Cannot be <see langword="null" />.</param>
/// <param name="RevealedPositions">The claimed revealed letter positions. Cannot be <see langword="null" />.</param>
public sealed record AnonymousRiddleProgressInput(
    int SchemaVersion,
    Guid RiddleId,
    DateOnly PublicationDate,
    RiddleProgressStatus Status,
    int AnswerAttemptCount,
    IReadOnlyList<RiddleRangeKind> UsedHints,
    IReadOnlyList<int> RevealedPositions);

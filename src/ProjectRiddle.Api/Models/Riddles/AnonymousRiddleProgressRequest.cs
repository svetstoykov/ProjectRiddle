using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Models.Riddles;

namespace ProjectRiddle.Api.Models.Riddles;

/// <summary>
/// Represents a self-asserted anonymous riddle progress snapshot.
/// </summary>
public sealed record AnonymousRiddleProgressRequest
{
    /// <summary>
    /// Gets the snapshot schema version.
    /// </summary>
    public required int SchemaVersion { get; init; }

    /// <summary>
    /// Gets the riddle identifier.
    /// </summary>
    public required Guid RiddleId { get; init; }

    /// <summary>
    /// Gets the claimed local publication date.
    /// </summary>
    public required DateOnly PublicationDate { get; init; }

    /// <summary>
    /// Gets the claimed play status.
    /// </summary>
    public required RiddleProgressStatus Status { get; init; }

    /// <summary>
    /// Gets the claimed attempt total.
    /// </summary>
    public required int AnswerAttemptCount { get; init; }

    /// <summary>
    /// Gets the claimed structural hint kinds.
    /// </summary>
    public IReadOnlyList<RiddleRangeKind> UsedHints { get; init; } = [];

    /// <summary>
    /// Gets the claimed revealed letter positions.
    /// </summary>
    public IReadOnlyList<int> RevealedPositions { get; init; } = [];

    /// <summary>
    /// Maps the request to a Core anonymous progress input.
    /// </summary>
    /// <returns>The corresponding Core input.</returns>
    public AnonymousRiddleProgressInput ToCoreAnonymousRiddleProgressInput()
    {
        return new AnonymousRiddleProgressInput(
            SchemaVersion,
            RiddleId,
            PublicationDate,
            Status,
            AnswerAttemptCount,
            UsedHints,
            RevealedPositions);
    }
}

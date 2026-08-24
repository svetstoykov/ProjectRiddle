using ProjectRiddle.Core.Enums.Riddles;

namespace ProjectRiddle.Core.Models.Play;

/// <summary>
/// Represents play state for one clue, independent of any calendar or capability-specific identity.
/// </summary>
/// <param name="Status">The play status.</param>
/// <param name="AnswerAttemptCount">The total number of accepted answer submissions.</param>
/// <param name="UsedHints">The recorded structural hint kinds. Cannot be <see langword="null" />.</param>
/// <param name="RevealedPositions">The unique revealed letter positions. Cannot be <see langword="null" />.</param>
/// <remarks>
/// This record deliberately carries no publication date. That is what lets a dateless lesson riddle use the same
/// play engine as a dated daily riddle.
/// </remarks>
public sealed record CluePlayState(
    RiddleProgressStatus Status,
    int AnswerAttemptCount,
    IReadOnlyList<RiddleRangeKind> UsedHints,
    IReadOnlyList<int> RevealedPositions);

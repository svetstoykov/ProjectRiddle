using ProjectRiddle.Core.Enums.Riddles;

namespace ProjectRiddle.Core.Models.Courses.Play;

/// <summary>
/// Represents play progress for one lesson exercise without answer characters.
/// </summary>
/// <param name="ExerciseId">The exercise identifier. The riddle behind it is never disclosed.</param>
/// <param name="Status">The play status.</param>
/// <param name="AnswerAttemptCount">The total number of accepted answer submissions.</param>
/// <param name="UsedHints">The recorded structural hint kinds. Cannot be <see langword="null" />.</param>
/// <param name="RevealedPositions">The unique revealed letter positions. Cannot be <see langword="null" />.</param>
/// <param name="LetterRevealCount">The number of unique revealed letter positions.</param>
public sealed record CourseProgressSnapshotOutput(
    Guid ExerciseId,
    RiddleProgressStatus Status,
    int AnswerAttemptCount,
    IReadOnlyList<RiddleRangeKind> UsedHints,
    IReadOnlyList<int> RevealedPositions,
    int LetterRevealCount);

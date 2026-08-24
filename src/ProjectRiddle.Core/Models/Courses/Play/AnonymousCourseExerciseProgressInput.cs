using ProjectRiddle.Core.Enums.Riddles;

namespace ProjectRiddle.Core.Models.Courses.Play;

/// <summary>
/// Represents a self-asserted anonymous play snapshot for one lesson exercise.
/// </summary>
/// <param name="SchemaVersion">The snapshot schema version.</param>
/// <param name="ExerciseId">The exercise identifier the snapshot claims to describe.</param>
/// <param name="Status">The claimed play status.</param>
/// <param name="AnswerAttemptCount">The claimed attempt total.</param>
/// <param name="UsedHints">The claimed structural hint kinds. Cannot be <see langword="null" />.</param>
/// <param name="RevealedPositions">The claimed revealed letter positions. Cannot be <see langword="null" />.</param>
/// <remarks>
/// Unlike the daily-riddle snapshot this carries no publication date. A lesson exercise has none, which is exactly
/// what lets it share the play engine.
/// </remarks>
public sealed record AnonymousCourseExerciseProgressInput(
    int SchemaVersion,
    Guid ExerciseId,
    RiddleProgressStatus Status,
    int AnswerAttemptCount,
    IReadOnlyList<RiddleRangeKind> UsedHints,
    IReadOnlyList<int> RevealedPositions);

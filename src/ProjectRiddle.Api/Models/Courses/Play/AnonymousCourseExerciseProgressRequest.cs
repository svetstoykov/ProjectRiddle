using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Models.Courses.Play;

namespace ProjectRiddle.Api.Models.Courses.Play;

/// <summary>
/// Represents a self-asserted anonymous play snapshot for one lesson exercise.
/// </summary>
public sealed record AnonymousCourseExerciseProgressRequest
{
    /// <summary>
    /// Gets the snapshot schema version.
    /// </summary>
    public required int SchemaVersion { get; init; }

    /// <summary>
    /// Gets the exercise identifier the snapshot claims to describe.
    /// </summary>
    public required Guid ExerciseId { get; init; }

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
    /// Maps the request to a Core snapshot input.
    /// </summary>
    /// <returns>The corresponding Core input.</returns>
    public AnonymousCourseExerciseProgressInput ToCoreAnonymousCourseExerciseProgressInput()
    {
        return new AnonymousCourseExerciseProgressInput(
            SchemaVersion,
            ExerciseId,
            Status,
            AnswerAttemptCount,
            UsedHints,
            RevealedPositions);
    }
}

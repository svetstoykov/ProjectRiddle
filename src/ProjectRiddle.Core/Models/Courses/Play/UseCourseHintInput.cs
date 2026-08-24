using ProjectRiddle.Core.Enums.Riddles;

namespace ProjectRiddle.Core.Models.Courses.Play;

/// <summary>
/// Represents a request to record one structural hint kind on a lesson exercise.
/// </summary>
/// <param name="ExerciseId">The exercise identifier.</param>
/// <param name="Kind">The structural hint kind.</param>
/// <param name="Progress">The optional anonymous play snapshot.</param>
public sealed record UseCourseHintInput(
    Guid ExerciseId,
    RiddleRangeKind Kind,
    AnonymousCourseExerciseProgressInput? Progress);

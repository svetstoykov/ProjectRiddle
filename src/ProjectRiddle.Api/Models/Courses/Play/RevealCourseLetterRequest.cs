using ProjectRiddle.Core.Models.Courses.Play;

namespace ProjectRiddle.Api.Models.Courses.Play;

/// <summary>
/// Represents a request to reveal one previously unrevealed letter of a lesson exercise.
/// </summary>
public sealed record RevealCourseLetterRequest
{
    /// <summary>
    /// Gets the optional anonymous play snapshot.
    /// </summary>
    public AnonymousCourseExerciseProgressRequest? Progress { get; init; }

    /// <summary>
    /// Maps the request to a Core reveal input.
    /// </summary>
    /// <param name="exerciseId">The exercise identifier from the route.</param>
    /// <returns>The corresponding Core input.</returns>
    public RevealCourseLetterInput ToCoreRevealCourseLetterInput(Guid exerciseId)
    {
        return new RevealCourseLetterInput(
            exerciseId,
            Progress?.ToCoreAnonymousCourseExerciseProgressInput());
    }
}

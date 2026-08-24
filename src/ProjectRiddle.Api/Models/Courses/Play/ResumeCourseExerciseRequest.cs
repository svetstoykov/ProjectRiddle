using ProjectRiddle.Core.Models.Courses.Play;

namespace ProjectRiddle.Api.Models.Courses.Play;

/// <summary>
/// Represents a request to rehydrate permitted play state for a lesson exercise.
/// </summary>
public sealed record ResumeCourseExerciseRequest
{
    /// <summary>
    /// Gets the optional anonymous play snapshot.
    /// </summary>
    public AnonymousCourseExerciseProgressRequest? Progress { get; init; }

    /// <summary>
    /// Maps the request to a Core resume input.
    /// </summary>
    /// <param name="exerciseId">The exercise identifier from the route.</param>
    /// <returns>The corresponding Core input.</returns>
    public ResumeCourseExerciseInput ToCoreResumeCourseExerciseInput(Guid exerciseId)
    {
        return new ResumeCourseExerciseInput(
            exerciseId,
            Progress?.ToCoreAnonymousCourseExerciseProgressInput());
    }
}

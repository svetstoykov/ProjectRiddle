using System.ComponentModel.DataAnnotations;
using ProjectRiddle.Core.Models.Courses.Play;

namespace ProjectRiddle.Api.Models.Courses.Play;

/// <summary>
/// Represents a submitted answer for a lesson exercise.
/// </summary>
public sealed record SubmitCourseAnswerRequest
{
    /// <summary>
    /// Gets the submitted answer.
    /// </summary>
    [Required]
    public required string Answer { get; init; }

    /// <summary>
    /// Gets the optional anonymous play snapshot.
    /// </summary>
    public AnonymousCourseExerciseProgressRequest? Progress { get; init; }

    /// <summary>
    /// Maps the request to a Core answer input.
    /// </summary>
    /// <param name="exerciseId">The exercise identifier from the route.</param>
    /// <returns>The corresponding Core input.</returns>
    public SubmitCourseAnswerInput ToCoreSubmitCourseAnswerInput(Guid exerciseId)
    {
        return new SubmitCourseAnswerInput(
            exerciseId,
            Answer,
            Progress?.ToCoreAnonymousCourseExerciseProgressInput());
    }
}

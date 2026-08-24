using System.ComponentModel.DataAnnotations;
using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Models.Courses.Play;

namespace ProjectRiddle.Api.Models.Courses.Play;

/// <summary>
/// Represents a request to record one structural hint kind on a lesson exercise.
/// </summary>
public sealed record UseCourseHintRequest
{
    /// <summary>
    /// Gets the structural hint kind.
    /// </summary>
    [Required]
    public required RiddleRangeKind Kind { get; init; }

    /// <summary>
    /// Gets the optional anonymous play snapshot.
    /// </summary>
    public AnonymousCourseExerciseProgressRequest? Progress { get; init; }

    /// <summary>
    /// Maps the request to a Core hint input.
    /// </summary>
    /// <param name="exerciseId">The exercise identifier from the route.</param>
    /// <returns>The corresponding Core input.</returns>
    public UseCourseHintInput ToCoreUseCourseHintInput(Guid exerciseId)
    {
        return new UseCourseHintInput(
            exerciseId,
            Kind,
            Progress?.ToCoreAnonymousCourseExerciseProgressInput());
    }
}

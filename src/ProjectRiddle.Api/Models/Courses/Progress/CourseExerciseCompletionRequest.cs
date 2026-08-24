using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Models.Courses.Progress;

namespace ProjectRiddle.Api.Models.Courses.Progress;

/// <summary>
/// Represents one completed exercise in an imported course progress snapshot.
/// </summary>
public sealed record CourseExerciseCompletionRequest
{
    /// <summary>
    /// Gets the exercise identifier.
    /// </summary>
    public required Guid ExerciseId { get; init; }

    /// <summary>
    /// Gets the completion status.
    /// </summary>
    public required RiddleProgressStatus Status { get; init; }

    /// <summary>
    /// Maps the request to a Core completion input.
    /// </summary>
    /// <returns>The corresponding Core input.</returns>
    public CourseExerciseCompletionInput ToCoreCourseExerciseCompletionInput()
    {
        return new CourseExerciseCompletionInput(ExerciseId, Status);
    }
}

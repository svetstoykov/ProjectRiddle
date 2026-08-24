namespace ProjectRiddle.Core.Models.Courses.Play;

/// <summary>
/// Represents a submitted answer for a lesson exercise.
/// </summary>
/// <param name="ExerciseId">The exercise identifier.</param>
/// <param name="Answer">The submitted answer. Cannot be <see langword="null" />.</param>
/// <param name="Progress">The optional anonymous play snapshot.</param>
public sealed record SubmitCourseAnswerInput(
    Guid ExerciseId,
    string Answer,
    AnonymousCourseExerciseProgressInput? Progress);

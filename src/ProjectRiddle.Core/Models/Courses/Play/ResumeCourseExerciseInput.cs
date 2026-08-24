namespace ProjectRiddle.Core.Models.Courses.Play;

/// <summary>
/// Represents a request to rehydrate permitted play state for a lesson exercise.
/// </summary>
/// <param name="ExerciseId">The exercise identifier.</param>
/// <param name="Progress">The optional anonymous play snapshot.</param>
public sealed record ResumeCourseExerciseInput(Guid ExerciseId, AnonymousCourseExerciseProgressInput? Progress);

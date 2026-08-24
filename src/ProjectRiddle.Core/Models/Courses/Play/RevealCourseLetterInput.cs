namespace ProjectRiddle.Core.Models.Courses.Play;

/// <summary>
/// Represents a request to reveal one previously unrevealed letter of a lesson exercise.
/// </summary>
/// <param name="ExerciseId">The exercise identifier.</param>
/// <param name="Progress">The optional anonymous play snapshot.</param>
public sealed record RevealCourseLetterInput(Guid ExerciseId, AnonymousCourseExerciseProgressInput? Progress);

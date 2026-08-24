using ProjectRiddle.Core.Enums.Riddles;

namespace ProjectRiddle.Core.Models.Courses.Progress;

/// <summary>
/// Represents one completed exercise in an imported course progress snapshot.
/// </summary>
/// <param name="ExerciseId">The exercise identifier.</param>
/// <param name="Status">The outcome: solved, or finished by revealing every letter.</param>
/// <remarks>
/// The outcome is carried rather than reduced to a boolean so an import can reconstruct progress at the right
/// status instead of flattening every completion into a solve.
/// </remarks>
public sealed record CourseExerciseCompletionInput(Guid ExerciseId, RiddleProgressStatus Status);

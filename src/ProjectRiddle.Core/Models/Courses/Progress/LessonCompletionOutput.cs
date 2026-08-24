namespace ProjectRiddle.Core.Models.Courses.Progress;

/// <summary>
/// Represents one lesson's completion for the current account.
/// </summary>
/// <param name="LessonId">The stable lesson identifier.</param>
/// <param name="LessonKey">The lesson key.</param>
/// <param name="CompletedExerciseCount">The number of complete exercises.</param>
/// <param name="ExerciseCount">The number of active exercises in the lesson.</param>
/// <param name="IsComplete">A value indicating whether every active exercise is complete.</param>
public sealed record LessonCompletionOutput(
    Guid LessonId,
    string LessonKey,
    int CompletedExerciseCount,
    int ExerciseCount,
    bool IsComplete);

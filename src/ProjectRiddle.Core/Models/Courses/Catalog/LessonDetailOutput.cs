using ProjectRiddle.Core.Enums.Courses;

namespace ProjectRiddle.Core.Models.Courses.Catalog;

/// <summary>
/// Represents a lesson read: its teaching prose and its ordered safe exercise projections.
/// </summary>
/// <param name="Id">The stable lesson identifier.</param>
/// <param name="Key">The lesson key.</param>
/// <param name="Ordinal">The one-based position within its course.</param>
/// <param name="Title">The lesson title.</param>
/// <param name="Kind">The role the lesson plays.</param>
/// <param name="Intro">The optional technique prose.</param>
/// <param name="PrerequisiteLessonKeys">The lesson keys that must be complete first. Cannot be <see langword="null" />.</param>
/// <param name="Exercises">The active exercises in ordinal order. Cannot be <see langword="null" />.</param>
/// <remarks>
/// This read never carries an answer, an explanation, or a teaching note. Those are released only by a play
/// command at a terminal state.
/// </remarks>
public sealed record LessonDetailOutput(
    Guid Id,
    string Key,
    int Ordinal,
    string Title,
    LessonKind Kind,
    string? Intro,
    IReadOnlyList<string> PrerequisiteLessonKeys,
    IReadOnlyList<LessonExerciseOutput> Exercises);

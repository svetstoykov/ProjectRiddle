using ProjectRiddle.Core.Enums.Courses;

namespace ProjectRiddle.Core.Models.Courses.Catalog;

/// <summary>
/// Represents one lesson in the catalog.
/// </summary>
/// <param name="Id">The stable lesson identifier.</param>
/// <param name="Key">The lesson key.</param>
/// <param name="Ordinal">The one-based position within its course.</param>
/// <param name="Title">The lesson title.</param>
/// <param name="Kind">The role the lesson plays.</param>
/// <param name="ExerciseCount">The number of active exercises beneath the lesson.</param>
/// <param name="PrerequisiteLessonKeys">The lesson keys that must be complete first. Cannot be <see langword="null" />.</param>
/// <param name="Progress">Completion and availability when the caller is signed in; otherwise <see langword="null" />.</param>
/// <remarks>
/// Teaching prose is deliberately absent. It belongs to the lesson read, which keeps the catalog payload small
/// enough to stay an ordinary cacheable read. Prerequisite keys ship for every caller: they are what a signed-out
/// client evaluates its own stored completion against, and what a locked tile names when it says what remains.
/// </remarks>
public sealed record LessonOutput(
    Guid Id,
    string Key,
    int Ordinal,
    string Title,
    LessonKind Kind,
    int ExerciseCount,
    IReadOnlyList<string> PrerequisiteLessonKeys,
    LessonProgressOutput? Progress);

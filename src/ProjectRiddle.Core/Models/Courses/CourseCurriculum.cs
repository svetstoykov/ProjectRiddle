using ProjectRiddle.Core.Models.Riddles;

namespace ProjectRiddle.Core.Models.Courses;

/// <summary>
/// Represents a manifest that has passed validation, projected into the rows a seed writes.
/// </summary>
/// <param name="Courses">The courses with their lessons and exercises, in ordinal order. Cannot be <see langword="null" />.</param>
/// <param name="LessonRiddles">The riddles holding the lesson clues. Cannot be <see langword="null" />.</param>
/// <param name="PrimerPages">The primer pages in ordinal order. Cannot be <see langword="null" />.</param>
/// <remarks>
/// The lesson riddles travel with the curriculum because they are written in the same transaction. A separate
/// repository could not join that transaction, and a partially seeded curriculum would leave exercises pointing at
/// clues that do not exist.
/// </remarks>
public sealed record CourseCurriculum(
    IReadOnlyList<Course> Courses,
    IReadOnlyList<Riddle> LessonRiddles,
    IReadOnlyList<PrimerPage> PrimerPages);

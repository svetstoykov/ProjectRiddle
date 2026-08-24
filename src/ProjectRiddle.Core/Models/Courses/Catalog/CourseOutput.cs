namespace ProjectRiddle.Core.Models.Courses.Catalog;

/// <summary>
/// Represents one course in the catalog.
/// </summary>
/// <param name="Id">The stable course identifier.</param>
/// <param name="Key">The course key.</param>
/// <param name="Ordinal">The one-based position within the curriculum.</param>
/// <param name="Title">The course title.</param>
/// <param name="Intro">The framing prose for the course page.</param>
/// <param name="Lessons">The active lessons in ordinal order. Cannot be <see langword="null" />.</param>
public sealed record CourseOutput(
    Guid Id,
    string Key,
    int Ordinal,
    string Title,
    string Intro,
    IReadOnlyList<LessonOutput> Lessons);

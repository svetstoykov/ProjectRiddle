namespace ProjectRiddle.Core.Models.Courses.Manifest;

/// <summary>
/// Represents one authored course in the shipped manifest.
/// </summary>
/// <param name="Id">The stable course identifier, authored once and never regenerated.</param>
/// <param name="Key">The course key, unique across the manifest.</param>
/// <param name="Ordinal">The one-based position within the curriculum.</param>
/// <param name="Title">The course title.</param>
/// <param name="Intro">The framing prose for the course page.</param>
/// <param name="Lessons">The lessons beneath the course.</param>
public sealed record CourseManifestEntry(
    Guid Id,
    string? Key,
    int Ordinal,
    string? Title,
    string? Intro,
    IReadOnlyList<LessonManifestEntry>? Lessons);

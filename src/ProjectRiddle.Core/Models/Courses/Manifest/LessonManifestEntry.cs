using ProjectRiddle.Core.Enums.Courses;

namespace ProjectRiddle.Core.Models.Courses.Manifest;

/// <summary>
/// Represents one authored lesson in the shipped manifest.
/// </summary>
/// <param name="Id">The stable lesson identifier, authored once and never regenerated.</param>
/// <param name="Key">The lesson key, unique across the manifest.</param>
/// <param name="Ordinal">The one-based position within its course.</param>
/// <param name="Kind">The role the lesson plays.</param>
/// <param name="Title">The lesson title.</param>
/// <param name="Intro">The optional technique prose; absent for a mixed set.</param>
/// <param name="PrerequisiteLessonKeys">The lesson keys that must be complete first.</param>
/// <param name="Exercises">The exercises beneath the lesson.</param>
public sealed record LessonManifestEntry(
    Guid Id,
    string? Key,
    int Ordinal,
    LessonKind Kind,
    string? Title,
    string? Intro,
    IReadOnlyList<string>? PrerequisiteLessonKeys,
    IReadOnlyList<LessonExerciseManifestEntry>? Exercises);

namespace ProjectRiddle.Core.Models.Courses.Progress;

/// <summary>
/// Represents a bounded, versioned snapshot of anonymous course completion offered for import.
/// </summary>
/// <param name="SchemaVersion">The snapshot schema version.</param>
/// <param name="Entries">The completed exercises. Cannot be <see langword="null" />.</param>
/// <remarks>
/// This contract can never grant a role, archive access, unpublished content, or any other authorization outcome.
/// It never accepts a caller-supplied account identifier; identity comes only from the current-account boundary.
/// </remarks>
public sealed record AnonymousCourseProgressInput(
    int SchemaVersion,
    IReadOnlyList<CourseExerciseCompletionInput> Entries);

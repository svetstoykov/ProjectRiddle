using ProjectRiddle.Core.Models.Courses.Progress;

namespace ProjectRiddle.Api.Models.Courses.Progress;

/// <summary>
/// Represents an imported anonymous course completion snapshot.
/// </summary>
public sealed record ImportCourseProgressRequest
{
    /// <summary>
    /// Gets the snapshot schema version.
    /// </summary>
    public required int SchemaVersion { get; init; }

    /// <summary>
    /// Gets the completed exercise entries.
    /// </summary>
    public IReadOnlyList<CourseExerciseCompletionRequest> Entries { get; init; } = [];

    /// <summary>
    /// Maps the request to a Core anonymous course progress input.
    /// </summary>
    /// <returns>The corresponding Core input.</returns>
    public AnonymousCourseProgressInput ToCoreAnonymousCourseProgressInput()
    {
        return new AnonymousCourseProgressInput(
            SchemaVersion,
            Entries.Select(entry => entry.ToCoreCourseExerciseCompletionInput()).ToArray());
    }
}

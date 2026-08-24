namespace ProjectRiddle.Core.Constants.Courses;

/// <summary>
/// Provides the bounds applied to anonymous and imported course progress.
/// </summary>
public static class CourseLimits
{
    /// <summary>
    /// The supported anonymous course progress schema version.
    /// </summary>
    public const int AnonymousProgressSchemaVersion = 1;

    /// <summary>
    /// The maximum number of completed exercise identifiers accepted in one import.
    /// </summary>
    public const int MaxImportedExerciseCount = 200;
}

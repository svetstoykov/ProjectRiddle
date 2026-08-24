namespace ProjectRiddle.Core.Constants.Courses;

/// <summary>
/// Provides stable codes for expected Courses capability failures.
/// </summary>
/// <remarks>
/// Play-level failures keep the <c>riddles.*</c> codes the shared engine produces. A lesson exercise genuinely is
/// a riddle, so a parallel play code set would be churn without meaning.
/// </remarks>
public static class CourseErrorCodes
{
    /// <summary>
    /// Identifies a missing course.
    /// </summary>
    public const string NotFound = "courses.notFound";

    /// <summary>
    /// Identifies a missing or deactivated lesson.
    /// </summary>
    public const string LessonNotFound = "courses.lesson.notFound";

    /// <summary>
    /// Identifies a missing or deactivated lesson exercise.
    /// </summary>
    public const string ExerciseNotFound = "courses.exercise.notFound";

    /// <summary>
    /// Identifies an anonymous or imported course progress payload with an invalid shape or bound.
    /// </summary>
    public const string ProgressInvalid = "courses.progress.invalid";

    /// <summary>
    /// Identifies a course progress payload that refers to content which is not active lesson content.
    /// </summary>
    public const string ProgressReferenceInvalid = "courses.progress.referenceInvalid";
}

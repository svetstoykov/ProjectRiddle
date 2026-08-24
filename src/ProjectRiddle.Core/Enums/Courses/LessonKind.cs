namespace ProjectRiddle.Core.Enums.Courses;

/// <summary>
/// Defines the role a lesson plays within the curriculum.
/// </summary>
/// <remarks>
/// The kind describes what a lesson is, not what gates it. Every prerequisite is authored explicitly, so nothing
/// derives a progression from this enum at runtime.
/// </remarks>
public enum LessonKind
{
    /// <summary>
    /// Indicates a lesson that teaches one technique and drills it.
    /// </summary>
    Technique = 0,

    /// <summary>
    /// Indicates the mixed practice set that sits behind one course's techniques.
    /// </summary>
    Mix = 1,

    /// <summary>
    /// Indicates the single mixed set drawn from the whole curriculum.
    /// </summary>
    FinalMix = 2
}

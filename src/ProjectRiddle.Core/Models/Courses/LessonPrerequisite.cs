namespace ProjectRiddle.Core.Models.Courses;

/// <summary>
/// Represents one lesson key that must be complete before its owning lesson becomes available.
/// </summary>
public sealed class LessonPrerequisite
{
    /// <summary>
    /// Initializes a prerequisite.
    /// </summary>
    /// <param name="lessonKey">The required lesson key. Cannot be <see langword="null" /> or whitespace.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="lessonKey" /> is empty or whitespace.</exception>
    public LessonPrerequisite(string lessonKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lessonKey);
        LessonKey = lessonKey;
    }

    /// <summary>
    /// Gets the required lesson key.
    /// </summary>
    public string LessonKey { get; }
}

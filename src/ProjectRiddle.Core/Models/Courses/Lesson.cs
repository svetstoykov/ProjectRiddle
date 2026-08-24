using ProjectRiddle.Core.Enums.Courses;

namespace ProjectRiddle.Core.Models.Courses;

/// <summary>
/// Represents one lesson: teaching prose, an authored prerequisite list, and its ordered exercises.
/// </summary>
public sealed class Lesson
{
    private readonly List<LessonPrerequisite> _prerequisites;
    private readonly List<LessonExercise> _exercises;

    /// <summary>
    /// Initializes a lesson.
    /// </summary>
    /// <param name="id">The stable lesson identifier from the manifest. Cannot be <see cref="Guid.Empty" />.</param>
    /// <param name="key">The lesson key, unique across the curriculum. Cannot be <see langword="null" /> or whitespace.</param>
    /// <param name="ordinal">The one-based position within its course. Must be greater than zero.</param>
    /// <param name="title">The lesson title. Cannot be <see langword="null" /> or whitespace.</param>
    /// <param name="intro">The optional technique prose; absent for a mixed set.</param>
    /// <param name="kind">The role the lesson plays.</param>
    /// <param name="isActive">A value indicating whether the lesson is still part of the shipped curriculum.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="id" /> is empty or <paramref name="ordinal" /> is not positive.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key" /> or <paramref name="title" /> is empty or whitespace.</exception>
    public Lesson(Guid id, string key, int ordinal, string title, string? intro, LessonKind kind, bool isActive)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(id, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ordinal);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        Id = id;
        Key = key;
        Ordinal = ordinal;
        Title = title;
        Intro = intro;
        Kind = kind;
        IsActive = isActive;
        _prerequisites = [];
        _exercises = [];
    }

    /// <summary>
    /// Gets the stable lesson identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the lesson key, unique across the curriculum.
    /// </summary>
    public string Key { get; private set; }

    /// <summary>
    /// Gets the one-based position within its course.
    /// </summary>
    public int Ordinal { get; private set; }

    /// <summary>
    /// Gets the lesson title.
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Gets the optional technique prose.
    /// </summary>
    public string? Intro { get; private set; }

    /// <summary>
    /// Gets the role the lesson plays.
    /// </summary>
    public LessonKind Kind { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the lesson is still part of the shipped curriculum.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets the lesson keys that must be complete before this lesson becomes available.
    /// </summary>
    public IReadOnlyList<LessonPrerequisite> Prerequisites => _prerequisites;

    /// <summary>
    /// Gets the exercises belonging to the lesson.
    /// </summary>
    public IReadOnlyList<LessonExercise> Exercises => _exercises;

    /// <summary>
    /// Replaces the authored fields of the lesson and marks it active.
    /// </summary>
    /// <param name="key">The lesson key. Cannot be <see langword="null" /> or whitespace.</param>
    /// <param name="ordinal">The one-based position within its course. Must be greater than zero.</param>
    /// <param name="title">The lesson title. Cannot be <see langword="null" /> or whitespace.</param>
    /// <param name="intro">The optional technique prose.</param>
    /// <param name="kind">The role the lesson plays.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="ordinal" /> is not positive.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key" /> or <paramref name="title" /> is empty or whitespace.</exception>
    public void ReplaceContent(string key, int ordinal, string title, string? intro, LessonKind kind)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ordinal);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        Key = key;
        Ordinal = ordinal;
        Title = title;
        Intro = intro;
        Kind = kind;
        IsActive = true;
    }

    /// <summary>
    /// Replaces the authored prerequisite list.
    /// </summary>
    /// <param name="prerequisites">The prerequisites. Cannot be <see langword="null" />.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="prerequisites" /> is <see langword="null" />.</exception>
    public void ReplacePrerequisites(IReadOnlyList<LessonPrerequisite> prerequisites)
    {
        ArgumentNullException.ThrowIfNull(prerequisites);
        _prerequisites.Clear();
        _prerequisites.AddRange(prerequisites);
    }

    /// <summary>
    /// Replaces the exercise collection.
    /// </summary>
    /// <param name="exercises">The exercises. Cannot be <see langword="null" />.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exercises" /> is <see langword="null" />.</exception>
    public void ReplaceExercises(IReadOnlyList<LessonExercise> exercises)
    {
        ArgumentNullException.ThrowIfNull(exercises);
        _exercises.Clear();
        _exercises.AddRange(exercises);
    }

    /// <summary>
    /// Withdraws the lesson and every exercise beneath it from the shipped curriculum.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        foreach (var exercise in _exercises)
        {
            exercise.Deactivate();
        }
    }
}

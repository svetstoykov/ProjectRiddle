namespace ProjectRiddle.Core.Models.Courses;

/// <summary>
/// Represents one course: framing prose and the lessons beneath it.
/// </summary>
/// <remarks>
/// Courses never lock. The final mixed set is modelled as a course of its own holding a single lesson, which keeps
/// the schema uniform and gives that set its own page.
/// </remarks>
public sealed class Course
{
    private readonly List<Lesson> _lessons;

    /// <summary>
    /// Initializes a course.
    /// </summary>
    /// <param name="id">The stable course identifier from the manifest. Cannot be <see cref="Guid.Empty" />.</param>
    /// <param name="key">The course key, unique across the curriculum. Cannot be <see langword="null" /> or whitespace.</param>
    /// <param name="ordinal">The one-based position within the curriculum. Must be greater than zero.</param>
    /// <param name="title">The course title. Cannot be <see langword="null" /> or whitespace.</param>
    /// <param name="intro">The framing prose for the course page. Cannot be <see langword="null" /> or whitespace.</param>
    /// <param name="isActive">A value indicating whether the course is still part of the shipped curriculum.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="id" /> is empty or <paramref name="ordinal" /> is not positive.</exception>
    /// <exception cref="ArgumentException">Thrown when a required string argument is empty or whitespace.</exception>
    public Course(Guid id, string key, int ordinal, string title, string intro, bool isActive)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(id, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ordinal);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(intro);

        Id = id;
        Key = key;
        Ordinal = ordinal;
        Title = title;
        Intro = intro;
        IsActive = isActive;
        _lessons = [];
    }

    /// <summary>
    /// Gets the stable course identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the course key, unique across the curriculum.
    /// </summary>
    public string Key { get; private set; }

    /// <summary>
    /// Gets the one-based position within the curriculum.
    /// </summary>
    public int Ordinal { get; private set; }

    /// <summary>
    /// Gets the course title.
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Gets the framing prose for the course page.
    /// </summary>
    public string Intro { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the course is still part of the shipped curriculum.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets the lessons beneath the course.
    /// </summary>
    public IReadOnlyList<Lesson> Lessons => _lessons;

    /// <summary>
    /// Replaces the authored fields of the course and marks it active.
    /// </summary>
    /// <param name="key">The course key. Cannot be <see langword="null" /> or whitespace.</param>
    /// <param name="ordinal">The one-based position within the curriculum. Must be greater than zero.</param>
    /// <param name="title">The course title. Cannot be <see langword="null" /> or whitespace.</param>
    /// <param name="intro">The framing prose. Cannot be <see langword="null" /> or whitespace.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="ordinal" /> is not positive.</exception>
    /// <exception cref="ArgumentException">Thrown when a required string argument is empty or whitespace.</exception>
    public void ReplaceContent(string key, int ordinal, string title, string intro)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ordinal);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(intro);

        Key = key;
        Ordinal = ordinal;
        Title = title;
        Intro = intro;
        IsActive = true;
    }

    /// <summary>
    /// Replaces the lesson collection.
    /// </summary>
    /// <param name="lessons">The lessons. Cannot be <see langword="null" />.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="lessons" /> is <see langword="null" />.</exception>
    public void ReplaceLessons(IReadOnlyList<Lesson> lessons)
    {
        ArgumentNullException.ThrowIfNull(lessons);
        _lessons.Clear();
        _lessons.AddRange(lessons);
    }

    /// <summary>
    /// Withdraws the course and everything beneath it from the shipped curriculum.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        foreach (var lesson in _lessons)
        {
            lesson.Deactivate();
        }
    }
}

namespace ProjectRiddle.Core.Models.Courses;

/// <summary>
/// Represents one clue placed in a lesson, with the ordering and teaching copy that belong to it there.
/// </summary>
/// <remarks>
/// The setup line and the teaching note describe the clue <em>within this lesson</em> rather than the clue itself,
/// which is why they live here and not on the riddle. This type also keeps the dependency one-way: Courses
/// references a riddle, and Riddles knows nothing about courses.
/// </remarks>
public sealed class LessonExercise
{
    /// <summary>
    /// Initializes a lesson exercise.
    /// </summary>
    /// <param name="id">The stable exercise identifier from the manifest. Cannot be <see cref="Guid.Empty" />.</param>
    /// <param name="riddleId">The riddle holding the clue. Cannot be <see cref="Guid.Empty" />.</param>
    /// <param name="ordinal">The one-based position within the lesson. Must be greater than zero.</param>
    /// <param name="setup">The optional one-line nudge shown before solving.</param>
    /// <param name="teachingNote">The optional one-line note shown after solving.</param>
    /// <param name="isActive">A value indicating whether the exercise is still part of the shipped curriculum.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an identifier is empty or <paramref name="ordinal" /> is not positive.</exception>
    public LessonExercise(
        Guid id,
        Guid riddleId,
        int ordinal,
        string? setup,
        string? teachingNote,
        bool isActive)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(id, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfEqual(riddleId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ordinal);

        Id = id;
        RiddleId = riddleId;
        Ordinal = ordinal;
        Setup = setup;
        TeachingNote = teachingNote;
        IsActive = isActive;
    }

    /// <summary>
    /// Gets the stable exercise identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the identifier of the riddle holding the clue.
    /// </summary>
    public Guid RiddleId { get; private set; }

    /// <summary>
    /// Gets the one-based position within the lesson.
    /// </summary>
    public int Ordinal { get; private set; }

    /// <summary>
    /// Gets the optional one-line nudge shown before solving.
    /// </summary>
    public string? Setup { get; private set; }

    /// <summary>
    /// Gets the optional one-line note shown after solving.
    /// </summary>
    /// <remarks>
    /// This is post-solve teaching copy and is therefore answer-sensitive. It is withheld under exactly the same
    /// terminal-state rule as the answer and the explanation.
    /// </remarks>
    public string? TeachingNote { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the exercise is still part of the shipped curriculum.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Replaces the authored fields of the exercise and marks it active.
    /// </summary>
    /// <param name="riddleId">The riddle holding the clue. Cannot be <see cref="Guid.Empty" />.</param>
    /// <param name="ordinal">The one-based position within the lesson. Must be greater than zero.</param>
    /// <param name="setup">The optional one-line nudge shown before solving.</param>
    /// <param name="teachingNote">The optional one-line note shown after solving.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="riddleId" /> is empty or <paramref name="ordinal" /> is not positive.</exception>
    public void ReplaceContent(Guid riddleId, int ordinal, string? setup, string? teachingNote)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(riddleId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ordinal);

        RiddleId = riddleId;
        Ordinal = ordinal;
        Setup = setup;
        TeachingNote = teachingNote;
        IsActive = true;
    }

    /// <summary>
    /// Withdraws the exercise from the shipped curriculum without deleting it.
    /// </summary>
    /// <remarks>
    /// Deactivation rather than deletion is what protects a learner's completion of content later withdrawn.
    /// </remarks>
    public void Deactivate()
    {
        IsActive = false;
    }
}

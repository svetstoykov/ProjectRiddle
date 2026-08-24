using ProjectRiddle.Core.Enums.Riddles;

namespace ProjectRiddle.Core.Models.Riddles;

/// <summary>
/// Represents a persisted riddle and its publication state.
/// </summary>
public sealed class Riddle
{
    private readonly List<RiddleRange> _ranges;

    /// <summary>
    /// Initializes a riddle.
    /// </summary>
    /// <param name="id">The stable riddle identifier. Cannot be <see cref="Guid.Empty" />.</param>
    /// <param name="clue">The clue text. Cannot be <see langword="null" /> or whitespace.</param>
    /// <param name="answer">The answer text. Cannot be <see langword="null" /> or whitespace.</param>
    /// <param name="answerPattern">The answer pattern. Cannot be <see langword="null" /> or whitespace.</param>
    /// <param name="explanation">The explanation text. Cannot be <see langword="null" /> or whitespace.</param>
    /// <param name="isLesson">A value indicating whether the riddle is course lesson content rather than a daily riddle.</param>
    /// <param name="publicationState">The current publication state.</param>
    /// <param name="sofiaPublicationDate">The Sofia calendar date when the riddle occupies or occupied the calendar.</param>
    /// <param name="createdAtUtc">The UTC timestamp when the riddle was created.</param>
    /// <param name="updatedAtUtc">The UTC timestamp when the riddle was last changed.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="id" /> is empty.</exception>
    /// <exception cref="ArgumentException">Thrown when a required string argument is empty or whitespace.</exception>
    public Riddle(
        Guid id,
        string clue,
        string answer,
        string answerPattern,
        string explanation,
        bool isLesson,
        RiddlePublicationState publicationState,
        DateOnly? sofiaPublicationDate,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(id, Guid.Empty);
        ArgumentException.ThrowIfNullOrWhiteSpace(clue);
        ArgumentException.ThrowIfNullOrWhiteSpace(answer);
        ArgumentException.ThrowIfNullOrWhiteSpace(answerPattern);
        ArgumentException.ThrowIfNullOrWhiteSpace(explanation);

        Id = id;
        Clue = clue;
        Answer = answer;
        AnswerPattern = answerPattern;
        Explanation = explanation;
        IsLesson = isLesson;
        PublicationState = publicationState;
        SofiaPublicationDate = sofiaPublicationDate;
        _ranges = [];
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    /// <summary>
    /// Gets the stable riddle identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the clue text.
    /// </summary>
    public string Clue { get; private set; }

    /// <summary>
    /// Gets the stored answer text.
    /// </summary>
    public string Answer { get; private set; }

    /// <summary>
    /// Gets the stored answer pattern.
    /// </summary>
    public string AnswerPattern { get; private set; }

    /// <summary>
    /// Gets the stored explanation.
    /// </summary>
    public string Explanation { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the riddle is course lesson content rather than a daily riddle.
    /// </summary>
    /// <remarks>
    /// Lesson content is stored without a publication date, so it cannot occupy the daily calendar. This flag is
    /// what additionally keeps it out of the administrator listing, which is the only riddle read that does not
    /// already filter on a publication date.
    /// </remarks>
    public bool IsLesson { get; private set; }

    /// <summary>
    /// Gets the current publication state.
    /// </summary>
    public RiddlePublicationState PublicationState { get; private set; }

    /// <summary>
    /// Gets the Sofia calendar date when the riddle occupies or occupied the calendar.
    /// </summary>
    public DateOnly? SofiaPublicationDate { get; private set; }

    /// <summary>
    /// Gets the labelled structural ranges.
    /// </summary>
    public IReadOnlyList<RiddleRange> Ranges => _ranges;

    /// <summary>
    /// Replaces the authored content of the riddle.
    /// </summary>
    /// <param name="clue">The clue text. Cannot be <see langword="null" /> or whitespace.</param>
    /// <param name="answer">The answer text. Cannot be <see langword="null" /> or whitespace.</param>
    /// <param name="answerPattern">The answer pattern. Cannot be <see langword="null" /> or whitespace.</param>
    /// <param name="explanation">The explanation text. Cannot be <see langword="null" /> or whitespace.</param>
    /// <param name="updatedAtUtc">The UTC timestamp of the change.</param>
    /// <exception cref="ArgumentException">Thrown when a required string argument is empty or whitespace.</exception>
    /// <remarks>
    /// This exists so a content release can update lesson text in place. Replacing the row instead would cascade
    /// away the progress records that reference it.
    /// </remarks>
    public void ReplaceContent(
        string clue,
        string answer,
        string answerPattern,
        string explanation,
        DateTimeOffset updatedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clue);
        ArgumentException.ThrowIfNullOrWhiteSpace(answer);
        ArgumentException.ThrowIfNullOrWhiteSpace(answerPattern);
        ArgumentException.ThrowIfNullOrWhiteSpace(explanation);

        Clue = clue;
        Answer = answer;
        AnswerPattern = answerPattern;
        Explanation = explanation;
        UpdatedAtUtc = updatedAtUtc;
    }

    /// <summary>
    /// Gets the UTC timestamp when the riddle was created.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; }

    /// <summary>
    /// Gets the UTC timestamp when the riddle was last changed.
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Replaces the labelled structural ranges.
    /// </summary>
    /// <param name="contentRanges">The labelled structural ranges. Cannot be <see langword="null" />.</param>
    public void ReplaceRanges(IReadOnlyList<RiddleRange> contentRanges)
    {
        ArgumentNullException.ThrowIfNull(contentRanges);
        _ranges.Clear();
        _ranges.AddRange(contentRanges);
    }

    /// <summary>
    /// Reserves a Sofia calendar date without publishing the riddle.
    /// </summary>
    /// <param name="publicationDate">The Sofia calendar date to occupy.</param>
    /// <param name="updatedAtUtc">The UTC timestamp of the change.</param>
    public void Schedule(DateOnly publicationDate, DateTimeOffset updatedAtUtc)
    {
        PublicationState = RiddlePublicationState.Scheduled;
        SofiaPublicationDate = publicationDate;
        UpdatedAtUtc = updatedAtUtc;
    }

    /// <summary>
    /// Publishes the riddle onto a Sofia calendar date.
    /// </summary>
    /// <param name="publicationDate">The Sofia calendar date to occupy.</param>
    /// <param name="updatedAtUtc">The UTC timestamp of the change.</param>
    public void Publish(DateOnly publicationDate, DateTimeOffset updatedAtUtc)
    {
        PublicationState = RiddlePublicationState.Published;
        SofiaPublicationDate = publicationDate;
        UpdatedAtUtc = updatedAtUtc;
    }

    /// <summary>
    /// Withdraws the riddle from the calendar so its date may be reused.
    /// </summary>
    /// <param name="updatedAtUtc">The UTC timestamp of the change.</param>
    public void Unpublish(DateTimeOffset updatedAtUtc)
    {
        PublicationState = RiddlePublicationState.Unpublished;
        UpdatedAtUtc = updatedAtUtc;
    }
}

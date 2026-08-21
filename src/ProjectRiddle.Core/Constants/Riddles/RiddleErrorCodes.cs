namespace ProjectRiddle.Core.Constants.Riddles;

/// <summary>
/// Provides stable codes for expected Riddles capability failures.
/// </summary>
public static class RiddleErrorCodes
{
    /// <summary>
    /// Identifies a missing riddle.
    /// </summary>
    public const string NotFound = "riddles.notFound";

    /// <summary>
    /// Identifies clue text that is missing or whitespace.
    /// </summary>
    public const string ClueInvalid = "riddles.clue.invalid";

    /// <summary>
    /// Identifies answer text that is missing, whitespace, or has no letters in a word.
    /// </summary>
    public const string AnswerInvalid = "riddles.answer.invalid";

    /// <summary>
    /// Identifies explanation text that is missing or whitespace.
    /// </summary>
    public const string ExplanationInvalid = "riddles.explanation.invalid";

    /// <summary>
    /// Identifies a structural range that is malformed or outside the clue.
    /// </summary>
    public const string RangeInvalid = "riddles.ranges.invalid";

    /// <summary>
    /// Identifies a Sofia publication date that is missing or not legal for the requested transition.
    /// </summary>
    public const string PublicationDateInvalid = "riddles.publicationDate.invalid";

    /// <summary>
    /// Identifies a Sofia publication date already occupied by another scheduled or published riddle.
    /// </summary>
    public const string PublicationDateConflict = "riddles.publicationDate.conflict";

    /// <summary>
    /// Identifies a publication transition that is not legal for the current state.
    /// </summary>
    public const string TransitionInvalid = "riddles.transition.invalid";

    /// <summary>
    /// Identifies a delete that is not permitted for the current publication state.
    /// </summary>
    public const string DeleteNotPermitted = "riddles.delete.notPermitted";

    /// <summary>
    /// Identifies that today's eligible public riddle is unavailable.
    /// </summary>
    public const string TodayUnavailable = "riddles.today.unavailable";

    /// <summary>
    /// Identifies that archive play requires an authenticated account.
    /// </summary>
    public const string ArchiveAuthenticationRequired = "riddles.archive.authenticationRequired";

    /// <summary>
    /// Identifies an empty or malformed submitted answer.
    /// </summary>
    public const string AnswerRequestInvalid = "riddles.answer.invalid";

    /// <summary>
    /// Identifies an unknown structural hint kind.
    /// </summary>
    public const string HintKindInvalid = "riddles.hint.kind.invalid";

    /// <summary>
    /// Identifies an invalid anonymous or imported progress shape.
    /// </summary>
    public const string ProgressInvalid = "riddles.progress.invalid";

    /// <summary>
    /// Identifies a progress snapshot that refers to the wrong or missing riddle.
    /// </summary>
    public const string ProgressReferenceInvalid = "riddles.progress.referenceInvalid";

    /// <summary>
    /// Identifies revealed positions that are outside the answer or inconsistent with status.
    /// </summary>
    public const string ProgressPositionInvalid = "riddles.progress.positionInvalid";

    /// <summary>
    /// Identifies an archive page or page size that is outside the allowed bounds.
    /// </summary>
    public const string ArchivePageInvalid = "riddles.archive.page.invalid";
}

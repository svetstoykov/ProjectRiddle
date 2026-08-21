namespace ProjectRiddle.Core.Services.Riddles;

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
    /// Identifies answer text that is missing or whitespace.
    /// </summary>
    public const string AnswerInvalid = "riddles.answer.invalid";

    /// <summary>
    /// Identifies an answer pattern that is missing, malformed, or inconsistent with the answer.
    /// </summary>
    public const string AnswerPatternInvalid = "riddles.answerPattern.invalid";

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
}

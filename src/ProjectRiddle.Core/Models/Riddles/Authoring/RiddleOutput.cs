using ProjectRiddle.Core.Enums.Riddles;

namespace ProjectRiddle.Core.Models.Riddles.Authoring;

/// <summary>
/// Represents the administrative projection of a riddle, including answer-sensitive fields.
/// </summary>
/// <param name="Id">The stable riddle identifier.</param>
/// <param name="Clue">The clue text.</param>
/// <param name="Answer">The stored answer text.</param>
/// <param name="AnswerPattern">The stored answer pattern.</param>
/// <param name="Explanation">The stored explanation.</param>
/// <param name="PublicationState">The current publication state.</param>
/// <param name="SofiaPublicationDate">The Sofia calendar date when the riddle occupies or occupied the calendar.</param>
/// <param name="Ranges">The labelled structural ranges.</param>
/// <param name="CreatedAtUtc">The UTC timestamp when the riddle was created.</param>
/// <param name="UpdatedAtUtc">The UTC timestamp when the riddle was last changed.</param>
public sealed record RiddleOutput(
    Guid Id,
    string Clue,
    string Answer,
    string AnswerPattern,
    string Explanation,
    RiddlePublicationState PublicationState,
    DateOnly? SofiaPublicationDate,
    IReadOnlyList<RiddleRangeOutput> Ranges,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

namespace ProjectRiddle.Core.Models.Riddles;

/// <summary>
/// Represents the initial public play projection without progress or answer-sensitive fields.
/// </summary>
/// <param name="Id">The stable riddle identifier.</param>
/// <param name="PublicationDate">The local publication date.</param>
/// <param name="Clue">The full clue text.</param>
/// <param name="AnswerPattern">The public answer pattern.</param>
/// <param name="Ranges">The safe structural ranges. Cannot be <see langword="null" />.</param>
public sealed record PublicRiddlePlayOutput(
    Guid Id,
    DateOnly PublicationDate,
    string Clue,
    string AnswerPattern,
    IReadOnlyList<PublicRiddleRangeOutput> Ranges);

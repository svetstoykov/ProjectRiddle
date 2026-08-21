namespace ProjectRiddle.Core.Models.Riddles;

/// <summary>
/// Represents a safe public discovery item for weekly or archive lists.
/// </summary>
/// <param name="Id">The stable riddle identifier.</param>
/// <param name="PublicationDate">The local publication date.</param>
/// <param name="ClueExcerpt">The bounded clue excerpt.</param>
/// <param name="AnswerPattern">The public answer pattern.</param>
public sealed record PublicRiddleDiscoveryItemOutput(
    Guid Id,
    DateOnly PublicationDate,
    string ClueExcerpt,
    string AnswerPattern);

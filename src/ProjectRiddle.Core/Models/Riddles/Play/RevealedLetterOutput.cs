namespace ProjectRiddle.Core.Models.Riddles.Play;

/// <summary>
/// Represents one permitted revealed letter.
/// </summary>
/// <param name="Position">The zero-based letter position.</param>
/// <param name="Character">The revealed character at that position.</param>
public sealed record RevealedLetterOutput(int Position, char Character);

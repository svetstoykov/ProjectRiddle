using ProjectRiddle.Core.Models.Riddles.Play;

namespace ProjectRiddle.Api.Models.Riddles.Play;

/// <summary>
/// Represents one permitted revealed letter.
/// </summary>
public sealed record RevealedLetterResponse
{
    /// <summary>
    /// Gets the zero-based letter position.
    /// </summary>
    public required int Position { get; init; }

    /// <summary>
    /// Gets the revealed character at that position.
    /// </summary>
    public required char Character { get; init; }

    /// <summary>
    /// Maps a Core revealed letter to the API response.
    /// </summary>
    /// <param name="output">The Core output. Cannot be <see langword="null" />.</param>
    /// <returns>The corresponding API response.</returns>
    public static RevealedLetterResponse FromCoreRevealedLetterOutput(RevealedLetterOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        return new RevealedLetterResponse
        {
            Position = output.Position,
            Character = output.Character
        };
    }
}

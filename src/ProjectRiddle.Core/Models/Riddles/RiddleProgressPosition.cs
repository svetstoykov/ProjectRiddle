namespace ProjectRiddle.Core.Models.Riddles;

/// <summary>
/// Represents one revealed letter position recorded on a riddle progress snapshot.
/// </summary>
public sealed class RiddleProgressPosition
{
    /// <summary>
    /// Initializes a revealed letter position.
    /// </summary>
    /// <param name="letterPosition">The zero-based letter position. Cannot be negative.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="letterPosition" /> is negative.</exception>
    public RiddleProgressPosition(int letterPosition)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(letterPosition);
        LetterPosition = letterPosition;
    }

    /// <summary>
    /// Gets the zero-based letter position, excluding word separators.
    /// </summary>
    public int LetterPosition { get; }
}

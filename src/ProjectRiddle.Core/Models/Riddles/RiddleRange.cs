using ProjectRiddle.Core.Enums.Riddles;

namespace ProjectRiddle.Core.Models.Riddles;

/// <summary>
/// Represents a labelled structural range within a clue.
/// </summary>
public sealed class RiddleRange
{
    /// <summary>
    /// Initializes a structural range.
    /// </summary>
    /// <param name="id">The stable range identifier. Cannot be <see cref="Guid.Empty" />.</param>
    /// <param name="kind">The structural role of the range.</param>
    /// <param name="start">The inclusive UTF-16 start index within the clue.</param>
    /// <param name="end">The exclusive UTF-16 end index within the clue. Must be greater than <paramref name="start" />.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="id" /> is empty, <paramref name="start" /> is negative, or <paramref name="end" /> is not greater than <paramref name="start" />.</exception>
    public RiddleRange(Guid id, RiddleRangeKind kind, int start, int end)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(id, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfNegative(start);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(end, start);

        Id = id;
        Kind = kind;
        Start = start;
        End = end;
    }

    /// <summary>
    /// Gets the stable range identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the structural role of the range.
    /// </summary>
    public RiddleRangeKind Kind { get; }

    /// <summary>
    /// Gets the inclusive UTF-16 start index within the clue.
    /// </summary>
    public int Start { get; }

    /// <summary>
    /// Gets the exclusive UTF-16 end index within the clue.
    /// </summary>
    public int End { get; }
}

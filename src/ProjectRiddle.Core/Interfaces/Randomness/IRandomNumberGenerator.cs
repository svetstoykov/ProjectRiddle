namespace ProjectRiddle.Core.Interfaces.Randomness;

/// <summary>
/// Provides non-negative random integers for domain selection.
/// </summary>
public interface IRandomNumberGenerator
{
    /// <summary>
    /// Returns a non-negative random integer that is less than <paramref name="exclusiveUpperBound" />.
    /// </summary>
    /// <param name="exclusiveUpperBound">The exclusive upper bound. Must be greater than zero.</param>
    /// <returns>An integer in the range <c>[0, exclusiveUpperBound)</c>.</returns>
    int NextExclusive(int exclusiveUpperBound);
}

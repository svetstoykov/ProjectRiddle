using ProjectRiddle.Core.Interfaces.Randomness;

namespace ProjectRiddle.Infrastructure.Randomness;

/// <summary>
/// Provides random integers from the shared system generator.
/// </summary>
public sealed class SystemRandomNumberGenerator : IRandomNumberGenerator
{
    /// <inheritdoc />
    public int NextExclusive(int exclusiveUpperBound)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(exclusiveUpperBound);
        return Random.Shared.Next(exclusiveUpperBound);
    }
}

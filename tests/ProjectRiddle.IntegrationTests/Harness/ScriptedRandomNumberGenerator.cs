using ProjectRiddle.Core.Interfaces.Randomness;

namespace ProjectRiddle.IntegrationTests.Harness;

/// <summary>
/// Returns scripted integers so letter-reveal tests stay deterministic.
/// </summary>
public sealed class ScriptedRandomNumberGenerator : IRandomNumberGenerator
{
    private readonly Queue<int> _values;

    /// <summary>
    /// Initializes the generator with queued return values.
    /// </summary>
    /// <param name="values">The values to return from <see cref="NextExclusive" />. Cannot be <see langword="null" />.</param>
    public ScriptedRandomNumberGenerator(params int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        _values = new Queue<int>(values);
    }

    /// <inheritdoc />
    public int NextExclusive(int exclusiveUpperBound)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(exclusiveUpperBound);
        var value = _values.Count == 0 ? 0 : _values.Dequeue();
        if (value < 0 || value >= exclusiveUpperBound)
        {
            throw new InvalidOperationException("The scripted random value is outside the requested range.");
        }

        return value;
    }
}

namespace ProjectRiddle.Core.Interfaces.Time;

/// <summary>
/// Provides the current instant to application code.
/// </summary>
public interface IClock
{
    /// <summary>
    /// Gets the current instant in UTC.
    /// </summary>
    DateTimeOffset UtcNow { get; }
}

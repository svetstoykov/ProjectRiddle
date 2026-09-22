namespace ProjectRiddle.Core.Exceptions;

/// <summary>
/// Represents a riddle write that lost an optimistic-concurrency check.
/// </summary>
public sealed class StaleRiddleWriteException : Exception
{
    /// <summary>
    /// Initializes the exception.
    /// </summary>
    public StaleRiddleWriteException()
        : base("The riddle was changed by another operation before this write could be stored.")
    {
    }
}

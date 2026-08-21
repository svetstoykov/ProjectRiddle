namespace ProjectRiddle.Core.Exceptions;

/// <summary>
/// Represents a uniqueness conflict while saving account riddle progress.
/// </summary>
public sealed class DuplicateRiddleProgressException : Exception
{
    /// <summary>
    /// Initializes the exception.
    /// </summary>
    public DuplicateRiddleProgressException()
        : base("The riddle progress write conflicted with an existing unique record.")
    {
    }
}

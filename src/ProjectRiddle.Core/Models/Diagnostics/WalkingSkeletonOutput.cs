namespace ProjectRiddle.Core.Models.Diagnostics;

/// <summary>
/// Represents the successful output of the Phase 0 walking-skeleton operation.
/// </summary>
public sealed record WalkingSkeletonOutput
{
    /// <summary>
    /// Initializes walking-skeleton output.
    /// </summary>
    /// <param name="message">The safe readiness message.</param>
    /// <param name="publicationDate">The current publication date.</param>
    public WalkingSkeletonOutput(string message, DateOnly publicationDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        Message = message;
        PublicationDate = publicationDate;
    }

    /// <summary>
    /// Gets the safe readiness message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the current publication date.
    /// </summary>
    public DateOnly PublicationDate { get; }
}

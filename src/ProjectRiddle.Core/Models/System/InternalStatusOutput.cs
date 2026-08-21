namespace ProjectRiddle.Core.Models.System;

/// <summary>
/// Represents the current internal application status.
/// </summary>
public sealed record InternalStatusOutput
{
    /// <summary>
    /// Initializes an internal status output.
    /// </summary>
    /// <param name="message">The safe status message.</param>
    /// <param name="utcDateTime">The current UTC date-time.</param>
    /// <param name="localDateTime">The current configured local date-time.</param>
    public InternalStatusOutput(
        string message,
        DateTimeOffset utcDateTime,
        DateTimeOffset localDateTime)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        Message = message;
        UtcDateTime = utcDateTime;
        LocalDateTime = localDateTime;
    }

    /// <summary>
    /// Gets the safe status message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the current UTC date-time.
    /// </summary>
    public DateTimeOffset UtcDateTime { get; }

    /// <summary>
    /// Gets the current configured local date-time.
    /// </summary>
    public DateTimeOffset LocalDateTime { get; }
}

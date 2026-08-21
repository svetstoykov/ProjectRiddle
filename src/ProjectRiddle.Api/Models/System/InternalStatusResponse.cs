using ProjectRiddle.Core.Models.System;

namespace ProjectRiddle.Api.Models.System;

/// <summary>
/// Represents the public internal application status response.
/// </summary>
public sealed class InternalStatusResponse
{
    /// <summary>
    /// Initializes an internal status response.
    /// </summary>
    /// <param name="message">The safe status message.</param>
    /// <param name="utcDateTime">The current UTC date-time.</param>
    /// <param name="localDateTime">The current configured local date-time.</param>
    public InternalStatusResponse(
        string message,
        DateTimeOffset utcDateTime,
        DateTimeOffset localDateTime)
    {
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

    /// <summary>
    /// Maps a Core output to the public response contract.
    /// </summary>
    /// <param name="output">The Core output to map. Cannot be <see langword="null" />.</param>
    /// <returns>The corresponding API response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="output" /> is <see langword="null" />.</exception>
    public static InternalStatusResponse FromCoreOutput(InternalStatusOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        return new InternalStatusResponse(output.Message, output.UtcDateTime, output.LocalDateTime);
    }
}

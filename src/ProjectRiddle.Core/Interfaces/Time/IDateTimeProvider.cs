namespace ProjectRiddle.Core.Interfaces.Time;

/// <summary>
/// Provides the current UTC and configured local date-times.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current date-time in UTC.
    /// </summary>
    DateTimeOffset UtcDateTime { get; }

    /// <summary>
    /// Gets the current date-time in the configured local time zone.
    /// </summary>
    DateTimeOffset LocalDateTime { get; }

    /// <summary>
    /// Gets the current calendar date in the configured local time zone.
    /// </summary>
    DateOnly LocalDate { get; }

    /// <summary>
    /// Gets the UTC instant at which the next configured-local calendar day begins.
    /// </summary>
    /// <remarks>
    /// The instant is strictly later than <see cref="UtcDateTime" />. Callers recalculate it after each wait so a
    /// daylight-saving transition changes the following boundary.
    /// </remarks>
    DateTimeOffset NextLocalMidnightUtc { get; }
}

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
}

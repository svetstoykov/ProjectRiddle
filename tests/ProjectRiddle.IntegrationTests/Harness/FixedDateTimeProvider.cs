using ProjectRiddle.Core.Interfaces.Time;

namespace ProjectRiddle.IntegrationTests.Harness;

/// <summary>
/// Provides a controllable clock for deterministic Sofia-date tests.
/// </summary>
public sealed class FixedDateTimeProvider : IDateTimeProvider
{
    private readonly TimeZoneInfo timeZone;

    /// <summary>
    /// Initializes a controllable clock.
    /// </summary>
    /// <param name="utcNow">The current UTC instant.</param>
    /// <param name="timeZoneId">The configured local time-zone identifier.</param>
    public FixedDateTimeProvider(DateTimeOffset utcNow, string timeZoneId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(timeZoneId);

        UtcDateTime = utcNow;
        timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
    }

    /// <inheritdoc />
    public DateTimeOffset UtcDateTime { get; set; }

    /// <inheritdoc />
    public DateTimeOffset LocalDateTime => TimeZoneInfo.ConvertTime(UtcDateTime, timeZone);

    /// <inheritdoc />
    public DateOnly LocalDate
    {
        get
        {
            var localDateTime = LocalDateTime;
            return new DateOnly(localDateTime.Year, localDateTime.Month, localDateTime.Day);
        }
    }
}

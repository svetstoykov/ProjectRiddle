using Microsoft.Extensions.Options;
using ProjectRiddle.Core.Interfaces.Time;
using ProjectRiddle.Infrastructure.Configuration;

namespace ProjectRiddle.Infrastructure.Time;

/// <summary>
/// Provides current UTC and configured local date-times.
/// </summary>
public sealed class DateTimeProvider : IDateTimeProvider
{
    private readonly TimeZoneInfo localTimeZone;

    /// <summary>
    /// Initializes the date-time provider.
    /// </summary>
    /// <param name="options">The validated time settings.</param>
    /// <exception cref="TimeZoneNotFoundException">Thrown when the configured time zone cannot be found.</exception>
    /// <exception cref="InvalidTimeZoneException">Thrown when the configured time zone data is invalid.</exception>
    public DateTimeProvider(IOptions<TimeOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZoneId);
    }

    /// <inheritdoc />
    public DateTimeOffset UtcDateTime => DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public DateTimeOffset LocalDateTime => TimeZoneInfo.ConvertTime(UtcDateTime, localTimeZone);
}

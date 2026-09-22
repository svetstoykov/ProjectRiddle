namespace ProjectRiddle.Core.Time;

/// <summary>
/// Calculates configured-local calendar boundaries from a UTC instant.
/// </summary>
public static class ConfiguredLocalTime
{
    /// <summary>
    /// Gets the UTC instant at which the next local calendar day begins.
    /// </summary>
    /// <param name="utcNow">The current UTC instant.</param>
    /// <param name="timeZone">The configured local time zone. Cannot be <see langword="null" />.</param>
    /// <returns>The UTC instant of the next local midnight, strictly later than <paramref name="utcNow" />.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="timeZone" /> is <see langword="null" />.</exception>
    /// <remarks>
    /// <para>
    /// A daylight-saving gap at midnight is skipped until the first valid local time. An ambiguous midnight uses
    /// the earlier UTC instant, which is the larger offset.
    /// </para>
    /// </remarks>
    public static DateTimeOffset NextMidnightUtc(DateTimeOffset utcNow, TimeZoneInfo timeZone)
    {
        ArgumentNullException.ThrowIfNull(timeZone);

        var local = TimeZoneInfo.ConvertTime(utcNow, timeZone);
        var nextDate = DateOnly.FromDateTime(local.DateTime).AddDays(1);
        var midnight = nextDate.ToDateTime(TimeOnly.MinValue);

        while (timeZone.IsInvalidTime(midnight))
        {
            midnight = midnight.AddMinutes(1);
        }

        var offset = timeZone.IsAmbiguousTime(midnight)
            ? timeZone.GetAmbiguousTimeOffsets(midnight).Max()
            : timeZone.GetUtcOffset(midnight);

        return new DateTimeOffset(midnight, offset);
    }
}

using Microsoft.Extensions.Options;
using ProjectRiddle.Core.Interfaces.Time;
using ProjectRiddle.Infrastructure.Configuration;

namespace ProjectRiddle.Infrastructure.Time;

/// <summary>
/// Calculates the current publication date once from the configured application time zone.
/// </summary>
public sealed class SofiaPublicationDateProvider : IPublicationDateProvider
{
    private readonly IClock clock;
    private readonly TimeZoneInfo publicationTimeZone;

    /// <summary>
    /// Initializes the publication-date provider.
    /// </summary>
    /// <param name="clock">The clock supplying the current UTC instant.</param>
    /// <param name="options">The validated publication settings.</param>
    /// <exception cref="TimeZoneNotFoundException">Thrown when the configured time zone cannot be found.</exception>
    /// <exception cref="InvalidTimeZoneException">Thrown when the configured time zone data is invalid.</exception>
    public SofiaPublicationDateProvider(IClock clock, IOptions<PublicationOptions> options)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(options);

        this.clock = clock;
        publicationTimeZone = TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZoneId);
    }

    /// <inheritdoc />
    public DateOnly CurrentDate
    {
        get
        {
            var localNow = TimeZoneInfo.ConvertTime(clock.UtcNow, publicationTimeZone);
            return DateOnly.FromDateTime(localNow.DateTime);
        }
    }
}

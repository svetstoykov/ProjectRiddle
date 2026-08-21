using System.ComponentModel.DataAnnotations;

namespace ProjectRiddle.Infrastructure.Configuration;

/// <summary>
/// Defines the runtime settings used for application date-time calculations.
/// </summary>
public sealed class TimeOptions
{
    /// <summary>
    /// Identifies the configuration section containing time settings.
    /// </summary>
    public const string SectionName = "Time";

    /// <summary>
    /// Gets or sets the time-zone identifier used for local date-times.
    /// </summary>
    [Required]
    [MinLength(1)]
    public string TimeZoneId { get; set; } = string.Empty;
}

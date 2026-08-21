using System.ComponentModel.DataAnnotations;

namespace ProjectRiddle.Infrastructure.Configuration;

/// <summary>
/// Defines the runtime settings used for publication-date calculations.
/// </summary>
public sealed class PublicationOptions
{
    /// <summary>
    /// Identifies the configuration section containing publication settings.
    /// </summary>
    public const string SectionName = "Publication";

    /// <summary>
    /// Gets or sets the time-zone identifier used for publication dates.
    /// </summary>
    [Required]
    [MinLength(1)]
    public string TimeZoneId { get; set; } = string.Empty;
}

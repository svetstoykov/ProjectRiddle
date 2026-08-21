using System.ComponentModel.DataAnnotations;

namespace ProjectRiddle.Infrastructure.Configuration;

/// <summary>
/// Defines the runtime settings for the SQLite database.
/// </summary>
public sealed class DatabaseOptions
{
    /// <summary>
    /// Identifies the configuration section containing database settings.
    /// </summary>
    public const string SectionName = "Persistence";

    /// <summary>
    /// Gets or sets the relative or absolute SQLite database path.
    /// </summary>
    [Required]
    [MinLength(1)]
    public string DatabasePath { get; set; } = string.Empty;
}

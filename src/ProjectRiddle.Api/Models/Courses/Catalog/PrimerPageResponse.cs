using System.Text.Json.Serialization;
using ProjectRiddle.Core.Models.Courses.Catalog;

namespace ProjectRiddle.Api.Models.Courses.Catalog;

/// <summary>
/// Represents one guided-course primer page.
/// </summary>
public sealed record PrimerPageResponse
{
    /// <summary>
    /// Gets the one-based page position.
    /// </summary>
    public required int Ordinal { get; init; }

    /// <summary>
    /// Gets the page title.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets the page prose.
    /// </summary>
    public required string Body { get; init; }

    /// <summary>
    /// Gets the optional figure key.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Figure { get; init; }

    /// <summary>
    /// Maps a Core primer page output to the API response.
    /// </summary>
    /// <param name="output">The Core output. Cannot be <see langword="null" />.</param>
    /// <returns>The corresponding API response.</returns>
    public static PrimerPageResponse FromCorePrimerPageOutput(PrimerPageOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        return new PrimerPageResponse
        {
            Ordinal = output.Ordinal,
            Title = output.Title,
            Body = output.Body,
            Figure = output.Figure
        };
    }
}

using ProjectRiddle.Core.Models.Riddles;

namespace ProjectRiddle.Api.Models.Riddles;

/// <summary>
/// Represents a paged public archive of safe riddle metadata.
/// </summary>
public sealed record PublicRiddleListResponse
{
    /// <summary>
    /// Gets the one-based page number.
    /// </summary>
    public required int Page { get; init; }

    /// <summary>
    /// Gets the page size.
    /// </summary>
    public required int PageSize { get; init; }

    /// <summary>
    /// Gets the total number of archive riddles.
    /// </summary>
    public required int TotalCount { get; init; }

    /// <summary>
    /// Gets the page of discovery items.
    /// </summary>
    public required IReadOnlyList<PublicRiddleDiscoveryItemResponse> Items { get; init; }

    /// <summary>
    /// Maps a Core archive page to the API response.
    /// </summary>
    /// <param name="output">The Core output. Cannot be <see langword="null" />.</param>
    /// <returns>The corresponding API response.</returns>
    public static PublicRiddleListResponse FromCorePublicRiddleListOutput(PublicRiddleListOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        return new PublicRiddleListResponse
        {
            Page = output.Page,
            PageSize = output.PageSize,
            TotalCount = output.TotalCount,
            Items = output.Items.Select(PublicRiddleDiscoveryItemResponse.FromCorePublicRiddleDiscoveryItemOutput)
                .ToArray()
        };
    }
}

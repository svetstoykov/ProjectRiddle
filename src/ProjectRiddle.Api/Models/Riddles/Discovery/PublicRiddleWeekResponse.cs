using ProjectRiddle.Core.Models.Riddles.Discovery;

namespace ProjectRiddle.Api.Models.Riddles.Discovery;

/// <summary>
/// Represents safe metadata for the current local week.
/// </summary>
public sealed record PublicRiddleWeekResponse
{
    /// <summary>
    /// Gets the Monday of the current local week.
    /// </summary>
    public required DateOnly WeekStart { get; init; }

    /// <summary>
    /// Gets the Sunday of the current local week.
    /// </summary>
    public required DateOnly WeekEnd { get; init; }

    /// <summary>
    /// Gets the configured local date at the time of the read.
    /// </summary>
    public required DateOnly Today { get; init; }

    /// <summary>
    /// Gets the discovery items in publication-date order.
    /// </summary>
    public required IReadOnlyList<PublicRiddleDiscoveryItemResponse> Items { get; init; }

    /// <summary>
    /// Maps a Core week projection to the API response.
    /// </summary>
    /// <param name="output">The Core output. Cannot be <see langword="null" />.</param>
    /// <returns>The corresponding API response.</returns>
    public static PublicRiddleWeekResponse FromCorePublicRiddleWeekOutput(PublicRiddleWeekOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        return new PublicRiddleWeekResponse
        {
            WeekStart = output.WeekStart,
            WeekEnd = output.WeekEnd,
            Today = output.Today,
            Items = output.Items.Select(PublicRiddleDiscoveryItemResponse.FromCorePublicRiddleDiscoveryItemOutput)
                .ToArray()
        };
    }
}

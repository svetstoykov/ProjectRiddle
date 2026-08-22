using ProjectRiddle.Core.Models.Riddles.Discovery;

namespace ProjectRiddle.Api.Models.Riddles.Discovery;

/// <summary>
/// Represents safe metadata for the current local week.
/// </summary>
public sealed record PublicRiddleWeekResponse
{
    /// <summary>
    /// Gets the discovery items in publication-date order.
    /// </summary>
    public required IReadOnlyList<PublicRiddleDiscoveryItemResponse> Items { get; init; }

    /// <summary>
    /// Maps Core week items to the API response.
    /// </summary>
    /// <param name="items">The Core items. Cannot be <see langword="null" />.</param>
    /// <returns>The corresponding API response.</returns>
    public static PublicRiddleWeekResponse FromCoreWeekItems(IReadOnlyList<PublicRiddleDiscoveryItemOutput> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return new PublicRiddleWeekResponse
        {
            Items = items.Select(PublicRiddleDiscoveryItemResponse.FromCorePublicRiddleDiscoveryItemOutput).ToArray()
        };
    }
}

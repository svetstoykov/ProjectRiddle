namespace ProjectRiddle.Core.Models.Riddles.Discovery;

/// <summary>
/// Represents safe metadata for the current local week together with the configured dates that bound it.
/// </summary>
/// <param name="WeekStart">The Monday of the local week that contains the current local date.</param>
/// <param name="WeekEnd">The Sunday of the local week that contains the current local date.</param>
/// <param name="Today">The configured local date at the time of the read.</param>
/// <param name="Items">The published discovery items in publication-date order. Cannot be <see langword="null" />.</param>
public sealed record PublicRiddleWeekOutput(
    DateOnly WeekStart,
    DateOnly WeekEnd,
    DateOnly Today,
    IReadOnlyList<PublicRiddleDiscoveryItemOutput> Items);

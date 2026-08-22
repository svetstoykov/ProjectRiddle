namespace ProjectRiddle.Core.Models.Riddles.Discovery;

/// <summary>
/// Represents a paged public archive of safe riddle metadata.
/// </summary>
/// <param name="Page">The one-based page number.</param>
/// <param name="PageSize">The page size.</param>
/// <param name="TotalCount">The total number of archive riddles.</param>
/// <param name="Items">The page of discovery items. Cannot be <see langword="null" />.</param>
public sealed record PublicRiddleListOutput(
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyList<PublicRiddleDiscoveryItemOutput> Items);

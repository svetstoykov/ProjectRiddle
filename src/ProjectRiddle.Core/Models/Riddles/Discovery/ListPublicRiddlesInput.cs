namespace ProjectRiddle.Core.Models.Riddles.Discovery;

/// <summary>
/// Represents paging for the public archive list.
/// </summary>
/// <param name="Page">The one-based page number.</param>
/// <param name="PageSize">The page size.</param>
public sealed record ListPublicRiddlesInput(int Page, int PageSize);

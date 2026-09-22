namespace ProjectRiddle.Core.Models.Riddles.Publication;

/// <summary>
/// Represents the outcome of one scheduled-publication reconciliation.
/// </summary>
/// <param name="PublishedRiddleId">The riddle that was published, or <see langword="null" /> when nothing was due.</param>
/// <param name="PublishedDate">The unchanged Sofia date of the published riddle, or <see langword="null" /> when nothing was due.</param>
/// <param name="ExpiredRiddleIds">The riddles that missed publication because a later due schedule was published.</param>
public sealed record PublicationReconciliationOutput(
    Guid? PublishedRiddleId,
    DateOnly? PublishedDate,
    IReadOnlyList<Guid> ExpiredRiddleIds);

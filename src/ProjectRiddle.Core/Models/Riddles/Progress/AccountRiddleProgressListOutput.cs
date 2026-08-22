namespace ProjectRiddle.Core.Models.Riddles.Progress;

/// <summary>
/// Represents account-owned riddle progress for a date range.
/// </summary>
/// <param name="Items">The progress snapshots. Cannot be <see langword="null" />.</param>
public sealed record AccountRiddleProgressListOutput(IReadOnlyList<RiddleProgressSnapshotOutput> Items);

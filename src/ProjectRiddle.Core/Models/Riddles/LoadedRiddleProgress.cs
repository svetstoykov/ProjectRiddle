namespace ProjectRiddle.Core.Models.Riddles;

/// <summary>
/// Represents loaded play progress and whether it has not yet been persisted.
/// </summary>
/// <param name="Progress">The working progress snapshot. Cannot be <see langword="null" />.</param>
/// <param name="IsNew">A value indicating whether the snapshot has not yet been saved.</param>
internal sealed record LoadedRiddleProgress(RiddleProgress Progress, bool IsNew);

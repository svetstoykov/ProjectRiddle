namespace ProjectRiddle.Core.Models.Riddles;

/// <summary>
/// Represents the administrative list of riddles.
/// </summary>
/// <param name="Riddles">The riddles in list order. Cannot be <see langword="null" />.</param>
public sealed record ListRiddlesOutput(IReadOnlyList<RiddleOutput> Riddles);

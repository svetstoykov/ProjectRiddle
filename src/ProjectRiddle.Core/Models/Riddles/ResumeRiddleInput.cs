namespace ProjectRiddle.Core.Models.Riddles;

/// <summary>
/// Represents a request to rehydrate play state.
/// </summary>
/// <param name="RiddleId">The riddle identifier.</param>
/// <param name="Progress">The optional anonymous progress snapshot.</param>
public sealed record ResumeRiddleInput(Guid RiddleId, AnonymousRiddleProgressInput? Progress);

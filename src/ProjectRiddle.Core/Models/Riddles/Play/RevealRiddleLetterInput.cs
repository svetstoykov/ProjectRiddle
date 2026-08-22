using ProjectRiddle.Core.Models.Riddles.Progress;

namespace ProjectRiddle.Core.Models.Riddles.Play;

/// <summary>
/// Represents a request to reveal one previously unrevealed letter.
/// </summary>
/// <param name="RiddleId">The riddle identifier.</param>
/// <param name="Progress">The optional anonymous progress snapshot.</param>
public sealed record RevealRiddleLetterInput(Guid RiddleId, AnonymousRiddleProgressInput? Progress);

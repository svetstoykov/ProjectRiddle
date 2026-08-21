using ProjectRiddle.Core.Enums.Riddles;

namespace ProjectRiddle.Core.Models.Riddles;

/// <summary>
/// Represents a request to record one structural hint kind.
/// </summary>
/// <param name="RiddleId">The riddle identifier.</param>
/// <param name="Kind">The structural hint kind.</param>
/// <param name="Progress">The optional anonymous progress snapshot.</param>
public sealed record UseRiddleHintInput(
    Guid RiddleId,
    RiddleRangeKind Kind,
    AnonymousRiddleProgressInput? Progress);

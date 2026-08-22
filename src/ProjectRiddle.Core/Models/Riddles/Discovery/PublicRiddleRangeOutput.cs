using ProjectRiddle.Core.Enums.Riddles;

namespace ProjectRiddle.Core.Models.Riddles.Discovery;

/// <summary>
/// Represents a safe structural range in a public play projection.
/// </summary>
/// <param name="Kind">The structural role of the range.</param>
/// <param name="Start">The inclusive UTF-16 start index within the clue.</param>
/// <param name="End">The exclusive UTF-16 end index within the clue.</param>
public sealed record PublicRiddleRangeOutput(RiddleRangeKind Kind, int Start, int End);

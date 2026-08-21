using ProjectRiddle.Core.Enums.Riddles;

namespace ProjectRiddle.Core.Models.Riddles;

/// <summary>
/// Represents a structural range included in an administrative riddle projection.
/// </summary>
/// <param name="Id">The stable range identifier.</param>
/// <param name="Kind">The structural role of the range.</param>
/// <param name="Start">The inclusive UTF-16 start index within the clue.</param>
/// <param name="End">The exclusive UTF-16 end index within the clue.</param>
public sealed record RiddleRangeOutput(Guid Id, RiddleRangeKind Kind, int Start, int End);

using ProjectRiddle.Core.Enums.Riddles;

namespace ProjectRiddle.Core.Models.Riddles.Authoring;

/// <summary>
/// Represents a structural range supplied when creating a riddle.
/// </summary>
/// <param name="Kind">The structural role of the range.</param>
/// <param name="Start">The inclusive UTF-16 start index within the clue.</param>
/// <param name="End">The exclusive UTF-16 end index within the clue.</param>
public sealed record RiddleRangeInput(RiddleRangeKind Kind, int Start, int End);

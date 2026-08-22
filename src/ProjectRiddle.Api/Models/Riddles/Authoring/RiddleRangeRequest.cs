using System.ComponentModel.DataAnnotations;
using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Models.Riddles.Authoring;

namespace ProjectRiddle.Api.Models.Riddles.Authoring;

/// <summary>
/// Represents a labelled structural range in a riddle request.
/// </summary>
public sealed record RiddleRangeRequest
{
    /// <summary>
    /// Gets the structural role of the range.
    /// </summary>
    [Required]
    public required RiddleRangeKind Kind { get; init; }

    /// <summary>
    /// Gets the inclusive UTF-16 start index within the clue.
    /// </summary>
    [Required]
    public required int Start { get; init; }

    /// <summary>
    /// Gets the exclusive UTF-16 end index within the clue.
    /// </summary>
    [Required]
    public required int End { get; init; }

    /// <summary>
    /// Maps the request to a Core range input.
    /// </summary>
    /// <returns>The corresponding Core input.</returns>
    public RiddleRangeInput ToCoreRiddleRangeInput()
    {
        return new RiddleRangeInput(Kind, Start, End);
    }
}

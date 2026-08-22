using System.ComponentModel.DataAnnotations;
using ProjectRiddle.Core.Models.Riddles.Authoring;

namespace ProjectRiddle.Api.Models.Riddles.Authoring;

/// <summary>
/// Represents a request to create a riddle.
/// </summary>
public sealed record CreateRiddleRequest
{
    /// <summary>
    /// Gets the clue text.
    /// </summary>
    [Required]
    public required string Clue { get; init; }

    /// <summary>
    /// Gets the answer text.
    /// </summary>
    [Required]
    public required string Answer { get; init; }

    /// <summary>
    /// Gets the explanation text.
    /// </summary>
    [Required]
    public required string Explanation { get; init; }

    /// <summary>
    /// Gets the labelled structural ranges.
    /// </summary>
    public IReadOnlyList<RiddleRangeRequest> Ranges { get; init; } = [];

    /// <summary>
    /// Maps the request to a Core create input.
    /// </summary>
    /// <returns>The corresponding Core input.</returns>
    public CreateRiddleInput ToCoreCreateRiddleInput()
    {
        var ranges = Ranges.Select(range => range.ToCoreRiddleRangeInput()).ToArray();
        return new CreateRiddleInput(Clue, Answer, Explanation, ranges);
    }
}

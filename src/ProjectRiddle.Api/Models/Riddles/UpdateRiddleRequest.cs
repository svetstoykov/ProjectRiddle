using System.ComponentModel.DataAnnotations;
using ProjectRiddle.Core.Models.Riddles;

namespace ProjectRiddle.Api.Models.Riddles;

/// <summary>
/// Represents a request to update riddle content.
/// </summary>
public sealed record UpdateRiddleRequest
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
    /// Gets the answer pattern.
    /// </summary>
    [Required]
    public required string AnswerPattern { get; init; }

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
    /// Maps the request to a Core update input.
    /// </summary>
    /// <param name="id">The riddle identifier from the route.</param>
    /// <returns>The corresponding Core input.</returns>
    public UpdateRiddleInput ToCoreUpdateRiddleInput(Guid id)
    {
        var ranges = Ranges.Select(range => range.ToCoreRiddleRangeInput()).ToArray();
        return new UpdateRiddleInput(id, Clue, Answer, AnswerPattern, Explanation, ranges);
    }
}

using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Models.Riddles;

namespace ProjectRiddle.Api.Models.Riddles;

/// <summary>
/// Represents a labelled structural range in an administrative riddle response.
/// </summary>
public sealed record RiddleRangeResponse
{
    /// <summary>
    /// Gets the stable range identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the structural role of the range.
    /// </summary>
    public required RiddleRangeKind Kind { get; init; }

    /// <summary>
    /// Gets the inclusive UTF-16 start index within the clue.
    /// </summary>
    public required int Start { get; init; }

    /// <summary>
    /// Gets the exclusive UTF-16 end index within the clue.
    /// </summary>
    public required int End { get; init; }

    /// <summary>
    /// Maps a Core range output to the API response.
    /// </summary>
    /// <param name="output">The Core output. Cannot be <see langword="null" />.</param>
    /// <returns>The corresponding API response.</returns>
    public static RiddleRangeResponse FromCoreRiddleRangeOutput(RiddleRangeOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        return new RiddleRangeResponse
        {
            Id = output.Id,
            Kind = output.Kind,
            Start = output.Start,
            End = output.End
        };
    }
}

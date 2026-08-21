using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Models.Riddles;

namespace ProjectRiddle.Api.Models.Riddles;

/// <summary>
/// Represents a safe structural range in a public play projection.
/// </summary>
public sealed record PublicRiddleRangeResponse
{
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
    public static PublicRiddleRangeResponse FromCorePublicRiddleRangeOutput(PublicRiddleRangeOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        return new PublicRiddleRangeResponse
        {
            Kind = output.Kind,
            Start = output.Start,
            End = output.End
        };
    }
}

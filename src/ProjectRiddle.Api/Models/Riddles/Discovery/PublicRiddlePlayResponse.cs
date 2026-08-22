using ProjectRiddle.Core.Models.Riddles.Discovery;

namespace ProjectRiddle.Api.Models.Riddles.Discovery;

/// <summary>
/// Represents the initial public play projection.
/// </summary>
public sealed record PublicRiddlePlayResponse
{
    /// <summary>
    /// Gets the stable riddle identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the local publication date.
    /// </summary>
    public required DateOnly PublicationDate { get; init; }

    /// <summary>
    /// Gets the full clue text.
    /// </summary>
    public required string Clue { get; init; }

    /// <summary>
    /// Gets the public answer pattern.
    /// </summary>
    public required string AnswerPattern { get; init; }

    /// <summary>
    /// Gets the safe structural ranges.
    /// </summary>
    public required IReadOnlyList<PublicRiddleRangeResponse> Ranges { get; init; }

    /// <summary>
    /// Maps a Core play projection to the API response.
    /// </summary>
    /// <param name="output">The Core output. Cannot be <see langword="null" />.</param>
    /// <returns>The corresponding API response.</returns>
    public static PublicRiddlePlayResponse FromCorePublicRiddlePlayOutput(PublicRiddlePlayOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        return new PublicRiddlePlayResponse
        {
            Id = output.Id,
            PublicationDate = output.PublicationDate,
            Clue = output.Clue,
            AnswerPattern = output.AnswerPattern,
            Ranges = output.Ranges.Select(PublicRiddleRangeResponse.FromCorePublicRiddleRangeOutput).ToArray()
        };
    }
}

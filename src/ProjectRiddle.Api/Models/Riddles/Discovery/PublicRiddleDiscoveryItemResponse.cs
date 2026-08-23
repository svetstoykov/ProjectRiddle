using ProjectRiddle.Core.Models.Riddles.Discovery;

namespace ProjectRiddle.Api.Models.Riddles.Discovery;

/// <summary>
/// Represents a safe public discovery item.
/// </summary>
public sealed record PublicRiddleDiscoveryItemResponse
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
    /// Gets the bounded clue excerpt, or <see langword="null" /> when the caller cannot open the riddle.
    /// </summary>
    public required string? ClueExcerpt { get; init; }

    /// <summary>
    /// Gets the public answer pattern.
    /// </summary>
    public required string AnswerPattern { get; init; }

    /// <summary>
    /// Maps a Core discovery item to the API response.
    /// </summary>
    /// <param name="output">The Core output. Cannot be <see langword="null" />.</param>
    /// <returns>The corresponding API response.</returns>
    public static PublicRiddleDiscoveryItemResponse FromCorePublicRiddleDiscoveryItemOutput(
        PublicRiddleDiscoveryItemOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        return new PublicRiddleDiscoveryItemResponse
        {
            Id = output.Id,
            PublicationDate = output.PublicationDate,
            ClueExcerpt = output.ClueExcerpt,
            AnswerPattern = output.AnswerPattern
        };
    }
}

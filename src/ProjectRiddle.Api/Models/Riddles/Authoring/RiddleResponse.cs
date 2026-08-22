using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Models.Riddles.Authoring;

namespace ProjectRiddle.Api.Models.Riddles.Authoring;

/// <summary>
/// Represents the administrative riddle response, including answer-sensitive fields.
/// </summary>
public sealed record RiddleResponse
{
    /// <summary>
    /// Gets the stable riddle identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the clue text.
    /// </summary>
    public required string Clue { get; init; }

    /// <summary>
    /// Gets the stored answer text.
    /// </summary>
    public required string Answer { get; init; }

    /// <summary>
    /// Gets the stored answer pattern.
    /// </summary>
    public required string AnswerPattern { get; init; }

    /// <summary>
    /// Gets the stored explanation.
    /// </summary>
    public required string Explanation { get; init; }

    /// <summary>
    /// Gets the current publication state.
    /// </summary>
    public required RiddlePublicationState PublicationState { get; init; }

    /// <summary>
    /// Gets the Sofia calendar date when the riddle occupies or occupied the calendar.
    /// </summary>
    public DateOnly? SofiaPublicationDate { get; init; }

    /// <summary>
    /// Gets the labelled structural ranges.
    /// </summary>
    public required IReadOnlyList<RiddleRangeResponse> Ranges { get; init; }

    /// <summary>
    /// Gets the UTC timestamp when the riddle was created.
    /// </summary>
    public required DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>
    /// Gets the UTC timestamp when the riddle was last changed.
    /// </summary>
    public required DateTimeOffset UpdatedAtUtc { get; init; }

    /// <summary>
    /// Maps a Core riddle output to the API response.
    /// </summary>
    /// <param name="output">The Core output. Cannot be <see langword="null" />.</param>
    /// <returns>The corresponding API response.</returns>
    public static RiddleResponse FromCoreRiddleOutput(RiddleOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);

        return new RiddleResponse
        {
            Id = output.Id,
            Clue = output.Clue,
            Answer = output.Answer,
            AnswerPattern = output.AnswerPattern,
            Explanation = output.Explanation,
            PublicationState = output.PublicationState,
            SofiaPublicationDate = output.SofiaPublicationDate,
            Ranges = output.Ranges.Select(RiddleRangeResponse.FromCoreRiddleRangeOutput).ToArray(),
            CreatedAtUtc = output.CreatedAtUtc,
            UpdatedAtUtc = output.UpdatedAtUtc
        };
    }
}

using ProjectRiddle.Core.Models.Riddles.Authoring;

namespace ProjectRiddle.Api.Models.Riddles.Authoring;

/// <summary>
/// Represents a request to publish a riddle.
/// </summary>
public sealed record PublishRiddleRequest
{
    /// <summary>
    /// Gets the Sofia calendar date to publish onto when the riddle does not already have one.
    /// </summary>
    public DateOnly? PublicationDate { get; init; }

    /// <summary>
    /// Maps the request to a Core publish input.
    /// </summary>
    /// <param name="id">The riddle identifier from the route.</param>
    /// <returns>The corresponding Core input.</returns>
    public PublishRiddleInput ToCorePublishRiddleInput(Guid id)
    {
        return new PublishRiddleInput(id, PublicationDate);
    }
}

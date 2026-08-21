using ProjectRiddle.Core.Models.Riddles;

namespace ProjectRiddle.Api.Models.Riddles;

/// <summary>
/// Represents a request to reveal one previously unrevealed letter.
/// </summary>
public sealed record RevealRiddleLetterRequest
{
    /// <summary>
    /// Gets the optional anonymous progress snapshot.
    /// </summary>
    public AnonymousRiddleProgressRequest? Progress { get; init; }

    /// <summary>
    /// Maps the request to a Core reveal input.
    /// </summary>
    /// <param name="riddleId">The riddle identifier from the route.</param>
    /// <returns>The corresponding Core input.</returns>
    public RevealRiddleLetterInput ToCoreRevealRiddleLetterInput(Guid riddleId)
    {
        return new RevealRiddleLetterInput(riddleId, Progress?.ToCoreAnonymousRiddleProgressInput());
    }
}

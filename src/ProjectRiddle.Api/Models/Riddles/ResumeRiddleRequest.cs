using ProjectRiddle.Core.Models.Riddles;

namespace ProjectRiddle.Api.Models.Riddles;

/// <summary>
/// Represents a request to rehydrate play state.
/// </summary>
public sealed record ResumeRiddleRequest
{
    /// <summary>
    /// Gets the optional anonymous progress snapshot.
    /// </summary>
    public AnonymousRiddleProgressRequest? Progress { get; init; }

    /// <summary>
    /// Maps the request to a Core resume input.
    /// </summary>
    /// <param name="riddleId">The riddle identifier from the route.</param>
    /// <returns>The corresponding Core input.</returns>
    public ResumeRiddleInput ToCoreResumeRiddleInput(Guid riddleId)
    {
        return new ResumeRiddleInput(riddleId, Progress?.ToCoreAnonymousRiddleProgressInput());
    }
}

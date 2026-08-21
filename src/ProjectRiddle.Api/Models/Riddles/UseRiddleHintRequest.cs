using System.ComponentModel.DataAnnotations;
using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Models.Riddles;

namespace ProjectRiddle.Api.Models.Riddles;

/// <summary>
/// Represents a request to record one structural hint kind.
/// </summary>
public sealed record UseRiddleHintRequest
{
    /// <summary>
    /// Gets the structural hint kind.
    /// </summary>
    [Required]
    public required RiddleRangeKind Kind { get; init; }

    /// <summary>
    /// Gets the optional anonymous progress snapshot.
    /// </summary>
    public AnonymousRiddleProgressRequest? Progress { get; init; }

    /// <summary>
    /// Maps the request to a Core hint input.
    /// </summary>
    /// <param name="riddleId">The riddle identifier from the route.</param>
    /// <returns>The corresponding Core input.</returns>
    public UseRiddleHintInput ToCoreUseRiddleHintInput(Guid riddleId)
    {
        return new UseRiddleHintInput(riddleId, Kind, Progress?.ToCoreAnonymousRiddleProgressInput());
    }
}

using System.ComponentModel.DataAnnotations;
using ProjectRiddle.Core.Models.Riddles;

namespace ProjectRiddle.Api.Models.Riddles;

/// <summary>
/// Represents a submitted public riddle answer.
/// </summary>
public sealed record SubmitRiddleAnswerRequest
{
    /// <summary>
    /// Gets the submitted answer.
    /// </summary>
    [Required]
    public required string Answer { get; init; }

    /// <summary>
    /// Gets the optional anonymous progress snapshot.
    /// </summary>
    public AnonymousRiddleProgressRequest? Progress { get; init; }

    /// <summary>
    /// Maps the request to a Core answer input.
    /// </summary>
    /// <param name="riddleId">The riddle identifier from the route.</param>
    /// <returns>The corresponding Core input.</returns>
    public SubmitRiddleAnswerInput ToCoreSubmitRiddleAnswerInput(Guid riddleId)
    {
        return new SubmitRiddleAnswerInput(
            riddleId,
            Answer,
            Progress?.ToCoreAnonymousRiddleProgressInput());
    }
}

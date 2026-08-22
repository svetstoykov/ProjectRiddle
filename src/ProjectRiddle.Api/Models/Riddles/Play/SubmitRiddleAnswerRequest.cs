using System.ComponentModel.DataAnnotations;
using ProjectRiddle.Api.Models.Riddles.Progress;
using ProjectRiddle.Core.Models.Riddles.Play;

namespace ProjectRiddle.Api.Models.Riddles.Play;

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

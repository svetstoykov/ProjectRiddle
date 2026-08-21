using System.ComponentModel.DataAnnotations;
using ProjectRiddle.Core.Models.Riddles;

namespace ProjectRiddle.Api.Models.Riddles;

/// <summary>
/// Represents a request to schedule a riddle.
/// </summary>
public sealed record ScheduleRiddleRequest
{
    /// <summary>
    /// Gets the Sofia calendar date to occupy.
    /// </summary>
    [Required]
    public required DateOnly PublicationDate { get; init; }

    /// <summary>
    /// Maps the request to a Core schedule input.
    /// </summary>
    /// <param name="id">The riddle identifier from the route.</param>
    /// <returns>The corresponding Core input.</returns>
    public ScheduleRiddleInput ToCoreScheduleRiddleInput(Guid id)
    {
        return new ScheduleRiddleInput(id, PublicationDate);
    }
}

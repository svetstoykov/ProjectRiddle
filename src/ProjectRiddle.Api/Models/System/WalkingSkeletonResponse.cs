using ProjectRiddle.Core.Models.Diagnostics;

namespace ProjectRiddle.Api.Models.System;

/// <summary>
/// Represents the public response from the Phase 0 walking-skeleton endpoint.
/// </summary>
public sealed class WalkingSkeletonResponse
{
    /// <summary>
    /// Initializes a walking-skeleton response.
    /// </summary>
    /// <param name="message">The safe readiness message.</param>
    /// <param name="publicationDate">The current publication date.</param>
    public WalkingSkeletonResponse(string message, DateOnly publicationDate)
    {
        Message = message;
        PublicationDate = publicationDate;
    }

    /// <summary>
    /// Gets the safe readiness message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the current publication date.
    /// </summary>
    public DateOnly PublicationDate { get; }

    /// <summary>
    /// Maps a Core output to the public response contract.
    /// </summary>
    /// <param name="output">The Core output to map. Cannot be <see langword="null" />.</param>
    /// <returns>The corresponding API response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="output" /> is <see langword="null" />.</exception>
    public static WalkingSkeletonResponse FromCoreOutput(WalkingSkeletonOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        return new WalkingSkeletonResponse(output.Message, output.PublicationDate);
    }
}

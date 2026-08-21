using ProjectRiddle.Core.Models.Diagnostics;

namespace ProjectRiddle.Api.Models.System;

/// <summary>
/// Represents the query input for the Phase 0 walking-skeleton endpoint.
/// </summary>
public sealed class WalkingSkeletonRequest
{
    /// <summary>
    /// Gets or sets a value indicating whether the endpoint should return its deterministic sample failure.
    /// </summary>
    public bool Fail { get; set; }

    /// <summary>
    /// Maps the API request to its Core operation input.
    /// </summary>
    /// <returns>The corresponding Core input.</returns>
    public WalkingSkeletonInput ToCoreInput() => new(Fail);
}

namespace ProjectRiddle.Core.Models.Diagnostics;

/// <summary>
/// Represents input for the Phase 0 walking-skeleton operation.
/// </summary>
public sealed record WalkingSkeletonInput
{
    /// <summary>
    /// Initializes walking-skeleton input.
    /// </summary>
    /// <param name="shouldFail">A value indicating whether the operation should return its deterministic sample failure.</param>
    public WalkingSkeletonInput(bool shouldFail)
    {
        ShouldFail = shouldFail;
    }

    /// <summary>
    /// Gets a value indicating whether the operation should return its deterministic sample failure.
    /// </summary>
    public bool ShouldFail { get; }
}

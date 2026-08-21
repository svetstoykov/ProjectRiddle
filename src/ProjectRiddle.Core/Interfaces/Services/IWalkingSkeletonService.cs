using ProjectRiddle.Core.Models.Diagnostics;
using ProjectRiddle.Core.Results.Models;

namespace ProjectRiddle.Core.Interfaces.Services;

/// <summary>
/// Provides the trivial operation used to verify the Phase 0 application boundary.
/// </summary>
public interface IWalkingSkeletonService
{
    /// <summary>
    /// Executes the walking-skeleton operation.
    /// </summary>
    /// <param name="input">The operation input.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A result containing the current publication date or an expected failure.</returns>
    Task<Result<WalkingSkeletonOutput>> ExecuteAsync(
        WalkingSkeletonInput input,
        CancellationToken cancellationToken);
}

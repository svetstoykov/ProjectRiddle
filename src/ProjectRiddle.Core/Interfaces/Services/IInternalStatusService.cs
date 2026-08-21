using ProjectRiddle.Core.Models.System;
using ProjectRiddle.Core.Results.Models;

namespace ProjectRiddle.Core.Interfaces.Services;

/// <summary>
/// Provides the current internal application status.
/// </summary>
public interface IInternalStatusService
{
    /// <summary>
    /// Gets the current internal application status.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A result containing the current internal status.</returns>
    Task<Result<InternalStatusOutput>> GetAsync(CancellationToken cancellationToken);
}

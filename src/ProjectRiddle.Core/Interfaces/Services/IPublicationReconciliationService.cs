using ProjectRiddle.Core.Models.Riddles.Publication;
using ProjectRiddle.Core.Results.Models;

namespace ProjectRiddle.Core.Interfaces.Services;

/// <summary>
/// Publishes the latest due schedule and expires older due schedules.
/// </summary>
public interface IPublicationReconciliationService
{
    /// <summary>
    /// Reconciles every daily riddle still scheduled on or before the current configured-local date.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// The published and expired identifiers, or an expected failure when the batch cannot be stored. A cycle with
    /// nothing due is a success.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The latest due date is published without changing that date. Every older due schedule becomes expired and
    /// keeps its date. The batch is atomic. Repeating the operation does nothing once those rows have left the
    /// scheduled state.
    /// </para>
    /// <para>
    /// A stale write is reloaded and retried. The original editorial dates are never rewritten.
    /// </para>
    /// </remarks>
    Task<Result<PublicationReconciliationOutput>> ReconcileAsync(CancellationToken cancellationToken);
}

using ProjectRiddle.Core.Models.Riddles.Progress;

namespace ProjectRiddle.Core.Interfaces.Repositories;

/// <summary>
/// Persists account-owned riddle progress without exposing storage types to Core.
/// </summary>
public interface IRiddleProgressRepository
{
    /// <summary>
    /// Gets the progress record for an account and riddle.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="riddleId">The riddle identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The progress record when it exists; otherwise <see langword="null" />.</returns>
    Task<RiddleProgress?> GetAsync(Guid accountId, Guid riddleId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists account progress whose riddle publication date is in the inclusive local range.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="fromDate">The inclusive start date.</param>
    /// <param name="toDate">The inclusive end date.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The matching progress records.</returns>
    Task<IReadOnlyList<RiddleProgress>> ListByAccountAndPublicationDateRangeAsync(
        Guid accountId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken);

    /// <summary>
    /// Lists account progress for the supplied riddles.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="riddleIds">The riddle identifiers. Cannot be <see langword="null" />.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The matching progress records.</returns>
    /// <remarks>
    /// Course completion is derived from these rows rather than from a course progress table, so this read is how
    /// Courses learns what a learner has finished without recording the same fact twice.
    /// </remarks>
    Task<IReadOnlyList<RiddleProgress>> ListByAccountAndRiddleIdsAsync(
        Guid accountId,
        IReadOnlyCollection<Guid> riddleIds,
        CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new progress record and saves the change.
    /// </summary>
    /// <param name="progress">The progress record to add. Cannot be <see langword="null" />.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that represents the save operation.</returns>
    Task AddAsync(RiddleProgress progress, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing progress record and saves the change.
    /// </summary>
    /// <param name="progress">The progress record to update. Cannot be <see langword="null" />.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that represents the save operation.</returns>
    Task UpdateAsync(RiddleProgress progress, CancellationToken cancellationToken);
}

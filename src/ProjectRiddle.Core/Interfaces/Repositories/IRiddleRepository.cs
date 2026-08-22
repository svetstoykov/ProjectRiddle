using ProjectRiddle.Core.Models.Riddles;

namespace ProjectRiddle.Core.Interfaces.Repositories;

/// <summary>
/// Persists riddles without exposing storage types to Core.
/// </summary>
public interface IRiddleRepository
{
    /// <summary>
    /// Gets the riddle with the supplied identifier.
    /// </summary>
    /// <param name="id">The riddle identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The riddle when it exists; otherwise <see langword="null" />.</returns>
    Task<Riddle?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Gets riddles with the supplied identifiers.
    /// </summary>
    /// <param name="ids">The riddle identifiers. Cannot be <see langword="null" />.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The matching riddles.</returns>
    Task<IReadOnlyList<Riddle>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);

    /// <summary>
    /// Lists every stored riddle.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The stored riddles.</returns>
    Task<IReadOnlyList<Riddle>> ListAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the scheduled or published riddle occupying the supplied Sofia date.
    /// </summary>
    /// <param name="publicationDate">The Sofia calendar date to inspect.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The occupying riddle when one exists; otherwise <see langword="null" />.</returns>
    Task<Riddle?> GetOccupyingByPublicationDateAsync(DateOnly publicationDate, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the published riddle occupying the supplied local publication date.
    /// </summary>
    /// <param name="publicationDate">The local publication date.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The published riddle when one exists; otherwise <see langword="null" />.</returns>
    Task<Riddle?> GetPublishedByPublicationDateAsync(DateOnly publicationDate, CancellationToken cancellationToken);

    /// <summary>
    /// Lists published riddles whose local publication date is in the inclusive range.
    /// </summary>
    /// <param name="fromDate">The inclusive start date.</param>
    /// <param name="toDate">The inclusive end date.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The matching published riddles.</returns>
    Task<IReadOnlyList<Riddle>> ListPublishedBetweenAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken);

    /// <summary>
    /// Lists a page of published archive riddles whose local publication date is before <paramref name="beforeDate" />.
    /// </summary>
    /// <param name="beforeDate">The exclusive local date upper bound.</param>
    /// <param name="skip">The number of records to skip. Cannot be negative.</param>
    /// <param name="take">The number of records to take. Must be greater than zero.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The archive page ordered by publication date descending.</returns>
    Task<IReadOnlyList<Riddle>> ListPublishedArchivePageAsync(
        DateOnly beforeDate,
        int skip,
        int take,
        CancellationToken cancellationToken);

    /// <summary>
    /// Counts published archive riddles whose local publication date is before <paramref name="beforeDate" />.
    /// </summary>
    /// <param name="beforeDate">The exclusive local date upper bound.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The archive count.</returns>
    Task<int> CountPublishedArchiveAsync(DateOnly beforeDate, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new riddle and saves the change.
    /// </summary>
    /// <param name="riddle">The riddle to add. Cannot be <see langword="null" />.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that represents the save operation.</returns>
    Task AddAsync(Riddle riddle, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing riddle and saves the change.
    /// </summary>
    /// <param name="riddle">The riddle to update. Cannot be <see langword="null" />.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that represents the save operation.</returns>
    Task UpdateAsync(Riddle riddle, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a riddle and saves the change.
    /// </summary>
    /// <param name="riddle">The riddle to delete. Cannot be <see langword="null" />.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that represents the save operation.</returns>
    Task DeleteAsync(Riddle riddle, CancellationToken cancellationToken);
}

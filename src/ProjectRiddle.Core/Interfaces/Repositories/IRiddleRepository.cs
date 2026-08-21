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

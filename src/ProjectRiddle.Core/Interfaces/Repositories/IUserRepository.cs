using ProjectRiddle.Core.Models.Users;

namespace ProjectRiddle.Core.Interfaces.Repositories;

/// <summary>
/// Persists local accounts without exposing storage types to Core.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets the account with the supplied identifier.
    /// </summary>
    /// <param name="id">The account identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The account when it exists; otherwise <see langword="null" />.</returns>
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the account with the supplied normalized email.
    /// </summary>
    /// <param name="normalizedEmail">The normalized email. Cannot be <see langword="null" /> or whitespace.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The account when it exists; otherwise <see langword="null" />.</returns>
    Task<User?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new account and saves the change.
    /// </summary>
    /// <param name="user">The account to add. Cannot be <see langword="null" />.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that represents the save operation.</returns>
    Task AddAsync(User user, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing account and saves the change.
    /// </summary>
    /// <param name="user">The account to update. Cannot be <see langword="null" />.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that represents the save operation.</returns>
    Task UpdateAsync(User user, CancellationToken cancellationToken);
}

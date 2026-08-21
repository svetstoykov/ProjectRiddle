using ProjectRiddle.Core.Models.Users;
using ProjectRiddle.Core.Results.Models;

namespace ProjectRiddle.Core.Interfaces.Services;

/// <summary>
/// Provides registration, credential verification, and current-session lookup for local accounts.
/// </summary>
public interface IUsersService
{
    /// <summary>
    /// Registers a local account with the <c>user</c> role.
    /// </summary>
    /// <param name="input">The registration input. Cannot be <see langword="null" />.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The created account, or an expected failure.</returns>
    Task<Result<RegisterUserOutput>> RegisterAsync(RegisterUserInput input, CancellationToken cancellationToken);

    /// <summary>
    /// Verifies local credentials without disclosing whether the email exists.
    /// </summary>
    /// <param name="input">The credential input. Cannot be <see langword="null" />.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The matching account, or an expected failure.</returns>
    Task<Result<AuthenticateUserOutput>> AuthenticateAsync(
        AuthenticateUserInput input,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets the account for the current authenticated caller.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The current account, or an expected failure.</returns>
    Task<Result<CurrentSessionOutput>> GetCurrentAsync(CancellationToken cancellationToken);
}

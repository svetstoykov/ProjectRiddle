namespace ProjectRiddle.Core.Interfaces.Users;

/// <summary>
/// Hashes and verifies account passwords without exposing a hashing-provider type to Core.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Creates a one-way hash for the supplied plaintext password.
    /// </summary>
    /// <param name="password">The plaintext password. Cannot be <see langword="null" /> or empty.</param>
    /// <returns>The stored password hash.</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a plaintext password against a stored hash.
    /// </summary>
    /// <param name="hashedPassword">The stored password hash. Cannot be <see langword="null" /> or empty.</param>
    /// <param name="providedPassword">The plaintext password to verify. Cannot be <see langword="null" /> or empty.</param>
    /// <returns><see langword="true" /> when the password matches the hash; otherwise <see langword="false" />.</returns>
    bool VerifyHashedPassword(string hashedPassword, string providedPassword);
}

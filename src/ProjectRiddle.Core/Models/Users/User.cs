using ProjectRiddle.Core.Enums.Users;

namespace ProjectRiddle.Core.Models.Users;

/// <summary>
/// Represents a persisted local account.
/// </summary>
public sealed class User
{
    /// <summary>
    /// Initializes a local account.
    /// </summary>
    /// <param name="id">The stable account identifier. Cannot be <see cref="Guid.Empty" />.</param>
    /// <param name="email">The display email address. Cannot be <see langword="null" /> or whitespace.</param>
    /// <param name="normalizedEmail">The normalized email used for uniqueness. Cannot be <see langword="null" /> or whitespace.</param>
    /// <param name="passwordHash">The one-way password hash. Cannot be <see langword="null" /> or whitespace.</param>
    /// <param name="role">The assigned role.</param>
    /// <param name="createdAtUtc">The UTC timestamp when the account was created.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="id" /> is empty.</exception>
    /// <exception cref="ArgumentException">Thrown when a required string argument is empty or whitespace.</exception>
    public User(
        Guid id,
        string email,
        string normalizedEmail,
        string passwordHash,
        UserRole role,
        DateTimeOffset createdAtUtc)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(id, Guid.Empty);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        Id = id;
        Email = email;
        NormalizedEmail = normalizedEmail;
        PasswordHash = passwordHash;
        Role = role;
        CreatedAtUtc = createdAtUtc;
    }

    /// <summary>
    /// Gets the stable account identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the display email address.
    /// </summary>
    public string Email { get; }

    /// <summary>
    /// Gets the normalized email used for case-insensitive uniqueness.
    /// </summary>
    public string NormalizedEmail { get; }

    /// <summary>
    /// Gets the one-way password hash.
    /// </summary>
    public string PasswordHash { get; private set; }

    /// <summary>
    /// Gets the assigned role.
    /// </summary>
    public UserRole Role { get; }

    /// <summary>
    /// Gets the UTC timestamp when the account was created.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; }

    /// <summary>
    /// Replaces the stored password hash after a successful verification that requested a rehash.
    /// </summary>
    /// <param name="passwordHash">The replacement hash. Cannot be <see langword="null" /> or whitespace.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="passwordHash" /> is empty or whitespace.</exception>
    public void ReplacePasswordHash(string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        PasswordHash = passwordHash;
    }
}

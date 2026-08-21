namespace ProjectRiddle.Core.Services.Users;

/// <summary>
/// Normalizes email addresses for case-insensitive uniqueness.
/// </summary>
public static class EmailNormalizer
{
    /// <summary>
    /// Trims and lowercases an email address using the invariant culture.
    /// </summary>
    /// <param name="email">The email address to normalize. Cannot be <see langword="null" />.</param>
    /// <returns>The normalized email address.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="email" /> is <see langword="null" />.</exception>
    public static string Normalize(string email)
    {
        ArgumentNullException.ThrowIfNull(email);
        return email.Trim().ToLowerInvariant();
    }
}

namespace ProjectRiddle.Core.Exceptions;

/// <summary>
/// Represents a persistence conflict on the normalized-email uniqueness constraint.
/// </summary>
public sealed class DuplicateNormalizedEmailException : Exception
{
    /// <summary>
    /// Initializes the exception.
    /// </summary>
    /// <param name="normalizedEmail">The normalized email that already exists. Cannot be <see langword="null" /> or whitespace.</param>
    public DuplicateNormalizedEmailException(string normalizedEmail)
        : base("A user with the same normalized email already exists.")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedEmail);
        NormalizedEmail = normalizedEmail;
    }

    /// <summary>
    /// Gets the normalized email that already exists.
    /// </summary>
    public string NormalizedEmail { get; }
}

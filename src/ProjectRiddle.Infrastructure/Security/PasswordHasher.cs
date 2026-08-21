using Microsoft.AspNetCore.Identity;
using ProjectRiddle.Core.Interfaces.Users;

namespace ProjectRiddle.Infrastructure.Security;

/// <summary>
/// Hashes and verifies passwords using the ASP.NET Identity one-way hasher.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> hasher = new();

    /// <inheritdoc />
    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);
        return hasher.HashPassword(new object(), password);
    }

    /// <inheritdoc />
    public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
    {
        ArgumentException.ThrowIfNullOrEmpty(hashedPassword);
        ArgumentException.ThrowIfNullOrEmpty(providedPassword);

        var result = hasher.VerifyHashedPassword(new object(), hashedPassword, providedPassword);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}

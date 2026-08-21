namespace ProjectRiddle.Core.Models.Users;

/// <summary>
/// Represents the input required to verify local credentials.
/// </summary>
/// <param name="Email">The email address supplied by the visitor. Cannot be <see langword="null" />.</param>
/// <param name="Password">The plaintext password supplied by the visitor. Cannot be <see langword="null" />.</param>
public sealed record AuthenticateUserInput(string Email, string Password);

using System.ComponentModel.DataAnnotations;
using ProjectRiddle.Core.Models.Users;

namespace ProjectRiddle.Api.Models.Auth;

/// <summary>
/// Represents a sign-in request.
/// </summary>
public sealed record SignInRequest
{
    /// <summary>
    /// Gets the email address supplied by the visitor.
    /// </summary>
    [Required]
    public required string Email { get; init; }

    /// <summary>
    /// Gets the plaintext password supplied by the visitor.
    /// </summary>
    [Required]
    public required string Password { get; init; }

    /// <summary>
    /// Maps the request to a Core authentication input.
    /// </summary>
    /// <returns>The corresponding Core input.</returns>
    public AuthenticateUserInput ToCoreAuthenticateUserInput()
    {
        return new AuthenticateUserInput(Email, Password);
    }
}

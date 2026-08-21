using System.ComponentModel.DataAnnotations;
using ProjectRiddle.Core.Models.Users;

namespace ProjectRiddle.Api.Models.Auth;

/// <summary>
/// Represents a registration request.
/// </summary>
public sealed record RegisterRequest
{
    /// <summary>
    /// Gets the email address supplied by the visitor.
    /// </summary>
    [Required]
    [MaxLength(256)]
    public required string Email { get; init; }

    /// <summary>
    /// Gets the plaintext password supplied by the visitor.
    /// </summary>
    [Required]
    [MinLength(8)]
    [MaxLength(256)]
    public required string Password { get; init; }

    /// <summary>
    /// Maps the request to a Core registration input.
    /// </summary>
    /// <returns>The corresponding Core input.</returns>
    public RegisterUserInput ToCoreRegisterUserInput()
    {
        return new RegisterUserInput(Email, Password);
    }
}

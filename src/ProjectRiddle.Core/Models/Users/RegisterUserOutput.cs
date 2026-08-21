using ProjectRiddle.Core.Enums.Users;

namespace ProjectRiddle.Core.Models.Users;

/// <summary>
/// Represents the account created by a successful registration.
/// </summary>
/// <param name="Id">The stable account identifier.</param>
/// <param name="Email">The stored display email address.</param>
/// <param name="Role">The assigned role. Self-registration always produces <see cref="UserRole.User" />.</param>
public sealed record RegisterUserOutput(Guid Id, string Email, UserRole Role);

using ProjectRiddle.Core.Enums.Users;

namespace ProjectRiddle.Core.Models.Users;

/// <summary>
/// Represents the account returned by successful credential verification.
/// </summary>
/// <param name="Id">The stable account identifier.</param>
/// <param name="Email">The stored display email address.</param>
/// <param name="Role">The assigned role.</param>
public sealed record AuthenticateUserOutput(Guid Id, string Email, UserRole Role);

namespace ProjectRiddle.Core.Services.Users;

/// <summary>
/// Provides stable codes for expected Users capability failures.
/// </summary>
public static class UserErrorCodes
{
    /// <summary>
    /// Identifies an email that is missing or not a valid address.
    /// </summary>
    public const string EmailInvalid = "users.email.invalid";

    /// <summary>
    /// Identifies a normalized email that is already registered.
    /// </summary>
    public const string EmailConflict = "users.email.conflict";

    /// <summary>
    /// Identifies a password that does not meet the local-account rules.
    /// </summary>
    public const string PasswordInvalid = "users.password.invalid";

    /// <summary>
    /// Identifies credential verification that failed without disclosing account existence.
    /// </summary>
    public const string CredentialsInvalid = "users.credentials.invalid";

    /// <summary>
    /// Identifies a missing authenticated caller.
    /// </summary>
    public const string Unauthorized = "users.unauthorized";
}

namespace ProjectRiddle.Infrastructure.Configuration;

/// <summary>
/// Defines the runtime-only settings used to provision the first administrator.
/// </summary>
public sealed class AdminBootstrapOptions
{
    /// <summary>
    /// Identifies the configuration section containing administrator bootstrap settings.
    /// </summary>
    public const string SectionName = "AdminBootstrap";

    /// <summary>
    /// Gets or sets the email address of the bootstrap administrator.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the plaintext password of the bootstrap administrator.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

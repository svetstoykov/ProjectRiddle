namespace ProjectRiddle.Api.Configuration;

/// <summary>
/// Defines the production certificate used to encrypt persisted Data Protection keys.
/// </summary>
public sealed class DataProtectionCertificateOptions
{
    /// <summary>
    /// Identifies the configuration section containing Data Protection certificate settings.
    /// </summary>
    public const string SectionName = "DataProtection";

    /// <summary>
    /// Gets or sets the Base64-encoded PKCS#12 certificate containing its private key.
    /// </summary>
    public string CertificateBase64 { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password that protects the PKCS#12 certificate.
    /// </summary>
    public string CertificatePassword { get; set; } = string.Empty;
}

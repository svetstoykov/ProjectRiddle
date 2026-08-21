using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.DataProtection;
using ProjectRiddle.Api.Configuration;
using ProjectRiddle.Infrastructure.Configuration;

namespace ProjectRiddle.Api.Extensions;

/// <summary>
/// Registers ASP.NET Data Protection at the composition boundary.
/// </summary>
public static class DataProtectionServiceCollectionExtensions
{
    /// <summary>
    /// Adds Data Protection with an ephemeral provider outside Production and certificate-protected keys in Production.
    /// </summary>
    /// <param name="services">The service collection to configure. Cannot be <see langword="null" />.</param>
    /// <param name="configuration">The application configuration. Cannot be <see langword="null" />.</param>
    /// <param name="environment">The host environment. Cannot be <see langword="null" />.</param>
    /// <returns>The supplied service collection.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services" />, <paramref name="configuration" />, or <paramref name="environment" /> is
    /// <see langword="null" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown in Production when the Data Protection certificate or database path is missing or invalid.
    /// </exception>
    public static IServiceCollection AddProjectRiddleDataProtection(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        var dataProtection = services.AddDataProtection()
            .SetApplicationName("ProjectRiddle");

        if (!environment.IsProduction())
        {
            dataProtection.UseEphemeralDataProtectionProvider();
            return services;
        }

        var certificateOptions = configuration
            .GetSection(DataProtectionCertificateOptions.SectionName)
            .Get<DataProtectionCertificateOptions>() ?? new DataProtectionCertificateOptions();
        var certificate = LoadDataProtectionCertificate(certificateOptions);
        var databaseOptions = configuration
            .GetSection(DatabaseOptions.SectionName)
            .Get<DatabaseOptions>() ?? new DatabaseOptions();

        if (string.IsNullOrWhiteSpace(databaseOptions.DatabasePath))
        {
            throw new InvalidOperationException(
                $"Configuration value '{DatabaseOptions.SectionName}:{nameof(DatabaseOptions.DatabasePath)}' is required in Production.");
        }

        var databasePath = Path.GetFullPath(databaseOptions.DatabasePath, environment.ContentRootPath);
        var databaseDirectory = Path.GetDirectoryName(databasePath) ?? environment.ContentRootPath;
        var keysPath = Path.Combine(databaseDirectory, "keys");
        Directory.CreateDirectory(keysPath);

        dataProtection
            .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
            .ProtectKeysWithCertificate(certificate);

        return services;
    }

    /// <summary>
    /// Loads the PKCS#12 certificate used to encrypt persisted Data Protection keys.
    /// </summary>
    /// <param name="options">The certificate configuration. Cannot be <see langword="null" />.</param>
    /// <returns>The loaded certificate that contains an RSA private key.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the certificate payload or password is missing, the payload is not valid Base64 or PKCS#12 data, or
    /// the certificate does not contain an RSA private key.
    /// </exception>
    private static X509Certificate2 LoadDataProtectionCertificate(DataProtectionCertificateOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.CertificateBase64))
        {
            var configurationKey =
                $"{DataProtectionCertificateOptions.SectionName}:{nameof(options.CertificateBase64)}";
            throw new InvalidOperationException(
                $"Configuration value '{configurationKey}' is required in Production.");
        }

        if (string.IsNullOrEmpty(options.CertificatePassword))
        {
            var configurationKey =
                $"{DataProtectionCertificateOptions.SectionName}:{nameof(options.CertificatePassword)}";
            throw new InvalidOperationException(
                $"Configuration value '{configurationKey}' is required in Production.");
        }

        try
        {
            var certificateBytes = Convert.FromBase64String(options.CertificateBase64);
            var certificate = X509CertificateLoader.LoadPkcs12(
                certificateBytes,
                options.CertificatePassword,
                X509KeyStorageFlags.DefaultKeySet);

            using var rsaPublicKey = certificate.GetRSAPublicKey();
            if (!certificate.HasPrivateKey || rsaPublicKey is null)
            {
                certificate.Dispose();
                throw new InvalidOperationException(
                    "The configured Data Protection certificate must contain an RSA private key.");
            }

            return certificate;
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException(
                "The configured Data Protection certificate is not valid Base64.",
                exception);
        }
        catch (CryptographicException exception)
        {
            throw new InvalidOperationException(
                "The configured Data Protection certificate could not be loaded as PKCS#12 data.",
                exception);
        }
    }
}

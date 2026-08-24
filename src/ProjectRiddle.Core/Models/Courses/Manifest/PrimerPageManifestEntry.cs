namespace ProjectRiddle.Core.Models.Courses.Manifest;

/// <summary>
/// Represents one authored primer page in the shipped manifest.
/// </summary>
/// <param name="Ordinal">The one-based page position.</param>
/// <param name="Title">The page title.</param>
/// <param name="Body">The page prose.</param>
/// <param name="Figure">The optional figure key the frontend resolves to a diagram.</param>
public sealed record PrimerPageManifestEntry(int Ordinal, string? Title, string? Body, string? Figure);

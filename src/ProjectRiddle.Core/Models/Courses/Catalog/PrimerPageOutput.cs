namespace ProjectRiddle.Core.Models.Courses.Catalog;

/// <summary>
/// Represents one primer page.
/// </summary>
/// <param name="Ordinal">The one-based page position.</param>
/// <param name="Title">The page title.</param>
/// <param name="Body">The page prose.</param>
/// <param name="Figure">The optional figure key the frontend resolves to a diagram.</param>
public sealed record PrimerPageOutput(int Ordinal, string Title, string Body, string? Figure);

namespace ProjectRiddle.Core.Models.Courses;

/// <summary>
/// Represents one ordered page of the primer that introduces cryptic clue vocabulary.
/// </summary>
/// <remarks>
/// A primer page is identified by its ordinal rather than a manifest identifier, because nothing records progress
/// against a page. "Already seen" needs no storage: the primer opens when the learner has no completions at all.
/// </remarks>
public sealed class PrimerPage
{
    /// <summary>
    /// Initializes a primer page.
    /// </summary>
    /// <param name="ordinal">The one-based page position. Must be greater than zero.</param>
    /// <param name="title">The page title. Cannot be <see langword="null" /> or whitespace.</param>
    /// <param name="body">The page prose. Cannot be <see langword="null" /> or whitespace.</param>
    /// <param name="figure">The optional figure key the frontend resolves to a diagram.</param>
    /// <param name="isActive">A value indicating whether the page is still part of the shipped primer.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="ordinal" /> is not positive.</exception>
    /// <exception cref="ArgumentException">Thrown when a required string argument is empty or whitespace.</exception>
    public PrimerPage(int ordinal, string title, string body, string? figure, bool isActive)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ordinal);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        Ordinal = ordinal;
        Title = title;
        Body = body;
        Figure = figure;
        IsActive = isActive;
    }

    /// <summary>
    /// Gets the one-based page position.
    /// </summary>
    public int Ordinal { get; }

    /// <summary>
    /// Gets the page title.
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Gets the page prose.
    /// </summary>
    public string Body { get; private set; }

    /// <summary>
    /// Gets the optional figure key the frontend resolves to a diagram.
    /// </summary>
    public string? Figure { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the page is still part of the shipped primer.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Replaces the authored fields of the page and marks it active.
    /// </summary>
    /// <param name="title">The page title. Cannot be <see langword="null" /> or whitespace.</param>
    /// <param name="body">The page prose. Cannot be <see langword="null" /> or whitespace.</param>
    /// <param name="figure">The optional figure key.</param>
    /// <exception cref="ArgumentException">Thrown when a required string argument is empty or whitespace.</exception>
    public void ReplaceContent(string title, string body, string? figure)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        Title = title;
        Body = body;
        Figure = figure;
        IsActive = true;
    }

    /// <summary>
    /// Withdraws the page from the shipped primer without deleting it.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }
}

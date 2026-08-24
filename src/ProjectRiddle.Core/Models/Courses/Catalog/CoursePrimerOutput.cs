namespace ProjectRiddle.Core.Models.Courses.Catalog;

/// <summary>
/// Represents the ordered primer pages.
/// </summary>
/// <param name="Pages">The pages in ordinal order. Cannot be <see langword="null" />.</param>
public sealed record CoursePrimerOutput(IReadOnlyList<PrimerPageOutput> Pages);

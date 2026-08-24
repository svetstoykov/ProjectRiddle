namespace ProjectRiddle.Core.Models.Courses.Catalog;

/// <summary>
/// Represents the whole active curriculum as the catalog read returns it.
/// </summary>
/// <param name="Courses">The active courses in ordinal order. Cannot be <see langword="null" />.</param>
public sealed record CourseCatalogOutput(IReadOnlyList<CourseOutput> Courses);

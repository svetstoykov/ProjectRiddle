using ProjectRiddle.Core.Models.Courses.Catalog;

namespace ProjectRiddle.Api.Models.Courses.Catalog;

/// <summary>
/// Represents the active guided-course catalog.
/// </summary>
public sealed record CourseCatalogResponse
{
    /// <summary>
    /// Gets the active courses in ordinal order.
    /// </summary>
    public required IReadOnlyList<CourseResponse> Courses { get; init; }

    /// <summary>
    /// Maps a Core catalog to the API response.
    /// </summary>
    /// <param name="output">The Core output. Cannot be <see langword="null" />.</param>
    /// <returns>The corresponding API response.</returns>
    public static CourseCatalogResponse FromCoreCourseCatalogOutput(CourseCatalogOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        return new CourseCatalogResponse
        {
            Courses = output.Courses.Select(CourseResponse.FromCoreCourseOutput).ToArray()
        };
    }
}

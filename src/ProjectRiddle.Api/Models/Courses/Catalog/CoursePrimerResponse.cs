using ProjectRiddle.Core.Models.Courses.Catalog;

namespace ProjectRiddle.Api.Models.Courses.Catalog;

/// <summary>
/// Represents the ordered guided-course primer pages.
/// </summary>
public sealed record CoursePrimerResponse
{
    /// <summary>
    /// Gets the primer pages in ordinal order.
    /// </summary>
    public required IReadOnlyList<PrimerPageResponse> Pages { get; init; }

    /// <summary>
    /// Maps a Core primer to the API response.
    /// </summary>
    /// <param name="output">The Core output. Cannot be <see langword="null" />.</param>
    /// <returns>The corresponding API response.</returns>
    public static CoursePrimerResponse FromCoreCoursePrimerOutput(CoursePrimerOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        return new CoursePrimerResponse
        {
            Pages = output.Pages.Select(PrimerPageResponse.FromCorePrimerPageOutput).ToArray()
        };
    }
}

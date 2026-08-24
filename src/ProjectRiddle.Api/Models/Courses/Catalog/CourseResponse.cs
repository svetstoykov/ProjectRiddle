using ProjectRiddle.Core.Models.Courses.Catalog;

namespace ProjectRiddle.Api.Models.Courses.Catalog;

/// <summary>
/// Represents one guided course.
/// </summary>
public sealed record CourseResponse
{
    /// <summary>
    /// Gets the stable course identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the course key.
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Gets the one-based position within the curriculum.
    /// </summary>
    public required int Ordinal { get; init; }

    /// <summary>
    /// Gets the course title.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets the framing prose for the course page.
    /// </summary>
    public required string Intro { get; init; }

    /// <summary>
    /// Gets the active lessons in ordinal order.
    /// </summary>
    public required IReadOnlyList<LessonResponse> Lessons { get; init; }

    /// <summary>
    /// Maps a Core course to the API response.
    /// </summary>
    /// <param name="output">The Core output. Cannot be <see langword="null" />.</param>
    /// <returns>The corresponding API response.</returns>
    public static CourseResponse FromCoreCourseOutput(CourseOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        return new CourseResponse
        {
            Id = output.Id,
            Key = output.Key,
            Ordinal = output.Ordinal,
            Title = output.Title,
            Intro = output.Intro,
            Lessons = output.Lessons.Select(LessonResponse.FromCoreLessonOutput).ToArray()
        };
    }
}

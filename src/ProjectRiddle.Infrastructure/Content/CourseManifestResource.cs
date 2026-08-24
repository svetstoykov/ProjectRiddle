using System.Text.Json;
using ProjectRiddle.Core.Models.Courses.Manifest;

namespace ProjectRiddle.Infrastructure.Content;

/// <summary>
/// Reads the course manifest embedded in this assembly.
/// </summary>
/// <remarks>
/// The manifest ships inside the assembly rather than beside it so a deployment cannot start against a curriculum
/// that was edited or lost on disk.
/// </remarks>
internal static class CourseManifestResource
{
    private const string ResourceName = "ProjectRiddle.Infrastructure.Content.course-manifest.json";

    /// <summary>
    /// Reads and deserializes the embedded course manifest.
    /// </summary>
    /// <returns>The deserialized manifest.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the resource is missing or deserializes to null.</exception>
    /// <exception cref="JsonException">Thrown when the resource is not well-formed JSON.</exception>
    public static CourseManifest Read()
    {
        using var stream = typeof(CourseManifestResource).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException(
                $"The embedded course manifest '{ResourceName}' was not found in the assembly.");

        return JsonSerializer.Deserialize<CourseManifest>(stream, CourseManifestSerialization.Options)
            ?? throw new InvalidOperationException("The embedded course manifest deserialized to null.");
    }
}

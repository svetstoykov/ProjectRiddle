using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProjectRiddle.Core.Models.Courses.Manifest;

/// <summary>
/// Provides the single JSON contract used to read the shipped course manifest.
/// </summary>
public static class CourseManifestSerialization
{
    /// <summary>
    /// Gets the options that read the manifest: camel-cased names, named enum values, and skipped comments.
    /// </summary>
    /// <remarks>
    /// Comments are skipped so the manifest can carry authoring notes beside the content they explain. Trailing
    /// commas stay rejected so a truncated edit fails loudly at startup rather than seeding partial content.
    /// </remarks>
    public static JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = false,
        Converters = { new JsonStringEnumConverter() }
    };
}

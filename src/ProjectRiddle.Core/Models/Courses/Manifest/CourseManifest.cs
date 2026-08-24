namespace ProjectRiddle.Core.Models.Courses.Manifest;

/// <summary>
/// Represents the shipped curriculum exactly as it is authored.
/// </summary>
/// <param name="SchemaVersion">The manifest schema version.</param>
/// <param name="Primer">The ordered primer pages.</param>
/// <param name="Courses">The courses in ordinal order.</param>
/// <remarks>
/// Every field is nullable because this record models untrusted deserialized input. Validation, not the type
/// system, is what establishes that the manifest is complete.
/// </remarks>
public sealed record CourseManifest(
    int SchemaVersion,
    IReadOnlyList<PrimerPageManifestEntry>? Primer,
    IReadOnlyList<CourseManifestEntry>? Courses);

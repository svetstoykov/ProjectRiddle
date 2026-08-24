using ProjectRiddle.Core.Models.Riddles.Authoring;

namespace ProjectRiddle.Core.Models.Courses.Manifest;

/// <summary>
/// Represents one authored lesson exercise and the clue it carries.
/// </summary>
/// <param name="Id">The stable exercise identifier, authored once and never regenerated.</param>
/// <param name="RiddleId">The stable identifier of the riddle holding the clue.</param>
/// <param name="Ordinal">The one-based position within the lesson.</param>
/// <param name="Setup">The optional one-line nudge shown before solving.</param>
/// <param name="TeachingNote">The optional one-line note shown after solving.</param>
/// <param name="Clue">The clue text.</param>
/// <param name="Answer">The answer text.</param>
/// <param name="Explanation">The explanation shown at a terminal state.</param>
/// <param name="Ranges">The labelled structural ranges within the clue.</param>
public sealed record LessonExerciseManifestEntry(
    Guid Id,
    Guid RiddleId,
    int Ordinal,
    string? Setup,
    string? TeachingNote,
    string? Clue,
    string? Answer,
    string? Explanation,
    IReadOnlyList<RiddleRangeInput>? Ranges);

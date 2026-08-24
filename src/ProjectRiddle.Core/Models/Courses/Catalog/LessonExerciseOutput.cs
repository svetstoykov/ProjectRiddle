using ProjectRiddle.Core.Models.Riddles.Discovery;

namespace ProjectRiddle.Core.Models.Courses.Catalog;

/// <summary>
/// Represents the safe projection of one lesson exercise.
/// </summary>
/// <param name="Id">The stable exercise identifier. The riddle identifier behind it is never disclosed.</param>
/// <param name="Ordinal">The one-based position within the lesson.</param>
/// <param name="Setup">The optional one-line nudge shown before solving.</param>
/// <param name="Clue">The full clue text.</param>
/// <param name="AnswerPattern">The public answer pattern.</param>
/// <param name="Ranges">The safe structural ranges. Cannot be <see langword="null" />.</param>
/// <param name="IsComplete">Whether the signed-in caller has completed it; <see langword="null" /> when anonymous.</param>
public sealed record LessonExerciseOutput(
    Guid Id,
    int Ordinal,
    string? Setup,
    string Clue,
    string AnswerPattern,
    IReadOnlyList<PublicRiddleRangeOutput> Ranges,
    bool? IsComplete);

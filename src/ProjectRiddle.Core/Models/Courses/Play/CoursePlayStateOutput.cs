using ProjectRiddle.Core.Models.Riddles.Play;

namespace ProjectRiddle.Core.Models.Courses.Play;

/// <summary>
/// Represents play state returned by a course answer, hint, reveal, or resume command.
/// </summary>
/// <param name="Progress">The progress snapshot. Cannot be <see langword="null" />.</param>
/// <param name="RevealedLetters">The characters for permitted revealed positions. Cannot be <see langword="null" />.</param>
/// <param name="Answer">The normalized answer when a terminal state permits it; otherwise <see langword="null" />.</param>
/// <param name="Explanation">The explanation when a terminal state permits it; otherwise <see langword="null" />.</param>
/// <param name="TeachingNote">The post-solve teaching note when a terminal state permits it; otherwise <see langword="null" />.</param>
/// <param name="IsCorrect">Whether the submitted answer is correct, when this state is an answer result; otherwise <see langword="null" />.</param>
public sealed record CoursePlayStateOutput(
    CourseProgressSnapshotOutput Progress,
    IReadOnlyList<RevealedLetterOutput> RevealedLetters,
    string? Answer,
    string? Explanation,
    string? TeachingNote,
    bool? IsCorrect);

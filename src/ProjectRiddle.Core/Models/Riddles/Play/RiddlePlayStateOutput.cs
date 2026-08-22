using ProjectRiddle.Core.Models.Riddles.Progress;

namespace ProjectRiddle.Core.Models.Riddles.Play;

/// <summary>
/// Represents play-state returned by resume, hint, reveal, or answer operations.
/// </summary>
/// <param name="Progress">The progress snapshot. Cannot be <see langword="null" />.</param>
/// <param name="RevealedLetters">The characters for permitted revealed positions. Cannot be <see langword="null" />.</param>
/// <param name="Answer">The normalized answer when a terminal state permits it; otherwise <see langword="null" />.</param>
/// <param name="Explanation">The final explanation when a terminal state permits it; otherwise <see langword="null" />.</param>
/// <param name="IsCorrect">Whether the submitted answer is correct, when this state is an answer result; otherwise <see langword="null" />.</param>
public sealed record RiddlePlayStateOutput(
    RiddleProgressSnapshotOutput Progress,
    IReadOnlyList<RevealedLetterOutput> RevealedLetters,
    string? Answer,
    string? Explanation,
    bool? IsCorrect);

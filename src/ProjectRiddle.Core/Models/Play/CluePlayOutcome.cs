using ProjectRiddle.Core.Models.Riddles.Play;

namespace ProjectRiddle.Core.Models.Play;

/// <summary>
/// Represents the result of one play command before a capability shapes it for its own contract.
/// </summary>
/// <param name="State">The resulting play state. Cannot be <see langword="null" />.</param>
/// <param name="RevealedLetters">The characters for permitted revealed positions. Cannot be <see langword="null" />.</param>
/// <param name="Answer">The normalized answer when a terminal state permits it; otherwise <see langword="null" />.</param>
/// <param name="Explanation">The explanation when a terminal state permits it; otherwise <see langword="null" />.</param>
/// <param name="IsCorrect">Whether a submitted answer was correct, when this outcome came from an answer command; otherwise <see langword="null" />.</param>
public sealed record CluePlayOutcome(
    CluePlayState State,
    IReadOnlyList<RevealedLetterOutput> RevealedLetters,
    string? Answer,
    string? Explanation,
    bool? IsCorrect);

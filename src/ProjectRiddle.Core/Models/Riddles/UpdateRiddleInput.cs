namespace ProjectRiddle.Core.Models.Riddles;

/// <summary>
/// Represents the input required to update riddle content.
/// </summary>
/// <param name="Id">The riddle identifier.</param>
/// <param name="Clue">The clue text. Cannot be <see langword="null" />.</param>
/// <param name="Answer">The answer text. Cannot be <see langword="null" />.</param>
/// <param name="Explanation">The explanation shown after a permitted reveal. Cannot be <see langword="null" />.</param>
/// <param name="Ranges">The structural ranges labelled on the clue. Cannot be <see langword="null" />.</param>
public sealed record UpdateRiddleInput(
    Guid Id,
    string Clue,
    string Answer,
    string Explanation,
    IReadOnlyList<RiddleRangeInput> Ranges);

namespace ProjectRiddle.Core.Models.Riddles;

/// <summary>
/// Represents the input required to create a riddle.
/// </summary>
/// <param name="Clue">The clue text. Cannot be <see langword="null" />.</param>
/// <param name="Answer">The answer text. Cannot be <see langword="null" />.</param>
/// <param name="AnswerPattern">The answer pattern describing letter groups. Cannot be <see langword="null" />.</param>
/// <param name="Explanation">The explanation shown after a permitted reveal. Cannot be <see langword="null" />.</param>
/// <param name="Ranges">The structural ranges labelled on the clue. Cannot be <see langword="null" />.</param>
public sealed record CreateRiddleInput(
    string Clue,
    string Answer,
    string AnswerPattern,
    string Explanation,
    IReadOnlyList<RiddleRangeInput> Ranges);

using ProjectRiddle.Core.Models.Riddles;

namespace ProjectRiddle.Core.Services.Riddles;

/// <summary>
/// Represents riddle content that has already passed authoring validation.
/// </summary>
/// <param name="Clue">The trimmed clue text.</param>
/// <param name="Answer">The trimmed answer text.</param>
/// <param name="AnswerPattern">The trimmed answer pattern.</param>
/// <param name="Explanation">The trimmed explanation.</param>
/// <param name="Ranges">The validated structural ranges.</param>
internal sealed record ValidatedRiddleContent(
    string Clue,
    string Answer,
    string AnswerPattern,
    string Explanation,
    IReadOnlyList<RiddleRange> Ranges);

namespace ProjectRiddle.Core.Models.Riddles;

/// <summary>
/// Represents a submitted answer for a public riddle.
/// </summary>
/// <param name="RiddleId">The riddle identifier.</param>
/// <param name="Answer">The submitted answer. Cannot be <see langword="null" />.</param>
/// <param name="Progress">The optional anonymous progress snapshot.</param>
public sealed record SubmitRiddleAnswerInput(
    Guid RiddleId,
    string Answer,
    AnonymousRiddleProgressInput? Progress);

using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Models.Play;
using ProjectRiddle.Core.Models.Riddles;
using ProjectRiddle.Core.Results.Models;

namespace ProjectRiddle.Core.Interfaces.Services;

/// <summary>
/// Provides the play behaviour shared by every clue, whatever capability owns eligibility for it.
/// </summary>
/// <remarks>
/// Every method receives an already-authorized riddle and already-validated anonymous state. The engine never
/// decides whether the caller may play the clue; that stays with the calling capability.
/// </remarks>
public interface ICluePlayEngine
{
    /// <summary>
    /// Checks a submitted answer and persists the resulting progress.
    /// </summary>
    /// <param name="riddle">The authorized riddle. Cannot be <see langword="null" />.</param>
    /// <param name="answer">The submitted answer. Cannot be <see langword="null" />.</param>
    /// <param name="anonymous">The validated anonymous state, or <see langword="null" /> to start fresh or to use account progress.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The resulting play outcome, or an expected failure.</returns>
    Task<Result<CluePlayOutcome>> SubmitAnswerAsync(
        Riddle riddle,
        string answer,
        CluePlayState? anonymous,
        CancellationToken cancellationToken);

    /// <summary>
    /// Records one structural hint kind and persists the resulting progress.
    /// </summary>
    /// <param name="riddle">The authorized riddle. Cannot be <see langword="null" />.</param>
    /// <param name="kind">The structural hint kind.</param>
    /// <param name="anonymous">The validated anonymous state, or <see langword="null" /> to start fresh or to use account progress.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The resulting play outcome, or an expected failure.</returns>
    Task<Result<CluePlayOutcome>> UseHintAsync(
        Riddle riddle,
        RiddleRangeKind kind,
        CluePlayState? anonymous,
        CancellationToken cancellationToken);

    /// <summary>
    /// Reveals one previously unrevealed letter and persists the resulting progress.
    /// </summary>
    /// <param name="riddle">The authorized riddle. Cannot be <see langword="null" />.</param>
    /// <param name="anonymous">The validated anonymous state, or <see langword="null" /> to start fresh or to use account progress.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The resulting play outcome, or an expected failure.</returns>
    Task<Result<CluePlayOutcome>> RevealLetterAsync(
        Riddle riddle,
        CluePlayState? anonymous,
        CancellationToken cancellationToken);

    /// <summary>
    /// Rehydrates permitted play state without changing it.
    /// </summary>
    /// <param name="riddle">The authorized riddle. Cannot be <see langword="null" />.</param>
    /// <param name="anonymous">The validated anonymous state, or <see langword="null" /> to start fresh or to use account progress.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The resulting play outcome, or an expected failure.</returns>
    Task<Result<CluePlayOutcome>> ResumeAsync(
        Riddle riddle,
        CluePlayState? anonymous,
        CancellationToken cancellationToken);

    /// <summary>
    /// Merges an imported state monotonically into the account's record for the riddle.
    /// </summary>
    /// <param name="riddle">The authorized riddle. Cannot be <see langword="null" />.</param>
    /// <param name="accountId">The owning account identifier. Cannot be <see cref="Guid.Empty" />.</param>
    /// <param name="imported">The validated imported state. Cannot be <see langword="null" />.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The merged play outcome, or an expected failure.</returns>
    Task<Result<CluePlayOutcome>> MergeAccountProgressAsync(
        Riddle riddle,
        Guid accountId,
        CluePlayState imported,
        CancellationToken cancellationToken);
}

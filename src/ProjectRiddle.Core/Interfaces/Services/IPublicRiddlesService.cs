using ProjectRiddle.Core.Models.Riddles;
using ProjectRiddle.Core.Results.Models;

namespace ProjectRiddle.Core.Interfaces.Services;

/// <summary>
/// Provides public riddle discovery, play, and account progress operations.
/// </summary>
public interface IPublicRiddlesService
{
    /// <summary>
    /// Lists a page of safe archive metadata.
    /// </summary>
    /// <param name="input">The paging input. Cannot be <see langword="null" />.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The archive page, or an expected failure.</returns>
    Task<Result<PublicRiddleListOutput>> ListArchiveAsync(
        ListPublicRiddlesInput input,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets today's eligible public play projection.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>Today's play projection, or an expected failure.</returns>
    Task<Result<PublicRiddlePlayOutput>> GetTodayAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Lists safe metadata for published riddles in the current local week through today.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The week discovery items, or an expected failure.</returns>
    Task<Result<IReadOnlyList<PublicRiddleDiscoveryItemOutput>>> ListWeekAsync(
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets the initial play projection for a public riddle.
    /// </summary>
    /// <param name="id">The riddle identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The play projection, or an expected failure.</returns>
    Task<Result<PublicRiddlePlayOutput>> GetPlayAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Checks a submitted answer and updates progress.
    /// </summary>
    /// <param name="input">The answer input. Cannot be <see langword="null" />.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The resulting play state, or an expected failure.</returns>
    Task<Result<RiddlePlayStateOutput>> SubmitAnswerAsync(
        SubmitRiddleAnswerInput input,
        CancellationToken cancellationToken);

    /// <summary>
    /// Records one structural hint kind on progress.
    /// </summary>
    /// <param name="input">The hint input. Cannot be <see langword="null" />.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The resulting play state, or an expected failure.</returns>
    Task<Result<RiddlePlayStateOutput>> UseHintAsync(
        UseRiddleHintInput input,
        CancellationToken cancellationToken);

    /// <summary>
    /// Reveals one previously unrevealed letter.
    /// </summary>
    /// <param name="input">The reveal input. Cannot be <see langword="null" />.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The resulting play state, or an expected failure.</returns>
    Task<Result<RiddlePlayStateOutput>> RevealLetterAsync(
        RevealRiddleLetterInput input,
        CancellationToken cancellationToken);

    /// <summary>
    /// Rehydrates permitted play state from anonymous or account progress.
    /// </summary>
    /// <param name="input">The resume input. Cannot be <see langword="null" />.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The resulting play state, or an expected failure.</returns>
    Task<Result<RiddlePlayStateOutput>> ResumeAsync(ResumeRiddleInput input, CancellationToken cancellationToken);

    /// <summary>
    /// Lists account-owned riddle progress for a bounded local-date range.
    /// </summary>
    /// <param name="input">The date-range input. Cannot be <see langword="null" />.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The progress list, or an expected failure.</returns>
    Task<Result<AccountRiddleProgressListOutput>> ListProgressAsync(
        ListAccountRiddleProgressInput input,
        CancellationToken cancellationToken);

    /// <summary>
    /// Merges a typed anonymous progress snapshot into the current account record.
    /// </summary>
    /// <param name="input">The imported snapshot. Cannot be <see langword="null" />.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The merged progress snapshot, or an expected failure.</returns>
    Task<Result<RiddleProgressSnapshotOutput>> ImportProgressAsync(
        AnonymousRiddleProgressInput input,
        CancellationToken cancellationToken);
}

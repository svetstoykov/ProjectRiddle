using ProjectRiddle.Core.Models.Riddles;
using ProjectRiddle.Core.Results.Models;

namespace ProjectRiddle.Core.Interfaces.Services;

/// <summary>
/// Provides administrative authoring and publication operations for riddles.
/// </summary>
public interface IRiddlesService
{
    /// <summary>
    /// Creates a draft riddle.
    /// </summary>
    /// <param name="input">The create input. Cannot be <see langword="null" />.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The created riddle, or an expected failure.</returns>
    Task<Result<RiddleOutput>> CreateAsync(CreateRiddleInput input, CancellationToken cancellationToken);

    /// <summary>
    /// Gets one riddle by identifier.
    /// </summary>
    /// <param name="id">The riddle identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The riddle, or an expected failure.</returns>
    Task<Result<RiddleOutput>> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Lists every riddle.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The riddle list, or an expected failure.</returns>
    Task<Result<ListRiddlesOutput>> ListAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Updates authored riddle content without changing publication state.
    /// </summary>
    /// <param name="input">The update input. Cannot be <see langword="null" />.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The updated riddle, or an expected failure.</returns>
    Task<Result<RiddleOutput>> UpdateAsync(UpdateRiddleInput input, CancellationToken cancellationToken);

    /// <summary>
    /// Schedules a riddle onto a Sofia calendar date.
    /// </summary>
    /// <param name="input">The schedule input. Cannot be <see langword="null" />.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The scheduled riddle, or an expected failure.</returns>
    Task<Result<RiddleOutput>> ScheduleAsync(ScheduleRiddleInput input, CancellationToken cancellationToken);

    /// <summary>
    /// Publishes a riddle onto a Sofia calendar date.
    /// </summary>
    /// <param name="input">The publish input. Cannot be <see langword="null" />.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The published riddle, or an expected failure.</returns>
    Task<Result<RiddleOutput>> PublishAsync(PublishRiddleInput input, CancellationToken cancellationToken);

    /// <summary>
    /// Unpublishes a scheduled or published riddle.
    /// </summary>
    /// <param name="id">The riddle identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The unpublished riddle, or an expected failure.</returns>
    Task<Result<RiddleOutput>> UnpublishAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a riddle when the current publication state permits deletion.
    /// </summary>
    /// <param name="id">The riddle identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A successful result, or an expected failure.</returns>
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken);
}

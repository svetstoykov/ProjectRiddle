using ProjectRiddle.Core.Constants.Riddles;
using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Exceptions;
using ProjectRiddle.Core.Interfaces.Accounts;
using ProjectRiddle.Core.Interfaces.Randomness;
using ProjectRiddle.Core.Interfaces.Repositories;
using ProjectRiddle.Core.Interfaces.Services;
using ProjectRiddle.Core.Interfaces.Time;
using ProjectRiddle.Core.Models.Play;
using ProjectRiddle.Core.Models.Riddles;
using ProjectRiddle.Core.Models.Riddles.Play;
using ProjectRiddle.Core.Models.Riddles.Progress;
using ProjectRiddle.Core.Results.Models;
using ProjectRiddle.Core.Validators.Riddles;

namespace ProjectRiddle.Core.Services.Play;

/// <summary>
/// Applies answer checking, hint recording, letter reveals, and resume to any authorized clue.
/// </summary>
/// <remarks>
/// The engine owns the behaviour that must not differ between capabilities: reveals never repeat a position,
/// status is monotonic, a duplicate write merges rather than losing progress, and the answer and explanation
/// leave the server only at a terminal state.
/// </remarks>
public sealed class CluePlayEngine : ICluePlayEngine
{
    private readonly IRiddleProgressRepository _progressRepository;
    private readonly ICurrentAccount _currentAccount;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IRandomNumberGenerator _randomNumberGenerator;

    /// <summary>
    /// Initializes the clue play engine.
    /// </summary>
    /// <param name="progressRepository">The account progress persistence boundary.</param>
    /// <param name="currentAccount">The current caller identity.</param>
    /// <param name="dateTimeProvider">The clock used for timestamps.</param>
    /// <param name="randomNumberGenerator">The source used to select unrevealed letters.</param>
    public CluePlayEngine(
        IRiddleProgressRepository progressRepository,
        ICurrentAccount currentAccount,
        IDateTimeProvider dateTimeProvider,
        IRandomNumberGenerator randomNumberGenerator)
    {
        ArgumentNullException.ThrowIfNull(progressRepository);
        ArgumentNullException.ThrowIfNull(currentAccount);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        ArgumentNullException.ThrowIfNull(randomNumberGenerator);

        this._progressRepository = progressRepository;
        this._currentAccount = currentAccount;
        this._dateTimeProvider = dateTimeProvider;
        this._randomNumberGenerator = randomNumberGenerator;
    }

    /// <inheritdoc />
    public async Task<Result<CluePlayOutcome>> SubmitAnswerAsync(
        Riddle riddle,
        string answer,
        CluePlayState? anonymous,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(riddle);
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(answer))
        {
            return AnswerRequired();
        }

        var normalizedSubmitted = AnswerNormalizer.Normalize(answer);
        if (normalizedSubmitted.Length == 0)
        {
            return AnswerRequired();
        }

        var normalizedAnswer = AnswerNormalizer.Normalize(riddle.Answer);
        var isCorrect = string.Equals(normalizedSubmitted, normalizedAnswer, StringComparison.Ordinal);

        var working = await LoadWorkingProgressAsync(riddle, anonymous, cancellationToken);
        working.Progress.RecordAnswer(isCorrect, _dateTimeProvider.UtcDateTime);

        var saved = await PersistProgressAsync(working.Progress, working.IsNew, cancellationToken);
        if (saved.IsFailure)
        {
            return Result.Failure<CluePlayOutcome>(saved.Error!);
        }

        return Result.Success(ToOutcome(riddle, saved.Value!, isCorrect));
    }

    /// <inheritdoc />
    public async Task<Result<CluePlayOutcome>> UseHintAsync(
        Riddle riddle,
        RiddleRangeKind kind,
        CluePlayState? anonymous,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(riddle);
        cancellationToken.ThrowIfCancellationRequested();

        if (!Enum.IsDefined(kind))
        {
            return Result.Failure<CluePlayOutcome>(
                new OperationError(
                    "The structural hint kind is invalid.",
                    ErrorType.Validation,
                    RiddleErrorCodes.HintKindInvalid));
        }

        var working = await LoadWorkingProgressAsync(riddle, anonymous, cancellationToken);
        working.Progress.RecordHint(kind, _dateTimeProvider.UtcDateTime);

        var saved = await PersistProgressAsync(working.Progress, working.IsNew, cancellationToken);
        if (saved.IsFailure)
        {
            return Result.Failure<CluePlayOutcome>(saved.Error!);
        }

        return Result.Success(ToOutcome(riddle, saved.Value!, isCorrect: null));
    }

    /// <inheritdoc />
    public async Task<Result<CluePlayOutcome>> RevealLetterAsync(
        Riddle riddle,
        CluePlayState? anonymous,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(riddle);
        cancellationToken.ThrowIfCancellationRequested();

        var letters = AnswerLetters.FromNormalizedAnswer(AnswerNormalizer.Normalize(riddle.Answer));

        for (var attempt = 0; attempt < PublicRiddleLimits.ProgressWriteRetryLimit; attempt++)
        {
            var working = await LoadWorkingProgressAsync(riddle, anonymous, cancellationToken);
            var progress = working.Progress;
            if (progress.Status is not RiddleProgressStatus.Solved)
            {
                var remaining = Enumerable.Range(0, letters.Count)
                    .Where(position => !progress.RevealedPositions.Contains(position))
                    .ToArray();
                if (remaining.Length > 0)
                {
                    var selected = remaining[_randomNumberGenerator.NextExclusive(remaining.Length)];
                    progress.RecordReveal(selected, letters.Count, _dateTimeProvider.UtcDateTime);
                }
                else if (progress.Status is RiddleProgressStatus.InProgress && letters.Count > 0)
                {
                    progress.MergeFrom(
                        progress.AnswerAttemptCount,
                        RiddleProgressStatus.FullyRevealed,
                        progress.UsedHints,
                        progress.RevealedPositions,
                        _dateTimeProvider.UtcDateTime);
                }
            }

            var saved = await PersistProgressAsync(progress, working.IsNew, cancellationToken);
            if (saved.IsSuccess)
            {
                return Result.Success(ToOutcome(riddle, saved.Value!, isCorrect: null));
            }

            if (saved.Error!.Type is not ErrorType.Conflict || attempt == PublicRiddleLimits.ProgressWriteRetryLimit - 1)
            {
                return Result.Failure<CluePlayOutcome>(saved.Error);
            }
        }

        return Result.Failure<CluePlayOutcome>(ProgressConflict());
    }

    /// <inheritdoc />
    public async Task<Result<CluePlayOutcome>> ResumeAsync(
        Riddle riddle,
        CluePlayState? anonymous,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(riddle);
        cancellationToken.ThrowIfCancellationRequested();

        var working = await LoadWorkingProgressAsync(riddle, anonymous, cancellationToken);
        return Result.Success(ToOutcome(riddle, working.Progress, isCorrect: null));
    }

    /// <inheritdoc />
    public async Task<Result<CluePlayOutcome>> MergeAccountProgressAsync(
        Riddle riddle,
        Guid accountId,
        CluePlayState imported,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(riddle);
        ArgumentNullException.ThrowIfNull(imported);
        ArgumentOutOfRangeException.ThrowIfEqual(accountId, Guid.Empty);
        cancellationToken.ThrowIfCancellationRequested();

        var existing = await _progressRepository.GetAsync(accountId, riddle.Id, cancellationToken);
        var isNew = existing is null;
        var progress = existing ?? RiddleProgress.Start(accountId, riddle.Id, _dateTimeProvider.UtcDateTime);
        progress.MergeFrom(
            imported.AnswerAttemptCount,
            imported.Status,
            imported.UsedHints,
            imported.RevealedPositions,
            _dateTimeProvider.UtcDateTime);

        var saved = await PersistProgressAsync(progress, isNew, cancellationToken);
        if (saved.IsFailure)
        {
            return Result.Failure<CluePlayOutcome>(saved.Error!);
        }

        return Result.Success(ToOutcome(riddle, saved.Value!, isCorrect: null));
    }

    /// <summary>
    /// Loads the account's record, or builds a fresh in-memory record merged with the supplied anonymous state.
    /// </summary>
    /// <param name="riddle">The authorized riddle.</param>
    /// <param name="anonymous">The already-validated anonymous state, or <see langword="null" />.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The working progress and whether it still needs an insert.</returns>
    /// <remarks>
    /// An authenticated caller's account record always wins; the anonymous argument is ignored in that branch,
    /// which is the guard the resume contract relies on.
    /// </remarks>
    private async Task<LoadedRiddleProgress> LoadWorkingProgressAsync(
        Riddle riddle,
        CluePlayState? anonymous,
        CancellationToken cancellationToken)
    {
        var accountId = _currentAccount.AccountId;
        if (accountId is not null)
        {
            var existing = await _progressRepository.GetAsync(accountId.Value, riddle.Id, cancellationToken);
            if (existing is not null)
            {
                return new LoadedRiddleProgress(existing, IsNew: false);
            }

            return new LoadedRiddleProgress(
                RiddleProgress.Start(accountId.Value, riddle.Id, _dateTimeProvider.UtcDateTime),
                IsNew: true);
        }

        var started = RiddleProgress.Start(Guid.Empty, riddle.Id, _dateTimeProvider.UtcDateTime);
        if (anonymous is not null)
        {
            started.MergeFrom(
                anonymous.AnswerAttemptCount,
                anonymous.Status,
                anonymous.UsedHints,
                anonymous.RevealedPositions,
                _dateTimeProvider.UtcDateTime);
        }

        return new LoadedRiddleProgress(started, IsNew: true);
    }

    private async Task<Result<RiddleProgress>> PersistProgressAsync(
        RiddleProgress progress,
        bool isNew,
        CancellationToken cancellationToken)
    {
        if (progress.AccountId == Guid.Empty)
        {
            return Result.Success(progress);
        }

        try
        {
            if (isNew)
            {
                await _progressRepository.AddAsync(progress, cancellationToken);
            }
            else
            {
                await _progressRepository.UpdateAsync(progress, cancellationToken);
            }

            return Result.Success(progress);
        }
        catch (DuplicateRiddleProgressException)
        {
            var existing = await _progressRepository.GetAsync(progress.AccountId, progress.RiddleId, cancellationToken);
            if (existing is null)
            {
                return Result.Failure<RiddleProgress>(ProgressConflict());
            }

            existing.MergeFrom(
                progress.AnswerAttemptCount,
                progress.Status,
                progress.UsedHints,
                progress.RevealedPositions,
                progress.UpdatedAtUtc);

            try
            {
                await _progressRepository.UpdateAsync(existing, cancellationToken);
                return Result.Success(existing);
            }
            catch (DuplicateRiddleProgressException)
            {
                return Result.Failure<RiddleProgress>(ProgressConflict());
            }
        }
    }

    /// <summary>
    /// Projects progress into an outcome, releasing the answer and explanation only at a terminal status.
    /// </summary>
    /// <param name="riddle">The authorized riddle.</param>
    /// <param name="progress">The saved progress.</param>
    /// <param name="isCorrect">Whether a submitted answer was correct, or <see langword="null" />.</param>
    /// <returns>The play outcome for the caller to shape.</returns>
    private static CluePlayOutcome ToOutcome(Riddle riddle, RiddleProgress progress, bool? isCorrect)
    {
        var letters = AnswerLetters.FromNormalizedAnswer(AnswerNormalizer.Normalize(riddle.Answer));
        var revealedPositions = progress.RevealedPositions.OrderBy(position => position).ToArray();
        var revealedLetters = revealedPositions
            .Select(position => new RevealedLetterOutput(position, letters[position]))
            .ToArray();

        string? answer = null;
        string? explanation = null;
        if (progress.Status is RiddleProgressStatus.Solved or RiddleProgressStatus.FullyRevealed)
        {
            answer = AnswerNormalizer.Normalize(riddle.Answer);
            explanation = riddle.Explanation;
        }

        var state = new CluePlayState(
            progress.Status,
            progress.AnswerAttemptCount,
            progress.UsedHints.OrderBy(kind => kind).ToArray(),
            revealedPositions);

        return new CluePlayOutcome(state, revealedLetters, answer, explanation, isCorrect);
    }

    private static Result<CluePlayOutcome> AnswerRequired()
    {
        return Result.Failure<CluePlayOutcome>(
            new OperationError(
                "An answer is required.",
                ErrorType.Validation,
                RiddleErrorCodes.AnswerRequestInvalid));
    }

    private static OperationError ProgressConflict()
    {
        return new OperationError(
            "The riddle progress could not be updated.",
            ErrorType.Conflict,
            RiddleErrorCodes.ProgressInvalid);
    }
}

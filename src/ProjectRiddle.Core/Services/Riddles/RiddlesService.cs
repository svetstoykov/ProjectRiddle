using Microsoft.Extensions.Logging;
using ProjectRiddle.Core.Constants.Riddles;
using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Interfaces.Accounts;
using ProjectRiddle.Core.Interfaces.Repositories;
using ProjectRiddle.Core.Interfaces.Services;
using ProjectRiddle.Core.Interfaces.Time;
using ProjectRiddle.Core.Models.Play;
using ProjectRiddle.Core.Models.Riddles;
using ProjectRiddle.Core.Models.Riddles.Discovery;
using ProjectRiddle.Core.Models.Riddles.Play;
using ProjectRiddle.Core.Models.Riddles.Progress;
using ProjectRiddle.Core.Results.Models;
using ProjectRiddle.Core.Validators.Riddles;

namespace ProjectRiddle.Core.Services.Riddles;

/// <summary>
/// Coordinates public riddle eligibility, play commands, and account progress.
/// </summary>
public sealed class RiddlesService : IRiddlesService
{
    private readonly IRiddleRepository _riddleRepository;
    private readonly IRiddleProgressRepository _progressRepository;
    private readonly ICurrentAccount _currentAccount;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICluePlayEngine _playEngine;
    private readonly ILogger<RiddlesService> _logger;

    /// <summary>
    /// Initializes the riddles service.
    /// </summary>
    /// <param name="riddleRepository">The riddle persistence boundary.</param>
    /// <param name="progressRepository">The account progress persistence boundary.</param>
    /// <param name="currentAccount">The current caller identity.</param>
    /// <param name="dateTimeProvider">The clock used for local dates and timestamps.</param>
    /// <param name="playEngine">The shared clue play behavior.</param>
    /// <param name="logger">The logger for safe riddle events.</param>
    public RiddlesService(
        IRiddleRepository riddleRepository,
        IRiddleProgressRepository progressRepository,
        ICurrentAccount currentAccount,
        IDateTimeProvider dateTimeProvider,
        ICluePlayEngine playEngine,
        ILogger<RiddlesService> logger)
    {
        ArgumentNullException.ThrowIfNull(riddleRepository);
        ArgumentNullException.ThrowIfNull(progressRepository);
        ArgumentNullException.ThrowIfNull(currentAccount);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        ArgumentNullException.ThrowIfNull(playEngine);
        ArgumentNullException.ThrowIfNull(logger);

        this._riddleRepository = riddleRepository;
        this._progressRepository = progressRepository;
        this._currentAccount = currentAccount;
        this._dateTimeProvider = dateTimeProvider;
        this._playEngine = playEngine;
        this._logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<PublicRiddleListOutput>> ListArchiveAsync(
        ListPublicRiddlesInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();

        if (input.Page < 1 || input.PageSize < 1 || input.PageSize > PublicRiddleLimits.MaxPageSize)
        {
            return Result.Failure<PublicRiddleListOutput>(
                new OperationError(
                    "The archive page and page size must be within the allowed bounds.",
                    ErrorType.Validation,
                    RiddleErrorCodes.ArchivePageInvalid));
        }

        var today = _dateTimeProvider.LocalDate;
        var skip = (input.Page - 1) * input.PageSize;
        var total = await _riddleRepository.CountPublishedArchiveAsync(today, cancellationToken);
        var riddles = await _riddleRepository.ListPublishedArchivePageAsync(
            today,
            skip,
            input.PageSize,
            cancellationToken);

        return Result.Success(
            new PublicRiddleListOutput(
                input.Page,
                input.PageSize,
                total,
                riddles.Select(ToDiscoveryItem).ToArray()));
    }

    /// <inheritdoc />
    public async Task<Result<PublicRiddlePlayOutput>> GetTodayAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var riddle = await _riddleRepository.GetPublishedByPublicationDateAsync(
            _dateTimeProvider.LocalDate,
            cancellationToken);
        if (riddle is null)
        {
            return Result.Failure<PublicRiddlePlayOutput>(
                new OperationError(
                    "Today's riddle is unavailable.",
                    ErrorType.NotFound,
                    RiddleErrorCodes.TodayUnavailable));
        }

        _logger.LogInformation("Returned today's public riddle. RiddleId: {RiddleId}", riddle.Id);
        return Result.Success(ToPlayProjection(riddle));
    }

    /// <inheritdoc />
    public async Task<Result<PublicRiddleWeekOutput>> ListWeekAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var today = _dateTimeProvider.LocalDate;
        var (monday, sunday) = LocalCalendarWeek.Containing(today);
        var riddles = await _riddleRepository.ListPublishedBetweenAsync(monday, today, cancellationToken);
        var items = riddles
            .OrderBy(riddle => riddle.SofiaPublicationDate)
            .Select(ToDiscoveryItem)
            .ToArray();

        return Result.Success(new PublicRiddleWeekOutput(monday, sunday, today, items));
    }

    /// <inheritdoc />
    public async Task<Result<PublicRiddlePlayOutput>> GetPlayAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var riddle = await _riddleRepository.GetByIdAsync(id, cancellationToken);
        var eligibility = EnsurePlayable(riddle);
        if (eligibility.IsFailure)
        {
            return Result.Failure<PublicRiddlePlayOutput>(eligibility.Error!);
        }

        return Result.Success(ToPlayProjection(eligibility.Value!));
    }

    /// <inheritdoc />
    public async Task<Result<RiddlePlayStateOutput>> SubmitAnswerAsync(
        SubmitRiddleAnswerInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();

        var riddle = await _riddleRepository.GetByIdAsync(input.RiddleId, cancellationToken);
        var eligibility = EnsurePlayable(riddle);
        if (eligibility.IsFailure)
        {
            return Result.Failure<RiddlePlayStateOutput>(eligibility.Error!);
        }

        var playable = eligibility.Value!;
        var anonymous = ToAnonymousState(playable, input.Progress);
        if (anonymous.IsFailure)
        {
            return Result.Failure<RiddlePlayStateOutput>(anonymous.Error!);
        }

        var outcome = await _playEngine.SubmitAnswerAsync(playable, input.Answer, anonymous.Value, cancellationToken);
        if (outcome.IsFailure)
        {
            return Result.Failure<RiddlePlayStateOutput>(outcome.Error!);
        }

        _logger.LogInformation(
            "Checked a public riddle answer. RiddleId: {RiddleId} Correct: {IsCorrect}",
            playable.Id,
            outcome.Value!.IsCorrect);
        return Result.Success(ToPlayState(playable, outcome.Value));
    }

    /// <inheritdoc />
    public async Task<Result<RiddlePlayStateOutput>> UseHintAsync(
        UseRiddleHintInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();

        var riddle = await _riddleRepository.GetByIdAsync(input.RiddleId, cancellationToken);
        var eligibility = EnsurePlayable(riddle);
        if (eligibility.IsFailure)
        {
            return Result.Failure<RiddlePlayStateOutput>(eligibility.Error!);
        }

        var playable = eligibility.Value!;
        var anonymous = ToAnonymousState(playable, input.Progress);
        if (anonymous.IsFailure)
        {
            return Result.Failure<RiddlePlayStateOutput>(anonymous.Error!);
        }

        var outcome = await _playEngine.UseHintAsync(playable, input.Kind, anonymous.Value, cancellationToken);
        if (outcome.IsFailure)
        {
            return Result.Failure<RiddlePlayStateOutput>(outcome.Error!);
        }

        _logger.LogInformation("Recorded a public riddle hint. RiddleId: {RiddleId}", playable.Id);
        return Result.Success(ToPlayState(playable, outcome.Value!));
    }

    /// <inheritdoc />
    public async Task<Result<RiddlePlayStateOutput>> RevealLetterAsync(
        RevealRiddleLetterInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();

        var riddle = await _riddleRepository.GetByIdAsync(input.RiddleId, cancellationToken);
        var eligibility = EnsurePlayable(riddle);
        if (eligibility.IsFailure)
        {
            return Result.Failure<RiddlePlayStateOutput>(eligibility.Error!);
        }

        var playable = eligibility.Value!;
        var anonymous = ToAnonymousState(playable, input.Progress);
        if (anonymous.IsFailure)
        {
            return Result.Failure<RiddlePlayStateOutput>(anonymous.Error!);
        }

        var outcome = await _playEngine.RevealLetterAsync(playable, anonymous.Value, cancellationToken);
        if (outcome.IsFailure)
        {
            return Result.Failure<RiddlePlayStateOutput>(outcome.Error!);
        }

        _logger.LogInformation("Revealed a public riddle letter. RiddleId: {RiddleId}", playable.Id);
        return Result.Success(ToPlayState(playable, outcome.Value!));
    }

    /// <inheritdoc />
    public async Task<Result<RiddlePlayStateOutput>> ResumeAsync(
        ResumeRiddleInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();

        var riddle = await _riddleRepository.GetByIdAsync(input.RiddleId, cancellationToken);
        var eligibility = EnsurePlayable(riddle);
        if (eligibility.IsFailure)
        {
            return Result.Failure<RiddlePlayStateOutput>(eligibility.Error!);
        }

        var playable = eligibility.Value!;
        var anonymous = ToAnonymousState(playable, input.Progress);
        if (anonymous.IsFailure)
        {
            return Result.Failure<RiddlePlayStateOutput>(anonymous.Error!);
        }

        var outcome = await _playEngine.ResumeAsync(playable, anonymous.Value, cancellationToken);
        if (outcome.IsFailure)
        {
            return Result.Failure<RiddlePlayStateOutput>(outcome.Error!);
        }

        return Result.Success(ToPlayState(playable, outcome.Value!));
    }

    /// <inheritdoc />
    public async Task<Result<AccountRiddleProgressListOutput>> ListProgressAsync(
        ListAccountRiddleProgressInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();

        var accountId = _currentAccount.AccountId;
        if (accountId is null)
        {
            return Result.Failure<AccountRiddleProgressListOutput>(AuthenticationRequired());
        }

        if (input.FromDate > input.ToDate
            || input.ToDate.DayNumber - input.FromDate.DayNumber + 1 > PublicRiddleLimits.MaxProgressRangeDays)
        {
            return Result.Failure<AccountRiddleProgressListOutput>(
                new OperationError(
                    "The progress date range is invalid.",
                    ErrorType.Validation,
                    RiddleErrorCodes.ProgressInvalid));
        }

        var records = await _progressRepository.ListByAccountAndPublicationDateRangeAsync(
            accountId.Value,
            input.FromDate,
            input.ToDate,
            cancellationToken);
        var riddles = await _riddleRepository.GetByIdsAsync(
            records.Select(record => record.RiddleId).ToArray(),
            cancellationToken);
        var dates = riddles
            .Where(riddle => riddle.SofiaPublicationDate is not null)
            .ToDictionary(riddle => riddle.Id, riddle => riddle.SofiaPublicationDate!.Value);

        var items = records
            .Where(record => dates.ContainsKey(record.RiddleId))
            .Select(record => ToSnapshot(record, dates[record.RiddleId]))
            .OrderBy(item => item.PublicationDate)
            .ToArray();

        return Result.Success(new AccountRiddleProgressListOutput(items));
    }

    /// <inheritdoc />
    public async Task<Result<RiddleProgressSnapshotOutput>> ImportProgressAsync(
        AnonymousRiddleProgressInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();

        var accountId = _currentAccount.AccountId;
        if (accountId is null)
        {
            return Result.Failure<RiddleProgressSnapshotOutput>(AuthenticationRequired());
        }

        var riddle = await _riddleRepository.GetByIdAsync(input.RiddleId, cancellationToken);
        if (!IsPublicContent(riddle))
        {
            return Result.Failure<RiddleProgressSnapshotOutput>(
                new OperationError(
                    "The progress snapshot does not match a public riddle.",
                    ErrorType.UnprocessableInput,
                    RiddleErrorCodes.ProgressReferenceInvalid));
        }

        var playable = riddle!;
        var letters = AnswerLetters.FromNormalizedAnswer(AnswerNormalizer.Normalize(playable.Answer));
        var validation = AnonymousRiddleProgressValidator.Validate(input, playable, letters.Count);
        if (validation.IsFailure)
        {
            return Result.Failure<RiddleProgressSnapshotOutput>(validation.Error!);
        }

        var imported = new CluePlayState(
            input.Status,
            input.AnswerAttemptCount,
            input.UsedHints,
            input.RevealedPositions);

        var merged = await _playEngine.MergeAccountProgressAsync(
            playable,
            accountId.Value,
            imported,
            cancellationToken);
        if (merged.IsFailure)
        {
            return Result.Failure<RiddleProgressSnapshotOutput>(merged.Error!);
        }

        _logger.LogInformation("Imported anonymous riddle progress. RiddleId: {RiddleId}", playable.Id);
        return Result.Success(
            ToSnapshot(playable.Id, playable.SofiaPublicationDate!.Value, merged.Value!.State));
    }

    private Result<Riddle> EnsurePlayable(Riddle? riddle)
    {
        if (!IsPublicContent(riddle))
        {
            return Result.Failure<Riddle>(
                new OperationError(
                    "The riddle was not found.",
                    ErrorType.NotFound,
                    RiddleErrorCodes.NotFound));
        }

        if (RequiresAccount(riddle!.SofiaPublicationDate!.Value))
        {
            return Result.Failure<Riddle>(
                new OperationError(
                    "An authenticated account is required to play an archive riddle.",
                    ErrorType.Unauthorized,
                    RiddleErrorCodes.ArchiveAuthenticationRequired));
        }

        return Result.Success(riddle);
    }

    /// <summary>
    /// Reports whether the current caller is barred from the riddle published on the given local date. Today's riddle
    /// is free for everyone; an earlier one needs an account.
    /// </summary>
    /// <param name="publicationDate">The local publication date.</param>
    /// <returns><see langword="true" /> when the caller needs an account it does not have.</returns>
    private bool RequiresAccount(DateOnly publicationDate)
    {
        return publicationDate < _dateTimeProvider.LocalDate && _currentAccount.AccountId is null;
    }

    private bool IsPublicContent(Riddle? riddle)
    {
        return riddle is not null
            && riddle.PublicationState is RiddlePublicationState.Published
            && riddle.SofiaPublicationDate is not null
            && riddle.SofiaPublicationDate.Value <= _dateTimeProvider.LocalDate;
    }

    /// <summary>
    /// Validates an anonymous snapshot and converts it for the play engine.
    /// </summary>
    /// <param name="riddle">The playable riddle.</param>
    /// <param name="progress">The claimed snapshot, or <see langword="null" />.</param>
    /// <returns>The validated state, <see langword="null" /> when none applies, or a validation failure.</returns>
    /// <remarks>
    /// An authenticated caller's snapshot is discarded without validation. Account progress is authoritative for
    /// that caller, so a malformed body cannot fail a request it would not have influenced.
    /// </remarks>
    private Result<CluePlayState?> ToAnonymousState(Riddle riddle, AnonymousRiddleProgressInput? progress)
    {
        if (_currentAccount.AccountId is not null || progress is null)
        {
            return Result.Success<CluePlayState?>(null);
        }

        var letters = AnswerLetters.FromNormalizedAnswer(AnswerNormalizer.Normalize(riddle.Answer));
        var validation = AnonymousRiddleProgressValidator.Validate(progress, riddle, letters.Count);
        if (validation.IsFailure)
        {
            return Result.Failure<CluePlayState?>(validation.Error!);
        }

        return Result.Success<CluePlayState?>(
            new CluePlayState(
                progress.Status,
                progress.AnswerAttemptCount,
                progress.UsedHints,
                progress.RevealedPositions));
    }

    private static OperationError AuthenticationRequired()
    {
        return new OperationError(
            "An authenticated account is required.",
            ErrorType.Unauthorized);
    }

    /// <summary>
    /// Projects a discovery item. The clue excerpt is withheld from a caller that cannot open the riddle, so an
    /// account-only clue never reaches the client in the first place.
    /// </summary>
    /// <param name="riddle">The published riddle.</param>
    /// <returns>The discovery item for the current caller.</returns>
    private PublicRiddleDiscoveryItemOutput ToDiscoveryItem(Riddle riddle)
    {
        var publicationDate = riddle.SofiaPublicationDate!.Value;

        return new PublicRiddleDiscoveryItemOutput(
            riddle.Id,
            publicationDate,
            RequiresAccount(publicationDate) ? null : ClueExcerpt.FromClue(riddle.Clue),
            riddle.AnswerPattern);
    }

    private static PublicRiddlePlayOutput ToPlayProjection(Riddle riddle)
    {
        var ranges = riddle.Ranges
            .Select(range => new PublicRiddleRangeOutput(range.Kind, range.Start, range.End))
            .ToArray();

        return new PublicRiddlePlayOutput(
            riddle.Id,
            riddle.SofiaPublicationDate!.Value,
            riddle.Clue,
            riddle.AnswerPattern,
            ranges);
    }

    private static RiddlePlayStateOutput ToPlayState(Riddle riddle, CluePlayOutcome outcome)
    {
        return new RiddlePlayStateOutput(
            ToSnapshot(riddle.Id, riddle.SofiaPublicationDate!.Value, outcome.State),
            outcome.RevealedLetters,
            outcome.Answer,
            outcome.Explanation,
            outcome.IsCorrect);
    }

    private static RiddleProgressSnapshotOutput ToSnapshot(Guid riddleId, DateOnly publicationDate, CluePlayState state)
    {
        return new RiddleProgressSnapshotOutput(
            riddleId,
            publicationDate,
            state.Status,
            state.AnswerAttemptCount,
            state.UsedHints,
            state.RevealedPositions,
            state.RevealedPositions.Count);
    }

    private static RiddleProgressSnapshotOutput ToSnapshot(RiddleProgress progress, DateOnly publicationDate)
    {
        return new RiddleProgressSnapshotOutput(
            progress.RiddleId,
            publicationDate,
            progress.Status,
            progress.AnswerAttemptCount,
            progress.UsedHints.OrderBy(kind => kind).ToArray(),
            progress.RevealedPositions.OrderBy(position => position).ToArray(),
            progress.LetterRevealCount);
    }
}

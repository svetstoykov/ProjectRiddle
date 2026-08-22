using Microsoft.Extensions.Logging;
using ProjectRiddle.Core.Constants.Riddles;
using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Exceptions;
using ProjectRiddle.Core.Interfaces.Accounts;
using ProjectRiddle.Core.Interfaces.Randomness;
using ProjectRiddle.Core.Interfaces.Repositories;
using ProjectRiddle.Core.Interfaces.Services;
using ProjectRiddle.Core.Interfaces.Time;
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
public sealed class PublicRiddlesService : IPublicRiddlesService
{
    private readonly IRiddleRepository _riddleRepository;
    private readonly IRiddleProgressRepository _progressRepository;
    private readonly ICurrentAccount _currentAccount;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IRandomNumberGenerator _randomNumberGenerator;
    private readonly ILogger<PublicRiddlesService> _logger;

    /// <summary>
    /// Initializes the public riddles service.
    /// </summary>
    /// <param name="riddleRepository">The riddle persistence boundary.</param>
    /// <param name="progressRepository">The account progress persistence boundary.</param>
    /// <param name="currentAccount">The current caller identity.</param>
    /// <param name="dateTimeProvider">The clock used for local dates and timestamps.</param>
    /// <param name="randomNumberGenerator">The source used to select unrevealed letters.</param>
    /// <param name="logger">The logger for safe public riddle events.</param>
    public PublicRiddlesService(
        IRiddleRepository riddleRepository,
        IRiddleProgressRepository progressRepository,
        ICurrentAccount currentAccount,
        IDateTimeProvider dateTimeProvider,
        IRandomNumberGenerator randomNumberGenerator,
        ILogger<PublicRiddlesService> logger)
    {
        ArgumentNullException.ThrowIfNull(riddleRepository);
        ArgumentNullException.ThrowIfNull(progressRepository);
        ArgumentNullException.ThrowIfNull(currentAccount);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        ArgumentNullException.ThrowIfNull(randomNumberGenerator);
        ArgumentNullException.ThrowIfNull(logger);

        this._riddleRepository = riddleRepository;
        this._progressRepository = progressRepository;
        this._currentAccount = currentAccount;
        this._dateTimeProvider = dateTimeProvider;
        this._randomNumberGenerator = randomNumberGenerator;
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
    public async Task<Result<IReadOnlyList<PublicRiddleDiscoveryItemOutput>>> ListWeekAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var today = _dateTimeProvider.LocalDate;
        var (monday, _) = LocalCalendarWeek.Containing(today);
        var riddles = await _riddleRepository.ListPublishedBetweenAsync(monday, today, cancellationToken);
        var items = riddles
            .OrderBy(riddle => riddle.SofiaPublicationDate)
            .Select(ToDiscoveryItem)
            .ToArray();

        return Result.Success<IReadOnlyList<PublicRiddleDiscoveryItemOutput>>(items);
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

        if (string.IsNullOrWhiteSpace(input.Answer))
        {
            return Result.Failure<RiddlePlayStateOutput>(
                new OperationError(
                    "An answer is required.",
                    ErrorType.Validation,
                    RiddleErrorCodes.AnswerRequestInvalid));
        }

        var riddle = await _riddleRepository.GetByIdAsync(input.RiddleId, cancellationToken);
        var eligibility = EnsurePlayable(riddle);
        if (eligibility.IsFailure)
        {
            return Result.Failure<RiddlePlayStateOutput>(eligibility.Error!);
        }

        var playable = eligibility.Value!;
        var normalizedSubmitted = AnswerNormalizer.Normalize(input.Answer);
        var normalizedAnswer = AnswerNormalizer.Normalize(playable.Answer);
        if (normalizedSubmitted.Length == 0)
        {
            return Result.Failure<RiddlePlayStateOutput>(
                new OperationError(
                    "An answer is required.",
                    ErrorType.Validation,
                    RiddleErrorCodes.AnswerRequestInvalid));
        }

        var loaded = await LoadWorkingProgressAsync(playable, input.Progress, cancellationToken);
        if (loaded.IsFailure)
        {
            return Result.Failure<RiddlePlayStateOutput>(loaded.Error!);
        }

        var isCorrect = string.Equals(normalizedSubmitted, normalizedAnswer, StringComparison.Ordinal);
        var working = loaded.Value!;
        working.Progress.RecordAnswer(isCorrect, _dateTimeProvider.UtcDateTime);
        var saved = await PersistProgressAsync(working.Progress, working.IsNew, cancellationToken);
        if (saved.IsFailure)
        {
            return Result.Failure<RiddlePlayStateOutput>(saved.Error!);
        }

        _logger.LogInformation(
            "Checked a public riddle answer. RiddleId: {RiddleId} Correct: {IsCorrect}",
            playable.Id,
            isCorrect);
        return Result.Success(ToPlayState(playable, saved.Value!, isCorrect));
    }

    /// <inheritdoc />
    public async Task<Result<RiddlePlayStateOutput>> UseHintAsync(
        UseRiddleHintInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();

        if (!Enum.IsDefined(input.Kind))
        {
            return InvalidHintKind();
        }

        var riddle = await _riddleRepository.GetByIdAsync(input.RiddleId, cancellationToken);
        var eligibility = EnsurePlayable(riddle);
        if (eligibility.IsFailure)
        {
            return Result.Failure<RiddlePlayStateOutput>(eligibility.Error!);
        }

        var playable = eligibility.Value!;
        var loaded = await LoadWorkingProgressAsync(playable, input.Progress, cancellationToken);
        if (loaded.IsFailure)
        {
            return Result.Failure<RiddlePlayStateOutput>(loaded.Error!);
        }

        var working = loaded.Value!;
        working.Progress.RecordHint(input.Kind, _dateTimeProvider.UtcDateTime);
        var saved = await PersistProgressAsync(working.Progress, working.IsNew, cancellationToken);
        if (saved.IsFailure)
        {
            return Result.Failure<RiddlePlayStateOutput>(saved.Error!);
        }

        _logger.LogInformation("Recorded a public riddle hint. RiddleId: {RiddleId}", playable.Id);
        return Result.Success(ToPlayState(playable, saved.Value!, isCorrect: null));
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
        var letters = AnswerLetters.FromNormalizedAnswer(AnswerNormalizer.Normalize(playable.Answer));

        for (var attempt = 0; attempt < PublicRiddleLimits.ProgressWriteRetryLimit; attempt++)
        {
            var loaded = await LoadWorkingProgressAsync(playable, input.Progress, cancellationToken);
            if (loaded.IsFailure)
            {
                return Result.Failure<RiddlePlayStateOutput>(loaded.Error!);
            }

            var progress = loaded.Value!.Progress;
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

            var saved = await PersistProgressAsync(progress, loaded.Value!.IsNew, cancellationToken);
            if (saved.IsSuccess)
            {
                _logger.LogInformation("Revealed a public riddle letter. RiddleId: {RiddleId}", playable.Id);
                return Result.Success(ToPlayState(playable, saved.Value!, isCorrect: null));
            }

            if (saved.Error!.Type is not ErrorType.Conflict || attempt == PublicRiddleLimits.ProgressWriteRetryLimit - 1)
            {
                return Result.Failure<RiddlePlayStateOutput>(saved.Error);
            }
        }

        return Result.Failure<RiddlePlayStateOutput>(
            new OperationError(
                "The riddle progress could not be updated.",
                ErrorType.Conflict,
                RiddleErrorCodes.ProgressInvalid));
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
        var snapshot = _currentAccount.AccountId is null ? input.Progress : null;
        var loaded = await LoadWorkingProgressAsync(playable, snapshot, cancellationToken);
        if (loaded.IsFailure)
        {
            return Result.Failure<RiddlePlayStateOutput>(loaded.Error!);
        }

        return Result.Success(ToPlayState(playable, loaded.Value!.Progress, isCorrect: null));
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

        var existing = await _progressRepository.GetAsync(accountId.Value, playable.Id, cancellationToken);
        var isNew = existing is null;
        var progress = existing ?? RiddleProgress.Start(accountId.Value, playable.Id, _dateTimeProvider.UtcDateTime);
        progress.MergeFrom(
            input.AnswerAttemptCount,
            input.Status,
            input.UsedHints,
            input.RevealedPositions,
            _dateTimeProvider.UtcDateTime);

        var saved = await PersistProgressAsync(progress, isNew, cancellationToken);
        if (saved.IsFailure)
        {
            return Result.Failure<RiddleProgressSnapshotOutput>(saved.Error!);
        }

        _logger.LogInformation("Imported anonymous riddle progress. RiddleId: {RiddleId}", playable.Id);
        return Result.Success(ToSnapshot(saved.Value!, playable.SofiaPublicationDate!.Value));
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

        var publicationDate = riddle!.SofiaPublicationDate!.Value;
        if (publicationDate < _dateTimeProvider.LocalDate && _currentAccount.AccountId is null)
        {
            return Result.Failure<Riddle>(
                new OperationError(
                    "An authenticated account is required to play an archive riddle.",
                    ErrorType.Unauthorized,
                    RiddleErrorCodes.ArchiveAuthenticationRequired));
        }

        return Result.Success(riddle);
    }

    private bool IsPublicContent(Riddle? riddle)
    {
        return riddle is not null
            && riddle.PublicationState is RiddlePublicationState.Published
            && riddle.SofiaPublicationDate is not null
            && riddle.SofiaPublicationDate.Value <= _dateTimeProvider.LocalDate;
    }

    private async Task<Result<LoadedRiddleProgress>> LoadWorkingProgressAsync(
        Riddle riddle,
        AnonymousRiddleProgressInput? anonymousProgress,
        CancellationToken cancellationToken)
    {
        var accountId = _currentAccount.AccountId;
        if (accountId is not null)
        {
            var existing = await _progressRepository.GetAsync(accountId.Value, riddle.Id, cancellationToken);
            if (existing is not null)
            {
                return Result.Success(new LoadedRiddleProgress(existing, IsNew: false));
            }

            return Result.Success(
                new LoadedRiddleProgress(
                    RiddleProgress.Start(accountId.Value, riddle.Id, _dateTimeProvider.UtcDateTime),
                    true));
        }

        var started = RiddleProgress.Start(Guid.Empty, riddle.Id, _dateTimeProvider.UtcDateTime);
        if (anonymousProgress is null)
        {
            return Result.Success(new LoadedRiddleProgress(started, true));
        }

        var letters = AnswerLetters.FromNormalizedAnswer(AnswerNormalizer.Normalize(riddle.Answer));
        var validation = AnonymousRiddleProgressValidator.Validate(anonymousProgress, riddle, letters.Count);
        if (validation.IsFailure)
        {
            return Result.Failure<LoadedRiddleProgress>(validation.Error!);
        }

        started.MergeFrom(
            anonymousProgress.AnswerAttemptCount,
            anonymousProgress.Status,
            anonymousProgress.UsedHints,
            anonymousProgress.RevealedPositions,
            _dateTimeProvider.UtcDateTime);
        return Result.Success(new LoadedRiddleProgress(started, true));
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
                return Result.Failure<RiddleProgress>(
                    new OperationError(
                        "The riddle progress could not be updated.",
                        ErrorType.Conflict,
                        RiddleErrorCodes.ProgressInvalid));
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
                return Result.Failure<RiddleProgress>(
                    new OperationError(
                        "The riddle progress could not be updated.",
                        ErrorType.Conflict,
                        RiddleErrorCodes.ProgressInvalid));
            }
        }
    }

    private static Result<RiddlePlayStateOutput> InvalidHintKind()
    {
        return Result.Failure<RiddlePlayStateOutput>(
            new OperationError(
                "The structural hint kind is invalid.",
                ErrorType.Validation,
                RiddleErrorCodes.HintKindInvalid));
    }

    private static OperationError AuthenticationRequired()
    {
        return new OperationError(
            "An authenticated account is required.",
            ErrorType.Unauthorized);
    }

    private static PublicRiddleDiscoveryItemOutput ToDiscoveryItem(Riddle riddle)
    {
        return new PublicRiddleDiscoveryItemOutput(
            riddle.Id,
            riddle.SofiaPublicationDate!.Value,
            ClueExcerpt.FromClue(riddle.Clue),
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

    private static RiddlePlayStateOutput ToPlayState(Riddle riddle, RiddleProgress progress, bool? isCorrect)
    {
        var letters = AnswerLetters.FromNormalizedAnswer(AnswerNormalizer.Normalize(riddle.Answer));
        var revealedLetters = progress.RevealedPositions
            .OrderBy(position => position)
            .Select(position => new RevealedLetterOutput(position, letters[position]))
            .ToArray();

        string? answer = null;
        string? explanation = null;
        if (progress.Status is RiddleProgressStatus.Solved or RiddleProgressStatus.FullyRevealed)
        {
            answer = AnswerNormalizer.Normalize(riddle.Answer);
            explanation = riddle.Explanation;
        }

        return new RiddlePlayStateOutput(
            ToSnapshot(progress, riddle.SofiaPublicationDate!.Value),
            revealedLetters,
            answer,
            explanation,
            isCorrect);
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

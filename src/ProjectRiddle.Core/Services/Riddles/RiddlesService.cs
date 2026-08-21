using Microsoft.Extensions.Logging;
using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Exceptions;
using ProjectRiddle.Core.Interfaces.Repositories;
using ProjectRiddle.Core.Interfaces.Services;
using ProjectRiddle.Core.Interfaces.Time;
using ProjectRiddle.Core.Models.Riddles;
using ProjectRiddle.Core.Results.Models;

namespace ProjectRiddle.Core.Services.Riddles;

/// <summary>
/// Coordinates riddle authoring, range validation, and publication transitions.
/// </summary>
public sealed partial class RiddlesService : IRiddlesService
{
    private readonly IRiddleRepository riddleRepository;
    private readonly IDateTimeProvider dateTimeProvider;
    private readonly ILogger<RiddlesService> logger;

    /// <summary>
    /// Initializes the riddles service.
    /// </summary>
    /// <param name="riddleRepository">The riddle persistence boundary.</param>
    /// <param name="dateTimeProvider">The clock used for Sofia dates and timestamps.</param>
    /// <param name="logger">The logger for safe riddle lifecycle events.</param>
    public RiddlesService(
        IRiddleRepository riddleRepository,
        IDateTimeProvider dateTimeProvider,
        ILogger<RiddlesService> logger)
    {
        ArgumentNullException.ThrowIfNull(riddleRepository);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        ArgumentNullException.ThrowIfNull(logger);

        this.riddleRepository = riddleRepository;
        this.dateTimeProvider = dateTimeProvider;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<RiddleOutput>> CreateAsync(CreateRiddleInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();

        var content = ValidateContent(input.Clue, input.Answer, input.AnswerPattern, input.Explanation, input.Ranges);
        if (content.IsFailure)
        {
            return Result.Failure<RiddleOutput>(content.Error!);
        }

        var utcNow = dateTimeProvider.UtcDateTime;
        var riddle = new Riddle(
            Guid.NewGuid(),
            content.Value!.Clue,
            content.Value.Answer,
            content.Value.AnswerPattern,
            content.Value.Explanation,
            RiddlePublicationState.Draft,
            sofiaPublicationDate: null,
            utcNow,
            utcNow);
        riddle.ReplaceRanges(content.Value.Ranges);

        await riddleRepository.AddAsync(riddle, cancellationToken);
        LogRiddleCreated(logger, riddle.Id);
        return Result.Success(ToOutput(riddle));
    }

    /// <inheritdoc />
    public async Task<Result<RiddleOutput>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var riddle = await riddleRepository.GetByIdAsync(id, cancellationToken);
        if (riddle is null)
        {
            return NotFound<RiddleOutput>();
        }

        return Result.Success(ToOutput(riddle));
    }

    /// <inheritdoc />
    public async Task<Result<ListRiddlesOutput>> ListAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var riddles = await riddleRepository.ListAsync(cancellationToken);
        var ordered = riddles
            .OrderBy(riddle => riddle.PublicationState)
            .ThenByDescending(riddle => riddle.SofiaPublicationDate)
            .ThenByDescending(riddle => riddle.CreatedAtUtc)
            .Select(ToOutput)
            .ToArray();

        return Result.Success(new ListRiddlesOutput(ordered));
    }

    /// <inheritdoc />
    public async Task<Result<RiddleOutput>> UpdateAsync(UpdateRiddleInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();

        var riddle = await riddleRepository.GetByIdAsync(input.Id, cancellationToken);
        if (riddle is null)
        {
            return NotFound<RiddleOutput>();
        }

        var content = ValidateContent(input.Clue, input.Answer, input.AnswerPattern, input.Explanation, input.Ranges);
        if (content.IsFailure)
        {
            return Result.Failure<RiddleOutput>(content.Error!);
        }

        riddle.UpdateContent(
            content.Value!.Clue,
            content.Value.Answer,
            content.Value.AnswerPattern,
            content.Value.Explanation,
            content.Value.Ranges,
            dateTimeProvider.UtcDateTime);

        await riddleRepository.UpdateAsync(riddle, cancellationToken);
        LogRiddleUpdated(logger, riddle.Id);
        return Result.Success(ToOutput(riddle));
    }

    /// <inheritdoc />
    public async Task<Result<RiddleOutput>> ScheduleAsync(
        ScheduleRiddleInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();

        var riddle = await riddleRepository.GetByIdAsync(input.Id, cancellationToken);
        if (riddle is null)
        {
            return NotFound<RiddleOutput>();
        }

        if (riddle.PublicationState is not RiddlePublicationState.Draft
            and not RiddlePublicationState.Unpublished)
        {
            return InvalidTransition<RiddleOutput>();
        }

        if (input.PublicationDate < dateTimeProvider.LocalDate)
        {
            return Result.Failure<RiddleOutput>(
                new OperationError(
                    "A riddle cannot be scheduled on a Sofia date before today.",
                    ErrorType.Validation,
                    RiddleErrorCodes.PublicationDateInvalid));
        }

        var occupancy = await EnsureDateAvailableAsync(input.PublicationDate, riddle.Id, cancellationToken);
        if (occupancy.IsFailure)
        {
            return Result.Failure<RiddleOutput>(occupancy.Error!);
        }

        riddle.Schedule(input.PublicationDate, dateTimeProvider.UtcDateTime);

        try
        {
            await riddleRepository.UpdateAsync(riddle, cancellationToken);
        }
        catch (DuplicatePublicationDateException)
        {
            return DateConflict<RiddleOutput>();
        }

        LogRiddleScheduled(logger, riddle.Id);
        return Result.Success(ToOutput(riddle));
    }

    /// <inheritdoc />
    public async Task<Result<RiddleOutput>> PublishAsync(
        PublishRiddleInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();

        var riddle = await riddleRepository.GetByIdAsync(input.Id, cancellationToken);
        if (riddle is null)
        {
            return NotFound<RiddleOutput>();
        }

        if (riddle.PublicationState is RiddlePublicationState.Published)
        {
            return InvalidTransition<RiddleOutput>();
        }

        var publicationDate = input.PublicationDate ?? riddle.SofiaPublicationDate;
        if (publicationDate is null)
        {
            return Result.Failure<RiddleOutput>(
                new OperationError(
                    "A Sofia publication date is required to publish a riddle.",
                    ErrorType.Validation,
                    RiddleErrorCodes.PublicationDateInvalid));
        }

        var occupancy = await EnsureDateAvailableAsync(publicationDate.Value, riddle.Id, cancellationToken);
        if (occupancy.IsFailure)
        {
            return Result.Failure<RiddleOutput>(occupancy.Error!);
        }

        riddle.Publish(publicationDate.Value, dateTimeProvider.UtcDateTime);

        try
        {
            await riddleRepository.UpdateAsync(riddle, cancellationToken);
        }
        catch (DuplicatePublicationDateException)
        {
            return DateConflict<RiddleOutput>();
        }

        LogRiddlePublished(logger, riddle.Id);
        return Result.Success(ToOutput(riddle));
    }

    /// <inheritdoc />
    public async Task<Result<RiddleOutput>> UnpublishAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var riddle = await riddleRepository.GetByIdAsync(id, cancellationToken);
        if (riddle is null)
        {
            return NotFound<RiddleOutput>();
        }

        if (riddle.PublicationState is not RiddlePublicationState.Scheduled
            and not RiddlePublicationState.Published)
        {
            return InvalidTransition<RiddleOutput>();
        }

        riddle.Unpublish(dateTimeProvider.UtcDateTime);
        await riddleRepository.UpdateAsync(riddle, cancellationToken);
        LogRiddleUnpublished(logger, riddle.Id);
        return Result.Success(ToOutput(riddle));
    }

    /// <inheritdoc />
    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var riddle = await riddleRepository.GetByIdAsync(id, cancellationToken);
        if (riddle is null)
        {
            return Result.Failure(
                new OperationError(
                    "The riddle was not found.",
                    ErrorType.NotFound,
                    RiddleErrorCodes.NotFound));
        }

        if (riddle.PublicationState is not RiddlePublicationState.Draft
            and not RiddlePublicationState.Unpublished)
        {
            return Result.Failure(
                new OperationError(
                    "Only draft or unpublished riddles can be deleted.",
                    ErrorType.InvalidOperation,
                    RiddleErrorCodes.DeleteNotPermitted));
        }

        await riddleRepository.DeleteAsync(riddle, cancellationToken);
        LogRiddleDeleted(logger, riddle.Id);
        return Result.Success();
    }

    private async Task<Result> EnsureDateAvailableAsync(
        DateOnly publicationDate,
        Guid riddleId,
        CancellationToken cancellationToken)
    {
        var occupant = await riddleRepository.GetOccupyingByPublicationDateAsync(publicationDate, cancellationToken);
        if (occupant is not null && occupant.Id != riddleId)
        {
            return Result.Failure(
                new OperationError(
                    "Another riddle already occupies this Sofia publication date.",
                    ErrorType.Conflict,
                    RiddleErrorCodes.PublicationDateConflict));
        }

        return Result.Success();
    }

    private static Result<ValidatedRiddleContent> ValidateContent(
        string clue,
        string answer,
        string answerPattern,
        string explanation,
        IReadOnlyList<RiddleRangeInput> ranges)
    {
        ArgumentNullException.ThrowIfNull(ranges);

        if (string.IsNullOrWhiteSpace(clue))
        {
            return Result.Failure<ValidatedRiddleContent>(
                new OperationError(
                    "Clue is required.",
                    ErrorType.Validation,
                    RiddleErrorCodes.ClueInvalid));
        }

        if (string.IsNullOrWhiteSpace(answer))
        {
            return Result.Failure<ValidatedRiddleContent>(
                new OperationError(
                    "Answer is required.",
                    ErrorType.Validation,
                    RiddleErrorCodes.AnswerInvalid));
        }

        if (string.IsNullOrWhiteSpace(explanation))
        {
            return Result.Failure<ValidatedRiddleContent>(
                new OperationError(
                    "Explanation is required.",
                    ErrorType.Validation,
                    RiddleErrorCodes.ExplanationInvalid));
        }

        var trimmedClue = clue.Trim();
        var trimmedAnswer = answer.Trim();
        var trimmedPattern = answerPattern.Trim();
        var trimmedExplanation = explanation.Trim();

        var patternResult = AnswerPatternValidator.Validate(trimmedAnswer, trimmedPattern);
        if (patternResult.IsFailure)
        {
            return Result.Failure<ValidatedRiddleContent>(patternResult.Error!);
        }

        var rangeResult = RiddleRangeValidator.Validate(trimmedClue, ranges);
        if (rangeResult.IsFailure)
        {
            return Result.Failure<ValidatedRiddleContent>(rangeResult.Error!);
        }

        var mappedRanges = ranges
            .Select(range => new RiddleRange(Guid.NewGuid(), range.Kind, range.Start, range.End))
            .ToArray();

        return Result.Success(
            new ValidatedRiddleContent(
                trimmedClue,
                trimmedAnswer,
                trimmedPattern,
                trimmedExplanation,
                mappedRanges));
    }

    private static Result<T> NotFound<T>()
    {
        return Result.Failure<T>(
            new OperationError(
                "The riddle was not found.",
                ErrorType.NotFound,
                RiddleErrorCodes.NotFound));
    }

    private static Result<T> InvalidTransition<T>()
    {
        return Result.Failure<T>(
            new OperationError(
                "The requested publication transition is not allowed for the current state.",
                ErrorType.InvalidOperation,
                RiddleErrorCodes.TransitionInvalid));
    }

    private static Result<T> DateConflict<T>()
    {
        return Result.Failure<T>(
            new OperationError(
                "Another riddle already occupies this Sofia publication date.",
                ErrorType.Conflict,
                RiddleErrorCodes.PublicationDateConflict));
    }

    private static RiddleOutput ToOutput(Riddle riddle)
    {
        var ranges = riddle.Ranges
            .Select(range => new RiddleRangeOutput(range.Id, range.Kind, range.Start, range.End))
            .ToArray();

        return new RiddleOutput(
            riddle.Id,
            riddle.Clue,
            riddle.Answer,
            riddle.AnswerPattern,
            riddle.Explanation,
            riddle.PublicationState,
            riddle.SofiaPublicationDate,
            ranges,
            riddle.CreatedAtUtc,
            riddle.UpdatedAtUtc);
    }

    [LoggerMessage(
        EventId = 2100,
        Level = LogLevel.Information,
        Message = "Created a riddle. RiddleId: {RiddleId}")]
    private static partial void LogRiddleCreated(ILogger logger, Guid riddleId);

    [LoggerMessage(
        EventId = 2101,
        Level = LogLevel.Information,
        Message = "Updated a riddle. RiddleId: {RiddleId}")]
    private static partial void LogRiddleUpdated(ILogger logger, Guid riddleId);

    [LoggerMessage(
        EventId = 2102,
        Level = LogLevel.Information,
        Message = "Scheduled a riddle. RiddleId: {RiddleId}")]
    private static partial void LogRiddleScheduled(ILogger logger, Guid riddleId);

    [LoggerMessage(
        EventId = 2103,
        Level = LogLevel.Information,
        Message = "Published a riddle. RiddleId: {RiddleId}")]
    private static partial void LogRiddlePublished(ILogger logger, Guid riddleId);

    [LoggerMessage(
        EventId = 2104,
        Level = LogLevel.Information,
        Message = "Unpublished a riddle. RiddleId: {RiddleId}")]
    private static partial void LogRiddleUnpublished(ILogger logger, Guid riddleId);

    [LoggerMessage(
        EventId = 2105,
        Level = LogLevel.Information,
        Message = "Deleted a riddle. RiddleId: {RiddleId}")]
    private static partial void LogRiddleDeleted(ILogger logger, Guid riddleId);
}

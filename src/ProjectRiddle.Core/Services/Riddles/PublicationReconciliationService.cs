using System.Diagnostics;
using Microsoft.Extensions.Logging;
using ProjectRiddle.Core.Constants.Riddles;
using ProjectRiddle.Core.Exceptions;
using ProjectRiddle.Core.Interfaces.Repositories;
using ProjectRiddle.Core.Interfaces.Services;
using ProjectRiddle.Core.Interfaces.Time;
using ProjectRiddle.Core.Models.Riddles;
using ProjectRiddle.Core.Models.Riddles.Publication;
using ProjectRiddle.Core.Results.Models;

namespace ProjectRiddle.Core.Services.Riddles;

/// <summary>
/// Publishes the latest due schedule and expires every older due schedule.
/// </summary>
public sealed class PublicationReconciliationService : IPublicationReconciliationService
{
    private readonly IRiddleRepository _riddleRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<PublicationReconciliationService> _logger;

    /// <summary>
    /// Initializes the publication reconciliation service.
    /// </summary>
    /// <param name="riddleRepository">The riddle persistence boundary.</param>
    /// <param name="dateTimeProvider">The clock used for the configured local date and timestamps.</param>
    /// <param name="logger">The logger for safe reconciliation outcomes.</param>
    public PublicationReconciliationService(
        IRiddleRepository riddleRepository,
        IDateTimeProvider dateTimeProvider,
        ILogger<PublicationReconciliationService> logger)
    {
        ArgumentNullException.ThrowIfNull(riddleRepository);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        ArgumentNullException.ThrowIfNull(logger);

        this._riddleRepository = riddleRepository;
        this._dateTimeProvider = dateTimeProvider;
        this._logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<PublicationReconciliationOutput>> ReconcileAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        for (var attempt = 0; attempt < PublicationReconciliationLimits.WriteRetryLimit; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var localDate = _dateTimeProvider.LocalDate;

            try
            {
                var output = await ReconcileOnceAsync(localDate, cancellationToken);
                LogReconciled(localDate, output, stopwatch.ElapsedMilliseconds);
                return Result.Success(output);
            }
            catch (StaleRiddleWriteException) when (attempt < PublicationReconciliationLimits.WriteRetryLimit - 1)
            {
                // The batch was not stored. The next attempt loads the rows again and revalidates their states.
            }
            catch (StaleRiddleWriteException)
            {
                return Failed(localDate, RiddleErrorCodes.StaleWrite, stopwatch.ElapsedMilliseconds);
            }
            catch (DuplicatePublicationDateException exception)
            {
                return Failed(
                    localDate,
                    RiddleErrorCodes.PublicationDateConflict,
                    stopwatch.ElapsedMilliseconds,
                    exception.PublicationDate);
            }
        }

        return Failed(
            _dateTimeProvider.LocalDate,
            RiddleErrorCodes.StaleWrite,
            stopwatch.ElapsedMilliseconds);
    }

    private async Task<PublicationReconciliationOutput> ReconcileOnceAsync(
        DateOnly localDate,
        CancellationToken cancellationToken)
    {
        var due = await _riddleRepository.ListDueScheduledAsync(localDate, cancellationToken);
        if (due.Count == 0)
        {
            return new PublicationReconciliationOutput(null, null, []);
        }

        var ordered = due
            .OrderByDescending(riddle => riddle.SofiaPublicationDate)
            .ThenBy(riddle => riddle.Id)
            .ToArray();
        var selected = ordered[0];
        var utcNow = _dateTimeProvider.UtcDateTime;
        var publishedDate = selected.SofiaPublicationDate!.Value;
        selected.Publish(publishedDate, utcNow);

        var expiredIds = new List<Guid>(ordered.Length - 1);
        foreach (var older in ordered.Skip(1))
        {
            older.Expire(utcNow);
            expiredIds.Add(older.Id);
        }

        await _riddleRepository.UpdateBatchAsync(ordered, cancellationToken);
        return new PublicationReconciliationOutput(selected.Id, publishedDate, expiredIds);
    }

    private void LogReconciled(
        DateOnly localDate,
        PublicationReconciliationOutput output,
        long elapsedMilliseconds)
    {
        _logger.LogInformation(
            "Reconciled scheduled publication. LocalDate: {LocalDate} PublishedRiddleId: {PublishedRiddleId} PublishedDate: {PublishedDate} ExpiredRiddleIds: {@ExpiredRiddleIds} ElapsedMs: {ElapsedMs}",
            localDate,
            output.PublishedRiddleId,
            output.PublishedDate,
            output.ExpiredRiddleIds,
            elapsedMilliseconds);
    }

    private Result<PublicationReconciliationOutput> Failed(
        DateOnly localDate,
        string failureCategory,
        long elapsedMilliseconds,
        DateOnly? publicationDate = null)
    {
        _logger.LogWarning(
            "Scheduled publication reconciliation did not persist. LocalDate: {LocalDate} PublicationDate: {PublicationDate} FailureCategory: {FailureCategory} ElapsedMs: {ElapsedMs}",
            localDate,
            publicationDate,
            failureCategory,
            elapsedMilliseconds);

        var code = failureCategory;
        var error = code == RiddleErrorCodes.PublicationDateConflict
            ? new OperationError(
                "Another riddle already occupies this Sofia publication date.",
                ErrorType.Conflict,
                code)
            : new OperationError(
                "The scheduled publication changed before reconciliation could be stored.",
                ErrorType.Conflict,
                code);

        return Result.Failure<PublicationReconciliationOutput>(error);
    }
}

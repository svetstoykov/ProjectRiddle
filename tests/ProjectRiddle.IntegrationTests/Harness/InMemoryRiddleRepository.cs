using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Exceptions;
using ProjectRiddle.Core.Interfaces.Repositories;
using ProjectRiddle.Core.Models.Riddles;

namespace ProjectRiddle.IntegrationTests.Harness;

/// <summary>
/// Stores riddles in memory so Core domain tests do not depend on Infrastructure.
/// </summary>
public sealed class InMemoryRiddleRepository : IRiddleRepository
{
    private readonly List<Riddle> _riddles = [];
    private readonly Dictionary<Guid, Riddle> _committed = [];

    /// <summary>
    /// Gets or sets how many of the next single-riddle updates should fail as stale writes.
    /// </summary>
    public int FailNextUpdateCount { get; set; }

    /// <summary>
    /// Gets or sets how many of the next reconciliation batches should fail as stale writes.
    /// </summary>
    public int FailNextBatchUpdateCount { get; set; }

    /// <inheritdoc />
    public Task<Riddle?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_riddles.SingleOrDefault(riddle => riddle.Id == id));
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Riddle>> ListAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<Riddle>>(_riddles.Where(riddle => !riddle.IsLesson).ToArray());
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Riddle>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(ids);
        cancellationToken.ThrowIfCancellationRequested();
        var matches = _riddles.Where(riddle => ids.Contains(riddle.Id)).ToArray();
        return Task.FromResult<IReadOnlyList<Riddle>>(matches);
    }

    /// <inheritdoc />
    public Task<Riddle?> GetOccupyingByPublicationDateAsync(
        DateOnly publicationDate,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(
            _riddles.SingleOrDefault(
                riddle => riddle.SofiaPublicationDate == publicationDate
                    && (riddle.PublicationState == RiddlePublicationState.Scheduled
                        || riddle.PublicationState == RiddlePublicationState.Published)));
    }

    /// <inheritdoc />
    public Task<Riddle?> GetPublishedByPublicationDateAsync(
        DateOnly publicationDate,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(
            _riddles.SingleOrDefault(
                riddle => riddle.SofiaPublicationDate == publicationDate
                    && riddle.PublicationState == RiddlePublicationState.Published));
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Riddle>> ListPublishedBetweenAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var matches = _riddles
            .Where(
                riddle => riddle.PublicationState == RiddlePublicationState.Published
                    && riddle.SofiaPublicationDate is not null
                    && riddle.SofiaPublicationDate.Value >= fromDate
                    && riddle.SofiaPublicationDate.Value <= toDate)
            .ToArray();
        return Task.FromResult<IReadOnlyList<Riddle>>(matches);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Riddle>> ListPublishedArchivePageAsync(
        DateOnly beforeDate,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var matches = _riddles
            .Where(
                riddle => riddle.PublicationState == RiddlePublicationState.Published
                    && riddle.SofiaPublicationDate is not null
                    && riddle.SofiaPublicationDate.Value < beforeDate)
            .OrderByDescending(riddle => riddle.SofiaPublicationDate)
            .Skip(skip)
            .Take(take)
            .ToArray();
        return Task.FromResult<IReadOnlyList<Riddle>>(matches);
    }

    /// <inheritdoc />
    public Task<int> CountPublishedArchiveAsync(DateOnly beforeDate, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var count = _riddles.Count(
            riddle => riddle.PublicationState == RiddlePublicationState.Published
                && riddle.SofiaPublicationDate is not null
                && riddle.SofiaPublicationDate.Value < beforeDate);
        return Task.FromResult(count);
    }

    /// <inheritdoc />
    public Task AddAsync(Riddle riddle, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(riddle);
        cancellationToken.ThrowIfCancellationRequested();
        _riddles.Add(riddle);
        Remember(riddle);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    /// <exception cref="StaleRiddleWriteException">
    /// Thrown when <see cref="FailNextUpdateCount" /> is greater than zero. The stored riddle is left unchanged.
    /// </exception>
    public Task UpdateAsync(Riddle riddle, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(riddle);
        cancellationToken.ThrowIfCancellationRequested();

        if (FailNextUpdateCount > 0)
        {
            FailNextUpdateCount--;
            Restore(riddle.Id);
            throw new StaleRiddleWriteException();
        }

        ReplaceStored(riddle);
        Remember(riddle);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteAsync(Riddle riddle, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(riddle);
        cancellationToken.ThrowIfCancellationRequested();
        _riddles.RemoveAll(candidate => candidate.Id == riddle.Id);
        _committed.Remove(riddle.Id);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Riddle>> ListDueScheduledAsync(DateOnly localDate, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var matches = _riddles
            .Where(
                riddle => !riddle.IsLesson
                    && riddle.PublicationState == RiddlePublicationState.Scheduled
                    && riddle.SofiaPublicationDate is not null
                    && riddle.SofiaPublicationDate.Value <= localDate)
            .ToArray();
        return Task.FromResult<IReadOnlyList<Riddle>>(matches);
    }

    /// <inheritdoc />
    /// <exception cref="StaleRiddleWriteException">
    /// Thrown when <see cref="FailNextBatchUpdateCount" /> is greater than zero. No riddle in the batch is stored.
    /// </exception>
    public Task UpdateBatchAsync(IReadOnlyList<Riddle> riddles, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(riddles);
        cancellationToken.ThrowIfCancellationRequested();

        if (FailNextBatchUpdateCount > 0)
        {
            FailNextBatchUpdateCount--;
            foreach (var riddle in riddles)
            {
                Restore(riddle.Id);
            }

            throw new StaleRiddleWriteException();
        }

        foreach (var riddle in riddles)
        {
            ReplaceStored(riddle);
            Remember(riddle);
        }

        return Task.CompletedTask;
    }

    private void Remember(Riddle riddle)
    {
        _committed[riddle.Id] = riddle.Copy();
    }

    private void Restore(Guid id)
    {
        if (!_committed.TryGetValue(id, out var committed))
        {
            return;
        }

        var index = _riddles.FindIndex(riddle => riddle.Id == id);
        var restored = committed.Copy();
        if (index < 0)
        {
            _riddles.Add(restored);
            return;
        }

        _riddles[index] = restored;
    }

    private void ReplaceStored(Riddle riddle)
    {
        var index = _riddles.FindIndex(candidate => candidate.Id == riddle.Id);
        if (index < 0)
        {
            _riddles.Add(riddle);
            return;
        }

        _riddles[index] = riddle;
    }
}

using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Interfaces.Repositories;
using ProjectRiddle.Core.Models.Riddles;

namespace ProjectRiddle.IntegrationTests.Harness;

/// <summary>
/// Stores riddles in memory so Core domain tests do not depend on Infrastructure.
/// </summary>
public sealed class InMemoryRiddleRepository : IRiddleRepository
{
    private readonly List<Riddle> _riddles = [];

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
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task UpdateAsync(Riddle riddle, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(riddle);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteAsync(Riddle riddle, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(riddle);
        cancellationToken.ThrowIfCancellationRequested();
        _riddles.Remove(riddle);
        return Task.CompletedTask;
    }
}

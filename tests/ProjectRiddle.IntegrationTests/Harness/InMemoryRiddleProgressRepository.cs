using ProjectRiddle.Core.Interfaces.Repositories;
using ProjectRiddle.Core.Models.Riddles;

namespace ProjectRiddle.IntegrationTests.Harness;

/// <summary>
/// Stores account riddle progress in memory so Core domain tests do not depend on Infrastructure.
/// </summary>
public sealed class InMemoryRiddleProgressRepository : IRiddleProgressRepository
{
    private readonly List<RiddleProgress> _records = [];
    private readonly InMemoryRiddleRepository _riddles;

    /// <summary>
    /// Initializes the in-memory progress store.
    /// </summary>
    /// <param name="riddles">The riddle store used to resolve publication dates. Cannot be <see langword="null" />.</param>
    public InMemoryRiddleProgressRepository(InMemoryRiddleRepository riddles)
    {
        ArgumentNullException.ThrowIfNull(riddles);
        this._riddles = riddles;
    }

    /// <inheritdoc />
    public Task<RiddleProgress?> GetAsync(Guid accountId, Guid riddleId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(
            _records.SingleOrDefault(record => record.AccountId == accountId && record.RiddleId == riddleId));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RiddleProgress>> ListByAccountAndPublicationDateRangeAsync(
        Guid accountId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var riddles = await _riddles.ListPublishedBetweenAsync(fromDate, toDate, cancellationToken);
        var riddleIds = riddles.Select(riddle => riddle.Id).ToHashSet();
        return _records
            .Where(record => record.AccountId == accountId && riddleIds.Contains(record.RiddleId))
            .ToArray();
    }

    /// <inheritdoc />
    public Task AddAsync(RiddleProgress progress, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(progress);
        cancellationToken.ThrowIfCancellationRequested();
        _records.Add(progress);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task UpdateAsync(RiddleProgress progress, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(progress);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}

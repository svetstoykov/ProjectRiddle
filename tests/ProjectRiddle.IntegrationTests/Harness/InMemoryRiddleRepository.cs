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
        return Task.FromResult<IReadOnlyList<Riddle>>(_riddles.ToArray());
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

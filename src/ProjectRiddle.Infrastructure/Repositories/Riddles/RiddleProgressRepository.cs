using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Exceptions;
using ProjectRiddle.Core.Interfaces.Repositories;
using ProjectRiddle.Core.Models.Riddles;
using ProjectRiddle.Core.Models.Riddles.Progress;
using ProjectRiddle.Infrastructure.Persistence;

namespace ProjectRiddle.Infrastructure.Repositories.Riddles;

/// <summary>
/// Persists account riddle progress through EF Core.
/// </summary>
public sealed class RiddleProgressRepository : IRiddleProgressRepository
{
    private readonly ProjectRiddleDbContext _dbContext;

    /// <summary>
    /// Initializes the riddle progress repository.
    /// </summary>
    /// <param name="dbContext">The persistence context.</param>
    public RiddleProgressRepository(ProjectRiddleDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        this._dbContext = dbContext;
    }

    /// <inheritdoc />
    public Task<RiddleProgress?> GetAsync(Guid accountId, Guid riddleId, CancellationToken cancellationToken)
    {
        return _dbContext.Set<RiddleProgress>()
            .Include(progress => progress.Hints)
            .Include(progress => progress.Positions)
            .SingleOrDefaultAsync(
                progress => progress.AccountId == accountId && progress.RiddleId == riddleId,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RiddleProgress>> ListByAccountAndPublicationDateRangeAsync(
        Guid accountId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Set<RiddleProgress>()
            .Include(progress => progress.Hints)
            .Include(progress => progress.Positions)
            .AsNoTracking()
            .Where(
                progress => progress.AccountId == accountId
                    && _dbContext.Set<Riddle>().Any(
                        riddle => riddle.Id == progress.RiddleId
                            && riddle.PublicationState == RiddlePublicationState.Published
                            && riddle.SofiaPublicationDate != null
                            && riddle.SofiaPublicationDate >= fromDate
                            && riddle.SofiaPublicationDate <= toDate))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RiddleProgress>> ListByAccountAndRiddleIdsAsync(
        Guid accountId,
        IReadOnlyCollection<Guid> riddleIds,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(riddleIds);
        if (riddleIds.Count == 0)
        {
            return [];
        }

        return await _dbContext.Set<RiddleProgress>()
            .Include(progress => progress.Hints)
            .Include(progress => progress.Positions)
            .AsNoTracking()
            .Where(progress => progress.AccountId == accountId && riddleIds.Contains(progress.RiddleId))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(RiddleProgress progress, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(progress);
        _dbContext.Set<RiddleProgress>().Add(progress);
        await SaveProgressAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(RiddleProgress progress, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(progress);
        _dbContext.Set<RiddleProgress>().Update(progress);
        await SaveProgressAsync(cancellationToken);
    }

    private async Task SaveProgressAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueConstraint(exception))
        {
            throw new DuplicateRiddleProgressException();
        }
    }

    private static bool IsUniqueConstraint(DbUpdateException exception)
    {
        return exception.InnerException is SqliteException sqliteException
            && sqliteException.SqliteErrorCode == 19;
    }
}

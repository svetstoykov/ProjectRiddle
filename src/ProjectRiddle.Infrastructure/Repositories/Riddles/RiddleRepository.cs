using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Exceptions;
using ProjectRiddle.Core.Interfaces.Repositories;
using ProjectRiddle.Core.Models.Riddles;
using ProjectRiddle.Infrastructure.Persistence;

namespace ProjectRiddle.Infrastructure.Repositories.Riddles;

/// <summary>
/// Persists riddles through EF Core.
/// </summary>
public sealed class RiddleRepository : IRiddleRepository
{
    private readonly ProjectRiddleDbContext dbContext;

    /// <summary>
    /// Initializes the riddle repository.
    /// </summary>
    /// <param name="dbContext">The persistence context.</param>
    public RiddleRepository(ProjectRiddleDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public Task<Riddle?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Set<Riddle>()
            .Include(riddle => riddle.Ranges)
            .SingleOrDefaultAsync(riddle => riddle.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Riddle>> ListAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Set<Riddle>()
            .Include(riddle => riddle.Ranges)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<Riddle?> GetOccupyingByPublicationDateAsync(
        DateOnly publicationDate,
        CancellationToken cancellationToken)
    {
        return dbContext.Set<Riddle>()
            .Include(riddle => riddle.Ranges)
            .SingleOrDefaultAsync(
                riddle => riddle.SofiaPublicationDate == publicationDate
                    && (riddle.PublicationState == RiddlePublicationState.Scheduled
                        || riddle.PublicationState == RiddlePublicationState.Published),
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Riddle riddle, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(riddle);
        dbContext.Set<Riddle>().Add(riddle);
        await SaveOccupyingChangeAsync(riddle, cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Riddle riddle, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(riddle);
        dbContext.Set<Riddle>().Update(riddle);
        await SaveOccupyingChangeAsync(riddle, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Riddle riddle, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(riddle);
        dbContext.Set<Riddle>().Remove(riddle);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SaveOccupyingChangeAsync(Riddle riddle, CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueConstraint(exception) && riddle.SofiaPublicationDate is not null)
        {
            throw new DuplicatePublicationDateException(riddle.SofiaPublicationDate.Value);
        }
    }

    private static bool IsUniqueConstraint(DbUpdateException exception)
    {
        return exception.InnerException is SqliteException sqliteException
            && sqliteException.SqliteErrorCode == 19;
    }
}

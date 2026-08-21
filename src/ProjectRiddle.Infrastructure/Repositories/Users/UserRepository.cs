using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProjectRiddle.Core.Exceptions;
using ProjectRiddle.Core.Interfaces.Repositories;
using ProjectRiddle.Core.Models.Users;
using ProjectRiddle.Infrastructure.Persistence;

namespace ProjectRiddle.Infrastructure.Repositories.Users;

/// <summary>
/// Persists local accounts through EF Core.
/// </summary>
public sealed class UserRepository : IUserRepository
{
    private readonly ProjectRiddleDbContext dbContext;

    /// <summary>
    /// Initializes the user repository.
    /// </summary>
    /// <param name="dbContext">The persistence context.</param>
    public UserRepository(ProjectRiddleDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Set<User>().SingleOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public Task<User?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedEmail);
        return dbContext.Set<User>()
            .SingleOrDefaultAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        dbContext.Set<User>().Add(user);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueConstraint(exception))
        {
            throw new DuplicateNormalizedEmailException(user.NormalizedEmail);
        }
    }

    /// <inheritdoc />
    public async Task UpdateAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        dbContext.Set<User>().Update(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static bool IsUniqueConstraint(DbUpdateException exception)
    {
        return exception.InnerException is SqliteException sqliteException
            && sqliteException.SqliteErrorCode == 19;
    }
}

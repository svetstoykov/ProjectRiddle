using ProjectRiddle.Core.Interfaces.Services;
using ProjectRiddle.Core.Interfaces.Time;
using ProjectRiddle.Core.Models.System;
using ProjectRiddle.Core.Results.Models;

namespace ProjectRiddle.Core.Services.System;

/// <summary>
/// Provides the internal application status used by health endpoints.
/// </summary>
public sealed class InternalStatusService : IInternalStatusService
{
    private readonly IDateTimeProvider _dateTimeProvider;

    /// <summary>
    /// Initializes the internal status service.
    /// </summary>
    /// <param name="dateTimeProvider">The provider for the current UTC and local date-times.</param>
    public InternalStatusService(IDateTimeProvider dateTimeProvider)
    {
        ArgumentNullException.ThrowIfNull(dateTimeProvider);

        this._dateTimeProvider = dateTimeProvider;
    }

    /// <inheritdoc />
    public Task<Result<InternalStatusOutput>> GetAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var output = new InternalStatusOutput(
            "Project Riddle is healthy.",
            _dateTimeProvider.UtcDateTime,
            _dateTimeProvider.LocalDateTime);

        return Task.FromResult(Result.Success(output));
    }
}

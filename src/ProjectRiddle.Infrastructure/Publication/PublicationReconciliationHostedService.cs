using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProjectRiddle.Core.Interfaces.Services;
using ProjectRiddle.Core.Interfaces.Time;

namespace ProjectRiddle.Infrastructure.Publication;

/// <summary>
/// Reconciles scheduled publication at startup and at each configured-local midnight.
/// </summary>
/// <remarks>
/// <para>
/// The worker owns timing only. Each cycle creates a scope and calls
/// <see cref="IPublicationReconciliationService" />. It does not change stored riddles itself.
/// </para>
/// <para>
/// Startup reconciliation runs before the host accepts traffic. A failed cycle leaves the rows scheduled and is
/// retried after five minutes. After a successful cycle the worker waits until the next local midnight, calculated
/// again from UTC so a daylight-saving change is picked up.
/// </para>
/// </remarks>
public sealed class PublicationReconciliationHostedService : IHostedLifecycleService, IDisposable
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan MinimumDelay = TimeSpan.FromSeconds(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<PublicationReconciliationHostedService> _logger;
    private readonly CancellationTokenSource _stopping = new();
    private Task _loop = Task.CompletedTask;
    private bool _retryAfterFailure;
    private int _disposed;

    /// <summary>
    /// Initializes the publication reconciliation worker.
    /// </summary>
    /// <param name="scopeFactory">The factory used to resolve a scope for each reconciliation cycle.</param>
    /// <param name="dateTimeProvider">The clock used to calculate the next configured-local midnight.</param>
    /// <param name="logger">The logger for worker timing and unexpected failures.</param>
    public PublicationReconciliationHostedService(
        IServiceScopeFactory scopeFactory,
        IDateTimeProvider dateTimeProvider,
        ILogger<PublicationReconciliationHostedService> logger)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        ArgumentNullException.ThrowIfNull(logger);

        this._scopeFactory = scopeFactory;
        this._dateTimeProvider = dateTimeProvider;
        this._logger = logger;
    }

    /// <summary>
    /// Reconciles due schedules before the host accepts traffic.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel startup.</param>
    /// <returns>A task that represents the startup reconciliation.</returns>
    /// <exception cref="OperationCanceledException">Thrown when startup is cancelled.</exception>
    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        try
        {
            _retryAfterFailure = !await TryReconcileAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _retryAfterFailure = true;
            LogUnexpected(exception);
        }
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _loop = RunAsync(_stopping.Token);
        return Task.CompletedTask;
    }

    /// <summary>
    /// No work is required after the host has started.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the callback.</param>
    /// <returns>A completed task.</returns>
    public Task StartedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// No work is required before the worker stops.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the callback.</param>
    /// <returns>A completed task.</returns>
    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _stopping.CancelAsync();

        try
        {
            await _loop.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // The host stopped waiting. The loop still observes its own cancellation token.
        }
    }

    /// <summary>
    /// No work is required after the host has stopped.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the callback.</param>
    /// <returns>A completed task.</returns>
    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Cancels and releases the worker wait.
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        _stopping.Cancel();
        _stopping.Dispose();
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var delay = GetDelay();
            _logger.LogInformation(
                "Waiting for the next scheduled publication cycle. RetryAfterFailure: {RetryAfterFailure} DelayMs: {DelayMs}",
                _retryAfterFailure,
                (long)delay.TotalMilliseconds);

            try
            {
                await Task.Delay(delay, cancellationToken);
                _retryAfterFailure = !await TryReconcileAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _retryAfterFailure = true;
                LogUnexpected(exception);
            }
        }
    }

    private TimeSpan GetDelay()
    {
        if (_retryAfterFailure)
        {
            return RetryDelay;
        }

        var delay = _dateTimeProvider.NextLocalMidnightUtc - _dateTimeProvider.UtcDateTime;
        return delay < MinimumDelay ? MinimumDelay : delay;
    }

    private async Task<bool> TryReconcileAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IPublicationReconciliationService>();
        var result = await service.ReconcileAsync(cancellationToken);
        return result.IsSuccess;
    }

    private void LogUnexpected(Exception exception)
    {
        _logger.LogError(
            exception,
            "Scheduled publication reconciliation failed unexpectedly. FailureCategory: {FailureCategory}",
            exception.GetType().Name);
    }
}

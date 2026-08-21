using ProjectRiddle.Core.Interfaces.Time;

namespace ProjectRiddle.Infrastructure.Time;

/// <summary>
/// Provides the system UTC clock to the application.
/// </summary>
public sealed class SystemClock : IClock
{
    /// <inheritdoc />
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

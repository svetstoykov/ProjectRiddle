namespace ProjectRiddle.Core.Interfaces.Time;

/// <summary>
/// Provides the current publication date in the configured application time zone.
/// </summary>
public interface IPublicationDateProvider
{
    /// <summary>
    /// Gets the current publication date.
    /// </summary>
    DateOnly CurrentDate { get; }
}

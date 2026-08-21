namespace ProjectRiddle.Core.Interfaces.Accounts;

/// <summary>
/// Provides the provider-neutral identifier of the current caller.
/// </summary>
public interface ICurrentAccount
{
    /// <summary>
    /// Gets the stable account identifier when the caller is authenticated; otherwise <see langword="null" />.
    /// </summary>
    Guid? AccountId { get; }
}

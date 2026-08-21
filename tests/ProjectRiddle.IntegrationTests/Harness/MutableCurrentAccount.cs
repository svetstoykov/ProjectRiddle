using ProjectRiddle.Core.Interfaces.Accounts;

namespace ProjectRiddle.IntegrationTests.Harness;

/// <summary>
/// Provides a controllable current-account identity for Core tests.
/// </summary>
public sealed class MutableCurrentAccount : ICurrentAccount
{
    /// <summary>
    /// Initializes the current-account double.
    /// </summary>
    /// <param name="accountId">The account identifier, or <see langword="null" /> for an anonymous caller.</param>
    public MutableCurrentAccount(Guid? accountId)
    {
        AccountId = accountId;
    }

    /// <inheritdoc />
    public Guid? AccountId { get; set; }
}

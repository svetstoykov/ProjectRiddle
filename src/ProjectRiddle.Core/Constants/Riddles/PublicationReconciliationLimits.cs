namespace ProjectRiddle.Core.Constants.Riddles;

/// <summary>
/// Provides bounds for automatic scheduled-publication reconciliation.
/// </summary>
public static class PublicationReconciliationLimits
{
    /// <summary>
    /// The maximum number of attempts to persist one reconciliation batch after a stale write.
    /// </summary>
    public const int WriteRetryLimit = 3;
}

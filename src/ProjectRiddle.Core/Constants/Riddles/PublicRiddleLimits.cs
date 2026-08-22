namespace ProjectRiddle.Core.Constants.Riddles;

/// <summary>
/// Provides bounded defaults for public riddle discovery and progress reads.
/// </summary>
public static class PublicRiddleLimits
{
    /// <summary>
    /// The default one-based archive page number.
    /// </summary>
    public const int DefaultPage = 1;

    /// <summary>
    /// The default archive page size.
    /// </summary>
    public const int DefaultPageSize = 31;

    /// <summary>
    /// The maximum archive page size.
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// The maximum inclusive length of an account progress date range in days.
    /// </summary>
    public const int MaxProgressRangeDays = 366;

    /// <summary>
    /// The maximum number of Unicode characters included in a public clue excerpt.
    /// </summary>
    public const int ClueExcerptMaxChars = 80;

    /// <summary>
    /// The supported anonymous progress schema version.
    /// </summary>
    public const int AnonymousProgressSchemaVersion = 1;

    /// <summary>
    /// The maximum number of persistence retries for a conflicting progress write.
    /// </summary>
    public const int ProgressWriteRetryLimit = 5;
}

using ProjectRiddle.Core.Models.Riddles.Progress;

namespace ProjectRiddle.Api.Models.Riddles.Progress;

/// <summary>
/// Represents account-owned riddle progress for a date range.
/// </summary>
public sealed record AccountRiddleProgressListResponse
{
    /// <summary>
    /// Gets the progress snapshots.
    /// </summary>
    public required IReadOnlyList<RiddleProgressSnapshotResponse> Items { get; init; }

    /// <summary>
    /// Maps a Core progress list to the API response.
    /// </summary>
    /// <param name="output">The Core output. Cannot be <see langword="null" />.</param>
    /// <returns>The corresponding API response.</returns>
    public static AccountRiddleProgressListResponse FromCoreAccountRiddleProgressListOutput(
        AccountRiddleProgressListOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        return new AccountRiddleProgressListResponse
        {
            Items = output.Items.Select(RiddleProgressSnapshotResponse.FromCoreRiddleProgressSnapshotOutput).ToArray()
        };
    }
}

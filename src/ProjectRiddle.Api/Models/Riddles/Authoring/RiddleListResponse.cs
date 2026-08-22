using ProjectRiddle.Core.Models.Riddles.Authoring;

namespace ProjectRiddle.Api.Models.Riddles.Authoring;

/// <summary>
/// Represents the administrative list of riddles.
/// </summary>
public sealed record RiddleListResponse
{
    /// <summary>
    /// Gets the riddles in list order.
    /// </summary>
    public required IReadOnlyList<RiddleResponse> Riddles { get; init; }

    /// <summary>
    /// Maps a Core list output to the API response.
    /// </summary>
    /// <param name="output">The Core output. Cannot be <see langword="null" />.</param>
    /// <returns>The corresponding API response.</returns>
    public static RiddleListResponse FromCoreListRiddlesOutput(ListRiddlesOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);

        return new RiddleListResponse
        {
            Riddles = output.Riddles.Select(RiddleResponse.FromCoreRiddleOutput).ToArray()
        };
    }
}

using ProjectRiddle.Core.Enums.Riddles;

namespace ProjectRiddle.Core.Models.Riddles;

/// <summary>
/// Represents one structural hint kind recorded on a riddle progress snapshot.
/// </summary>
public sealed class RiddleProgressHint
{
    /// <summary>
    /// Initializes a recorded hint kind.
    /// </summary>
    /// <param name="kind">The structural hint kind.</param>
    public RiddleProgressHint(RiddleRangeKind kind)
    {
        Kind = kind;
    }

    /// <summary>
    /// Gets the structural hint kind.
    /// </summary>
    public RiddleRangeKind Kind { get; }
}

namespace ProjectRiddle.Core.Enums.Riddles;

/// <summary>
/// Defines the monotonic play status of a riddle progress snapshot.
/// </summary>
public enum RiddleProgressStatus
{
    /// <summary>
    /// Indicates that the riddle is still being played.
    /// </summary>
    InProgress = 0,

    /// <summary>
    /// Indicates that every letter position has been revealed without a correct solve.
    /// </summary>
    FullyRevealed = 1,

    /// <summary>
    /// Indicates that the riddle was solved with a correct answer.
    /// </summary>
    Solved = 2
}

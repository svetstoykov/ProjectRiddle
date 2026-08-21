namespace ProjectRiddle.Core.Enums.Riddles;

/// <summary>
/// Defines the structural roles a labelled clue range can play.
/// </summary>
public enum RiddleRangeKind
{
    /// <summary>
    /// Indicates the definition portion of the clue.
    /// </summary>
    Definition = 0,

    /// <summary>
    /// Indicates a wordplay indicator in the clue.
    /// </summary>
    Indicator = 1,

    /// <summary>
    /// Indicates fodder used by the wordplay.
    /// </summary>
    Fodder = 2
}

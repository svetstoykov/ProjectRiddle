namespace ProjectRiddle.Core.Enums.Riddles;

/// <summary>
/// Defines the publication lifecycle of a riddle.
/// </summary>
public enum RiddlePublicationState
{
    /// <summary>
    /// Indicates a riddle that has not been scheduled or published.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Indicates a riddle reserved for a Sofia calendar date that is not yet published.
    /// </summary>
    Scheduled = 1,

    /// <summary>
    /// Indicates a riddle occupying a Sofia calendar date as published content.
    /// </summary>
    Published = 2,

    /// <summary>
    /// Indicates a riddle withdrawn from the calendar so its date may be reused.
    /// </summary>
    Unpublished = 3,

    /// <summary>
    /// Indicates a scheduled riddle whose date was missed because a later due schedule was published instead.
    /// </summary>
    /// <remarks>
    /// The missed Sofia date is retained. The riddle does not occupy that date, so another riddle may be scheduled
    /// there. An expired riddle can be rescheduled for a current or future date and cannot be published directly.
    /// </remarks>
    Expired = 4
}

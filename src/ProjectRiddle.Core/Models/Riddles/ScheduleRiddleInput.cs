namespace ProjectRiddle.Core.Models.Riddles;

/// <summary>
/// Represents the input required to schedule a riddle on a Sofia calendar date.
/// </summary>
/// <param name="Id">The riddle identifier.</param>
/// <param name="PublicationDate">The Sofia calendar date that should occupy the calendar.</param>
public sealed record ScheduleRiddleInput(Guid Id, DateOnly PublicationDate);

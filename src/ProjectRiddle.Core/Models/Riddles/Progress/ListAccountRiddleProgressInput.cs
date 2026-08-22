namespace ProjectRiddle.Core.Models.Riddles.Progress;

/// <summary>
/// Represents a bounded local-date range for account riddle progress.
/// </summary>
/// <param name="FromDate">The inclusive start local date.</param>
/// <param name="ToDate">The inclusive end local date.</param>
public sealed record ListAccountRiddleProgressInput(DateOnly FromDate, DateOnly ToDate);

namespace ProjectRiddle.Core.Validators.Riddles;

/// <summary>
/// Calculates the Monday-through-Sunday local week that contains a date.
/// </summary>
public static class LocalCalendarWeek
{
    /// <summary>
    /// Gets the Monday and Sunday of the local week that contains <paramref name="localDate" />.
    /// </summary>
    /// <param name="localDate">The local calendar date.</param>
    /// <returns>The inclusive Monday and Sunday of that week.</returns>
    public static (DateOnly Monday, DateOnly Sunday) Containing(DateOnly localDate)
    {
        var offset = ((int)localDate.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var monday = localDate.AddDays(-offset);
        return (monday, monday.AddDays(6));
    }
}

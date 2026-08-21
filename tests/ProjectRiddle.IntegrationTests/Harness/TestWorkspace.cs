using Microsoft.Extensions.Logging.Abstractions;
using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Interfaces.Services;
using ProjectRiddle.Core.Models.Riddles;
using ProjectRiddle.Core.Services.Riddles;

namespace ProjectRiddle.IntegrationTests.Harness;

/// <summary>
/// Owns Core riddle collaborators for one domain test.
/// </summary>
public sealed class TestWorkspace
{
    /// <summary>
    /// Identifies the application time zone used by tests.
    /// </summary>
    public const string TimeZoneId = "Europe/Sofia";

    /// <summary>
    /// Initializes a Core riddle test workspace.
    /// </summary>
    /// <param name="utcNow">The fixed UTC instant.</param>
    public TestWorkspace(DateTimeOffset utcNow)
    {
        Clock = new FixedDateTimeProvider(utcNow, TimeZoneId);
        Service = new RiddlesService(
            new InMemoryRiddleRepository(),
            Clock,
            NullLogger<RiddlesService>.Instance);
    }

    /// <summary>
    /// Gets the controllable clock used by the service.
    /// </summary>
    public FixedDateTimeProvider Clock { get; }

    /// <summary>
    /// Gets the Core riddles service under test.
    /// </summary>
    public IRiddlesService Service { get; }

    /// <summary>
    /// Creates a valid riddle create input.
    /// </summary>
    /// <param name="clue">The clue text.</param>
    /// <param name="answer">The answer text.</param>
    /// <param name="answerPattern">The answer pattern.</param>
    /// <returns>The create input.</returns>
    public static CreateRiddleInput CreateRiddleInput(
        string clue = "бяла врана лети високо",
        string answer = "бяла врана",
        string answerPattern = "4,5")
    {
        var ranges = new[]
        {
            new RiddleRangeInput(RiddleRangeKind.Definition, 0, 4),
            new RiddleRangeInput(RiddleRangeKind.Fodder, 5, 10)
        };

        return new CreateRiddleInput(clue, answer, answerPattern, "Обяснение на уликата.", ranges);
    }
}

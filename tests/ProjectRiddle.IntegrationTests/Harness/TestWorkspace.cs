using Microsoft.Extensions.Logging.Abstractions;
using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Interfaces.Randomness;
using ProjectRiddle.Core.Interfaces.Services;
using ProjectRiddle.Core.Models.Riddles.Authoring;
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
    /// <param name="accountId">The current account identifier, or <see langword="null" /> for an anonymous caller.</param>
    /// <param name="randomNumberGenerator">The optional scripted random source for letter reveals.</param>
    public TestWorkspace(
        DateTimeOffset utcNow,
        Guid? accountId = null,
        IRandomNumberGenerator? randomNumberGenerator = null)
    {
        Clock = new FixedDateTimeProvider(utcNow, TimeZoneId);
        Account = new MutableCurrentAccount(accountId);
        var riddles = new InMemoryRiddleRepository();
        AdminService = new AdminRiddlesService(riddles, Clock, NullLogger<AdminRiddlesService>.Instance);
        Service = new RiddlesService(
            riddles,
            new InMemoryRiddleProgressRepository(riddles),
            Account,
            Clock,
            randomNumberGenerator ?? new ScriptedRandomNumberGenerator(),
            NullLogger<RiddlesService>.Instance);
    }

    /// <summary>
    /// Gets the controllable clock used by the service.
    /// </summary>
    public FixedDateTimeProvider Clock { get; }

    /// <summary>
    /// Gets the controllable current-account identity.
    /// </summary>
    public MutableCurrentAccount Account { get; }

    /// <summary>
    /// Gets the Core administrative riddles service under test.
    /// </summary>
    public IAdminRiddlesService AdminService { get; }

    /// <summary>
    /// Gets the Core riddles service under test.
    /// </summary>
    public IRiddlesService Service { get; }

    /// <summary>
    /// Creates a valid riddle create input.
    /// </summary>
    /// <param name="clue">The clue text.</param>
    /// <param name="answer">The answer text.</param>
    /// <returns>The create input.</returns>
    public static CreateRiddleInput CreateRiddleInput(
        string clue = "бяла врана лети високо",
        string answer = "бяла врана")
    {
        var ranges = new[]
        {
            new RiddleRangeInput(RiddleRangeKind.Definition, 0, 4),
            new RiddleRangeInput(RiddleRangeKind.Fodder, 5, 10)
        };

        return new CreateRiddleInput(clue, answer, "Обяснение на уликата.", ranges);
    }
}

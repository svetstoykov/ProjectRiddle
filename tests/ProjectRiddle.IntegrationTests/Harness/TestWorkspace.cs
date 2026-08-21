using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using ProjectRiddle.Api.Models.Auth;
using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.Core.Interfaces.Services;
using ProjectRiddle.Core.Models.Riddles;

namespace ProjectRiddle.IntegrationTests.Harness;

/// <summary>
/// Owns a disposable SQLite database and application host for one integration test.
/// </summary>
public sealed class TestWorkspace : IAsyncDisposable
{
    /// <summary>
    /// Identifies the application time zone used by tests.
    /// </summary>
    public const string TimeZoneId = "Europe/Sofia";

    /// <summary>
    /// Gets the JSON options matching the API contract.
    /// </summary>
    public static JsonSerializerOptions JsonOptions { get; } = CreateJsonOptions();

    private TestWorkspace(DirectoryInfo directory, ApplicationFactory factory)
    {
        Directory = directory;
        Factory = factory;
    }

    /// <summary>
    /// Gets the temporary directory that holds the database.
    /// </summary>
    public DirectoryInfo Directory { get; }

    /// <summary>
    /// Gets the application host factory.
    /// </summary>
    public ApplicationFactory Factory { get; }

    /// <summary>
    /// Creates a workspace with a unique disposable database.
    /// </summary>
    /// <param name="utcNow">The optional fixed UTC instant.</param>
    /// <param name="bootstrapEmail">The optional bootstrap administrator email.</param>
    /// <param name="bootstrapPassword">The optional bootstrap administrator password.</param>
    /// <returns>The created workspace.</returns>
    public static TestWorkspace Create(
        DateTimeOffset? utcNow = null,
        string? bootstrapEmail = null,
        string? bootstrapPassword = null)
    {
        var directory = System.IO.Directory.CreateTempSubdirectory("project-riddle-");
        var databasePath = Path.Combine(directory.FullName, "project-riddle.db");
        var factory = new ApplicationFactory(
            databasePath,
            TimeZoneId,
            utcNow,
            bootstrapEmail,
            bootstrapPassword);

        return new TestWorkspace(directory, factory);
    }

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

    /// <summary>
    /// Resolves a scoped Core service from the test host.
    /// </summary>
    /// <typeparam name="TService">The service type to resolve.</typeparam>
    /// <returns>The scoped service and a scope that must be disposed.</returns>
    public (IServiceScope Scope, TService Service) GetScopedService<TService>()
        where TService : notnull
    {
        var scope = Factory.Services.CreateScope();
        return (scope, scope.ServiceProvider.GetRequiredService<TService>());
    }

    /// <summary>
    /// Creates an HTTP client and stores a CSRF token header.
    /// </summary>
    /// <returns>The configured client.</returns>
    public async Task<HttpClient> CreateClientWithAntiforgeryAsync()
    {
        var client = Factory.CreateClient();
        await RefreshAntiforgeryAsync(client);
        return client;
    }

    /// <summary>
    /// Reads a new CSRF token and stores it on the client.
    /// </summary>
    /// <param name="client">The HTTP client.</param>
    /// <returns>A task that represents the operation.</returns>
    public static async Task RefreshAntiforgeryAsync(HttpClient client)
    {
        ArgumentNullException.ThrowIfNull(client);

        var response = await client.GetFromJsonAsync<AntiforgeryTokenResponse>(
            "/api/auth/antiforgery",
            JsonOptions);
        Assert.NotNull(response);
        client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", response.Token);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>
    /// Registers an account and signs it in on the supplied client.
    /// </summary>
    /// <param name="client">The HTTP client.</param>
    /// <param name="email">The account email.</param>
    /// <param name="password">The account password.</param>
    /// <returns>The session returned by sign-in.</returns>
    public static async Task<SessionResponse> RegisterAndSignInAsync(
        HttpClient client,
        string email,
        string password)
    {
        ArgumentNullException.ThrowIfNull(client);

        var registerResponse = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest { Email = email, Password = password },
            JsonOptions);
        registerResponse.EnsureSuccessStatusCode();

        var signInResponse = await client.PostAsJsonAsync(
            "/api/auth/sign-in",
            new SignInRequest { Email = email, Password = password },
            JsonOptions);
        signInResponse.EnsureSuccessStatusCode();
        await RefreshAntiforgeryAsync(client);

        var session = await signInResponse.Content.ReadFromJsonAsync<SessionResponse>(JsonOptions);
        Assert.NotNull(session);
        return session;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await Factory.DisposeAsync();
        System.IO.Directory.Delete(Directory.FullName, recursive: true);
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return options;
    }
}

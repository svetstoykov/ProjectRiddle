using System.Net;
using System.Net.Http.Json;
using ProjectRiddle.Api.Models.Auth;
using ProjectRiddle.Api.Models.Riddles;
using ProjectRiddle.Core.Enums.Riddles;
using ProjectRiddle.IntegrationTests.Harness;

namespace ProjectRiddle.IntegrationTests.Riddles;

/// <summary>
/// Verifies that administrative riddle operations require an administrator session.
/// </summary>
public sealed class AdminRiddleAuthorizationTests
{
    /// <summary>
    /// Verifies that an anonymous caller receives 401 for administrative operations.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task AnonymousCallerCannotUseAdministrativeOperations()
    {
        await using var workspace = TestWorkspace.Create();
        using var client = workspace.Factory.CreateClient();

        var list = await client.GetAsync("/api/riddles");
        var create = await client.PostAsJsonAsync(
            "/api/riddles",
            new CreateRiddleRequest
            {
                Clue = "бяла врана лети високо",
                Answer = "бяла врана",
                AnswerPattern = "4,5",
                Explanation = "Обяснение на уликата."
            },
            TestWorkspace.JsonOptions);

        Assert.Equal(HttpStatusCode.Unauthorized, list.StatusCode);
        Assert.Equal("application/problem+json", list.Content.Headers.ContentType?.MediaType);
        Assert.Equal(HttpStatusCode.Unauthorized, create.StatusCode);
        Assert.Equal("application/problem+json", create.Content.Headers.ContentType?.MediaType);
    }

    /// <summary>
    /// Verifies that a signed-in non-admin caller receives 403 for administrative operations.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task NonAdminCallerCannotUseAdministrativeOperations()
    {
        await using var workspace = TestWorkspace.Create();
        using var client = await workspace.CreateClientWithAntiforgeryAsync();
        await TestWorkspace.RegisterAndSignInAsync(client, "visitor@example.com", "password1");

        var list = await client.GetAsync("/api/riddles");
        var create = await client.PostAsJsonAsync(
            "/api/riddles",
            new CreateRiddleRequest
            {
                Clue = "бяла врана лети високо",
                Answer = "бяла врана",
                AnswerPattern = "4,5",
                Explanation = "Обяснение на уликата."
            },
            TestWorkspace.JsonOptions);

        Assert.Equal(HttpStatusCode.Forbidden, list.StatusCode);
        Assert.Equal("application/problem+json", list.Content.Headers.ContentType?.MediaType);
        Assert.Equal(HttpStatusCode.Forbidden, create.StatusCode);
        Assert.Equal("application/problem+json", create.Content.Headers.ContentType?.MediaType);
    }

    /// <summary>
    /// Verifies that an administrator can create and read a riddle through the API.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task AdministratorCanCreateAndReadARiddle()
    {
        await using var workspace = TestWorkspace.Create(
            bootstrapEmail: "admin@example.com",
            bootstrapPassword: "password1");
        using var client = await workspace.CreateClientWithAntiforgeryAsync();

        var signIn = await client.PostAsJsonAsync(
            "/api/auth/sign-in",
            new SignInRequest { Email = "admin@example.com", Password = "password1" },
            TestWorkspace.JsonOptions);
        Assert.Equal(HttpStatusCode.OK, signIn.StatusCode);
        await TestWorkspace.RefreshAntiforgeryAsync(client);

        var create = await client.PostAsJsonAsync(
            "/api/riddles",
            new CreateRiddleRequest
            {
                Clue = "бяла врана лети високо",
                Answer = "бяла врана",
                AnswerPattern = "4,5",
                Explanation = "Обяснение на уликата.",
                Ranges =
                [
                    new RiddleRangeRequest { Kind = RiddleRangeKind.Definition, Start = 0, End = 4 }
                ]
            },
            TestWorkspace.JsonOptions);

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<RiddleResponse>(TestWorkspace.JsonOptions);
        Assert.NotNull(created);
        Assert.Equal("бяла врана", created.Answer);
        Assert.Equal(RiddlePublicationState.Draft, created.PublicationState);

        var read = await client.GetAsync($"/api/riddles/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, read.StatusCode);
        var loaded = await read.Content.ReadFromJsonAsync<RiddleResponse>(TestWorkspace.JsonOptions);
        Assert.NotNull(loaded);
        Assert.Equal(created.Id, loaded.Id);
        Assert.Equal("бяла врана", loaded.Answer);
    }
}
